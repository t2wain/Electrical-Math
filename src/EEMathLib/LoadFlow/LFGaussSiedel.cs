using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;
using EEMathLib.DTO;

namespace EEMathLib.LoadFlow
{
    #region DTO

    public class BusResult
    {
        public class ErrVal
        {
            public double VErr { get; set; }
            public double AErr { get; set; }
            public double PErr { get; set; }
            public double QErr { get; set; }
        }

        public BusResult()
        {
            Err = new ErrVal();
            ErrRef = new ErrVal();
        }
        public EEBus BusData { get; set; }
        public int BusIndex { get; set; }
        public string ID { get; set; }
        public BusTypeEnum BusType { get; set; }
        public Complex BusVoltage { get; set; }
        public Complex Sbus { get; set; }
        public ErrVal Err { get; set; }
        public ErrVal ErrRef { get; set; }
    }

    #endregion

    public static class LFGaussSiedel
    {

        public static Result<IEnumerable<BusResult>> Solve(EENetwork network,
            double threshold = 0.015, int maxIteration = 100, int minIteration = 50) =>
            Solve(Initialize(network.Buses), network.YMatrix, threshold, maxIteration, minIteration);

        public static Result<IEnumerable<BusResult>> Solve(IEnumerable<BusResult> buses, Matrix<Complex> YMatrix, 
            double threshold = 0.015, int maxIteration = 100, int minIteration = 50)
        {
            var Y = YMatrix;

            #region Iteration

            BusResult slackBus = null;
            var i = 0;
            var isFound = false;
            while (i++ < maxIteration)
            {
                // calculate Vk, Ak, Qk for each bus
                foreach (var bus in buses)
                {
                    #region Calulate Bus

                    // load bus
                    if (bus.BusData.BusType == BusTypeEnum.PQ)
                    {
                        var vnxt = CalcVoltage(bus, Y, buses);
                        UpdateVErr(bus, bus.BusVoltage.Magnitude, vnxt.Magnitude, i);
                        UpdateAErr(bus, bus.BusVoltage.Phase, vnxt.Phase, i);
                        // update Vk, Ak
                        bus.BusVoltage = vnxt;
                    }

                    // voltage controlled bus
                    else if (bus.BusData.BusType == BusTypeEnum.PV)
                    {
                        // calculate Qk
                        var sk = CalcPower(bus, Y, buses);
                        UpdatePErr(bus, bus.Sbus.Real, sk.Real, i);

                        var (snxt, bt) = CalcMaxQk(bus, sk);
                        UpdateQErr(bus, bus.Sbus.Imaginary, snxt.Imaginary, i, bt != bus.BusType);
                        bus.BusType = bt;
                        // update Sbus
                        bus.Sbus = snxt;

                        // calculate Vbus
                        var vnxt = CalcVoltage(bus, Y, buses);
                        UpdateAErr(bus, bus.BusVoltage.Phase, vnxt.Phase, i);

                        if (bt == BusTypeEnum.PV)
                        {
                            // maintain bus controlled voltage and update Ak
                            bus.BusVoltage = Complex.FromPolarCoordinates(bus.BusData.Voltage, vnxt.Phase);
                        }
                        else
                        {
                            // required switching to PQ bus
                            // since Qk is outside limits of Qgen range (min and max)
                            bus.BusType = BusTypeEnum.PQ;
                            UpdateVErr(bus, bus.BusVoltage.Magnitude, vnxt.Magnitude, i);
                            // update Vk, Ak
                            bus.BusVoltage = vnxt;
                        }
                    }

                    // save slack bus for later calculation
                    else if (bus.BusData.BusType == BusTypeEnum.Slack)
                    {
                        slackBus = bus;
                    }

                    #endregion
                }

                #region Check for solution

                if (i < minIteration)
                    continue;
                else if (IsSolutionFound(buses, threshold))
                {
                    isFound = true;
                    break;
                }
                else if (IsDiverged(buses, i))
                {
                    return new Result<IEnumerable<BusResult>>
                    {
                        Data = buses,
                        IterationStop = i,
                        Error = ErrorEnum.Divergence,
                        ErrorMessage = "Divergence detected during Gauss-Siedel iterations."
                    };
                }

                #endregion
            }

            #endregion

            // Calculate Pk, Qk for slack bus
            slackBus.Sbus = CalcPower(slackBus, Y, buses);

            return new Result<IEnumerable<BusResult>>
            {
                Data = buses,
                IterationStop = i,
                Error = isFound ? ErrorEnum.NoError : ErrorEnum.MaxIteration,
                ErrorMessage = isFound ? "" : "Maximum iterations reached without convergence."
            };
        }

