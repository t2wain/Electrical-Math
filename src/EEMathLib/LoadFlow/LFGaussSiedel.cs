using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;
using EEMathLib.DTO;

namespace EEMathLib.LoadFlow
{
    public static class LFGaussSiedel
    {
        #region DTO

        public class ErrVal
        {
            public double VErr { get; set; }
            public double AErr { get; set; }
            public double QErr { get; set; }
        }

        public class BusResult
        {
            public BusResult()
            {
                Err = new ErrVal();
                ErrRef = new ErrVal();
            }
            public EEBus BusData { get; set; }
            public int BusIndex => BusData.BusIndex;
            public BusTypeEnum BusType { get; set; }
            public Complex BusVoltage { get; set; }
            public Complex Sbus { get; set; }
            public ErrVal Err { get; set; }
            public ErrVal ErrRef { get; set; }
        }

        #endregion

        public static Result<IEnumerable<BusResult>> Solve(EENetwork network, 
            double threshold = 0.001, int maxIteration = 100)
        {
            var buses = Initialize(network.Buses);
            var Y = network.YMatrix;

            BusResult slackBus = null;
            var i = 0;
            var isFound = false;
            while (i++ < maxIteration)
            {
                // calculate Vk, Ak, Qk for each bus
                foreach (var bus in buses)
                {
                    #region Calulate

                    // load bus
                    if (bus.BusType == BusTypeEnum.PQ)
                    {
                        var vnxt = CalcVoltage(bus, Y, buses);
                        UpdateVErr(bus, bus.BusVoltage.Magnitude, vnxt.Magnitude, i);
                        UpdateAErr(bus, bus.BusVoltage.Phase, vnxt.Phase, i);
                        // update Vk, Ak
                        bus.BusVoltage = vnxt;
                    }

                    // voltage controlled bus
                    else if (bus.BusType == BusTypeEnum.PV)
                    {
                        // calculate Qk
                        var sk = CalcPower(bus, Y, buses);
                        var (snxt, bt) = CalcMaxQk(bus, sk);
                        UpdateQErr(bus, bus.Sbus.Imaginary, snxt.Imaginary, i, bt == BusTypeEnum.PQ);
                        // update Sbus
                        bus.Sbus = snxt;

                        // calculate Vbus
                        var vnxt = CalcVoltage(bus, Y, buses);
                        UpdateAErr(bus, bus.BusVoltage.Phase, vnxt.Phase, i);

                        if (bt == BusTypeEnum.PV)
                        {
                            // maintain bus controlled voltage and update Ak
                            bus.BusVoltage = Complex.FromPolarCoordinates(bus.BusVoltage.Magnitude, vnxt.Phase);
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
                    else if (bus.BusType == BusTypeEnum.Slack)
                    {
                        slackBus = bus;
                    }

                    #endregion
                }

                #region Check solution

                if (IsSolutionFound(buses, threshold))
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

            // Calculate Pk, Qk for slack bus
            if (isFound)
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
                BusType = b.BusType,
                BusVoltage = new Complex(b.Voltage > 0 ? b.Voltage : 1.0, 0),
                Sbus = new Complex(b.Pgen - b.Pload, b.Qgen - b.Qload)
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
        static (Complex SBus, BusTypeEnum BustType) CalcMaxQk(BusResult bus, Complex sk)
        {
            var qk = sk.Imaginary;

            // required Qgen to maintain given bus voltage
            var qgen = qk + bus.BusData.Qload;

            // calculate Sbus
            if (qk < bus.BusData.Qmin)
                // use definded Qgen min and change to PQ bus
                return (new Complex(sk.Real, bus.BusData.Qmin), BusTypeEnum.PQ);
            else if (qk > bus.BusData.Qmax)
                // use definded Qgen max and change to PQ bus
                return (new Complex(sk.Real, bus.BusData.Qmax), BusTypeEnum.PQ);
            else
                // required Qgen is within limits to maintain bus voltage
                return (sk, BusTypeEnum.PV);
        }

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
                    && Math.Abs(bus.Err.VErr / bus.BusVoltage.Magnitude) <= threshold;
                cv = cv && Math.Abs(bus.BusVoltage.Phase) > 0
                    && Math.Abs(bus.Err.AErr / bus.BusVoltage.Phase) <= threshold;
                if (bus.BusType == BusTypeEnum.PV)
                {
                    cv = cv && Math.Abs(bus.Sbus.Imaginary) > 0
                        && Math.Abs(bus.Err.QErr / bus.Sbus.Imaginary) <= threshold;
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
                cv = cv && (bus.Err.VErr < 0.1 || bus.Err.VErr <= bus.ErrRef.VErr * 3);
                cv = cv && (bus.Err.AErr < 0.1 || bus.Err.AErr <= bus.ErrRef.AErr * 3);
                if (bus.BusType == BusTypeEnum.PV)
                    cv = cv && (bus.Err.QErr < 0.1 || bus.Err.QErr <= bus.ErrRef.QErr * 3);
            }
            return !cv;
        }

        #endregion
    }
}