        #region Calculation

        public static IEnumerable<BusResult> Initialize(IEnumerable<EEBus> buses) =>
            buses.Select(b => new BusResult
            {
                BusData = b,
                BusIndex = b.BusIndex,
                ID = b.ID,
                BusType = b.BusType,
                BusVoltage = new Complex(b.Voltage > 0 ? b.Voltage : 1.0, 0),
                Sbus = new Complex(-b.Pload, -b.Qload)
            })
            .ToList();

        /// <summary>
        /// Calculate Vk, Ak given Pk, Qk for bus k.
        /// </summary>
        /// <returns>Voltage Sk</returns>
        public static Complex CalcVoltage(BusResult bus, Matrix<Complex> Y, IEnumerable<BusResult> buses)
        {
            var k = bus.BusIndex;
            var yk = Y[k, k];
            var sk = bus.Sbus; // given Pk, Qk for bus k
            var vk = bus.BusVoltage; // calculated voltage from previous iteration
            var sv = buses
                .Where(b => b.BusIndex != k)
                .Select(b =>
                {
                    var idx = b.BusIndex;
                    var yb = Y[k, idx];
                    return yb * b.BusVoltage;
                })
                .Aggregate((v1, v2) => v1 + v2);

            // calculate next voltage value
            var skConj = sk.Conjugate();
            var vknxt = 1 / yk * (skConj / vk.Conjugate() - sv); // using vk
            var vknxt2 = 1 / yk * (skConj / vknxt.Conjugate() - sv); // using vknxt
            return vknxt2;
        }

        /// <summary>
        /// Calculate Sk for bus k.
        /// </summary>
        /// <returns>Apparent power Sk</returns>
        public static Complex CalcPower(BusResult bus, Matrix<Complex> Y, IEnumerable<BusResult> buses)
        {
            var vk = bus.BusVoltage; // given bus voltage
            var sv = buses
                .Select(b =>
                {
                    var idx = b.BusIndex;
                    var yb = Y[bus.BusIndex, idx];
                    return yb * b.BusVoltage;
                })
                .Aggregate((v1, v2) => v1 + v2);
            var sk = vk * sv.Conjugate();
            return sk;
        }

        /// <summary>
        /// Calculate Qk for given Sk based on Qgen limits.
        /// </summary>
        /// <returns>Tuples of Sk and the bus type</returns>
        public static (Complex SBus, BusTypeEnum BustType) CalcMaxQk(BusResult bus, Complex sk)
        {
            var qk = sk.Imaginary;

            // required Qgen to maintain given bus voltage
            var qgen = qk + bus.BusData.Qload;
            var pk = bus.BusData.Pgen - bus.BusData.Pload;

            // calculate Sbus
            if (qgen < bus.BusData.Qmin)
                // use definded Qgen min and change to PQ bus
                return (new Complex(pk, bus.BusData.Qmin + bus.BusData.Qload), BusTypeEnum.PQ);
            else if (qgen > bus.BusData.Qmax)
                // use definded Qgen max and change to PQ bus
                return (new Complex(pk, bus.BusData.Qmax - bus.BusData.Qload), BusTypeEnum.PQ);
            else
                // required Qgen is within limits to maintain bus voltage
                return (new Complex(pk, sk.Imaginary), BusTypeEnum.PV);
        }

        public static IEnumerable<EEBus> CalcResult(IEnumerable<BusResult> buses) =>
            buses.Select(b => new EEBus
            {
                BusIndex = b.BusIndex,
                ID = b.ID,
                BusType = b.BusType,
                Voltage = b.BusVoltage.Magnitude,
                Angle = Phasor.ConvertRadianToDegree(b.BusVoltage.Phase),
                Pgen = b.Sbus.Real + b.BusData.Pload,
                Qgen = b.Sbus.Imaginary + b.BusData.Qload,
                Pload = b.BusData.Pload,
                Qload = b.BusData.Qload,
                Qmin = b.BusData.Qmin,
                Qmax = b.BusData.Qmax
            })
            .ToList();

        #endregion

        #region Track error and convergence

        static void UpdateVErr(BusResult bus, double vcur, double vnxt, int iteration)
        {
            bus.Err.VErr = Math.Abs(vnxt - vcur);
            if (iteration == 1 || iteration % 6 == 0)
                bus.ErrRef.VErr = bus.Err.VErr;
        }

        static void UpdateAErr(BusResult bus, double acur, double anext, int iteration)
        {
            bus.Err.AErr = Math.Abs(anext - acur);
            if (iteration == 1 || iteration % 6 == 0)
                bus.ErrRef.AErr = bus.Err.AErr;
        }

        static void UpdatePErr(BusResult bus, double pcur, double pnext, int iteration)
        {
            bus.Err.PErr = Math.Abs(pnext - pcur);
            if (iteration == 1 || iteration % 6 == 0)
                bus.ErrRef.PErr = bus.Err.PErr;
        }

        static void UpdateQErr(BusResult bus, double qcur, double qnext, int iteration, bool setRef)
        {
            bus.Err.QErr = Math.Abs(qnext - qcur);
            if (setRef)
                bus.Err.QErr = 0;
            if (iteration == 1 || iteration % 6 == 0 || setRef)
                bus.ErrRef.QErr = bus.Err.QErr;
        }

        static bool IsSolutionFound(IEnumerable<BusResult> buses, double threshold)
        {
            var cv = true;
            foreach (var bus in buses.Where(b => b.BusType != BusTypeEnum.Slack))
            {
                cv = cv && Math.Abs(bus.BusVoltage.Magnitude) > 0
                    && Math.Abs(bus.Err.VErr / bus.BusVoltage.Magnitude) < threshold;
                cv = cv && Math.Abs(bus.BusVoltage.Phase) > 0
                    && Math.Abs(bus.Err.AErr / bus.BusVoltage.Phase) < (0.1 * threshold);
                if (bus.BusData.BusType == BusTypeEnum.PV)
                {
                    cv = cv && Math.Abs(bus.Sbus.Imaginary) > 0
                        && Math.Abs(bus.Err.QErr / bus.Sbus.Imaginary) < threshold;
                }
            }
            return cv;
        }

        static bool IsDiverged(IEnumerable<BusResult> buses, int iteration)
        {
            if (iteration % 5 != 0)
                return false;

            var cv = true;
            foreach (var bus in buses.Where(b => b.BusType != BusTypeEnum.Slack))
            {
                cv = cv && (bus.Err.VErr < 0.1 || bus.ErrRef.VErr < 0.1 || bus.Err.VErr < bus.ErrRef.VErr * 3);
                cv = cv && (bus.Err.AErr < 0.1 || bus.ErrRef.AErr < 0.1 || bus.Err.AErr < bus.ErrRef.AErr * 3);
                if (bus.BusData.BusType == BusTypeEnum.PV)
                    cv = cv && (bus.Err.QErr < 0.1 || bus.ErrRef.QErr < 0.1 || bus.Err.QErr < bus.ErrRef.QErr * 3);
            }
            return !cv;
        }

        #endregion
    }
}
