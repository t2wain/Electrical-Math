using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;

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
            public EEBus BusData { get; set; }
            public int BusIndex => BusData.BusIndex;
            public BusTypeEnum BusType { get; set; }
            public Complex BusVoltage { get; set; }
            public Complex Sbus { get; set; }
            public ErrVal Err { get; set; } = new ErrVal();
            public ErrVal ErrRef { get; set; } = new ErrVal();
        }

        #endregion

        public static IEnumerable<BusResult> Solve(EENetwork network, int maxIteration = 100)
        {
            // DTO
            var buses = network.Buses
                    .Select(b => new BusResult
                    {
                        BusData = b,
                        BusType = b.BusType,
                        BusVoltage = new Complex(b.Voltage > 0 ? b.Voltage : 1.0, 0),
                        Sbus = new Complex(b.Pgen - b.Pload, b.Qgen - b.Qload)
                    })
                    .ToList();

            var Y = network.YMatrix;

            BusResult slackBus = null;
            var i = 0;
            while (i++ < maxIteration)
            {
                // calculate Vk, Ak, Qk for each bus
                foreach (var bus in buses)
                {
                    if (bus.BusType == BusTypeEnum.PQ) // load bus
                    {
                        var vnxt = CalcVoltage(bus, Y, buses);
                        UpdateVErr(bus, bus.BusVoltage.Magnitude, vnxt.Magnitude, i);
                        UpdateAErr(bus, bus.BusVoltage.Phase, vnxt.Phase, i);
                        // update Vk, Ak
                        bus.BusVoltage = vnxt;
                    }
                    else if (bus.BusType == BusTypeEnum.PV) // voltage controlled bus
                    {
                        // calculate Qk
                        var sk = CalcPower(bus, Y, buses);
                        var (snxt, bt) = CalcMaxQk(bus, sk);
                        UpdateQErr(bus, bus.Sbus.Imaginary, snxt.Imaginary, i);
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
                    else if (bus.BusType == BusTypeEnum.Slack)
                        slackBus = bus; // save slack bus for later calculation

                    if (IsConverged(buses) || IsDiverged(buses, i))
                        break;
                }
            }

            // Calculate Pk, Qk for slack bus
            slackBus.Sbus = CalcPower(slackBus, Y, buses);

            return buses;
        }

        #region Calculation

        /// <summary>
        /// Calculate Vk, Ak given Pk, Qk for bus k.
        /// </summary>
        /// <returns>Voltage Sk</returns>
        static Complex CalcVoltage(BusResult bus, Matrix<Complex> Y, IEnumerable<BusResult> buses)
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
            var vknxt = 1 / yk * (sk / vk.Conjugate() - sv); // using vk
            vknxt = 1 / yk * (sk / vknxt.Conjugate() - sv); // using vknxt
            return vknxt;
        }

        /// <summary>
        /// Calculate Sk for bus k.
        /// </summary>
        /// <returns>Apparent power Sk</returns>
        static Complex CalcPower(BusResult bus, Matrix<Complex> Y, IEnumerable<BusResult> buses)
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
            bus.Err.VErr = Math.Abs(vcur - vnxt);
            if (iteration == 1 || iteration % 5 == 0)
                bus.ErrRef.VErr = bus.Err.VErr;
        }

        static void UpdateAErr(BusResult bus, double acur, double anext, int iteration)
        {
            bus.Err.AErr = Math.Abs(anext - acur);
            if (iteration == 1 || iteration % 5 == 0)
                bus.ErrRef.AErr = bus.Err.AErr;
        }

        static void UpdateQErr(BusResult bus, double qcur, double qnext, int iteration)
        {
            bus.Err.QErr = Math.Abs(qcur - qnext);
            if (iteration == 1 || iteration % 5 == 0)
                bus.ErrRef.QErr = bus.Err.QErr;
        }

        static bool IsConverged(IEnumerable<BusResult> buses, double threshold = 0.001)
        {
            var cv = false;
            foreach (var bus in buses.Where(b => b.BusType != BusTypeEnum.Slack))
            {
                cv = bus.BusVoltage.Magnitude > 0 && (bus.Err.VErr / bus.BusVoltage.Magnitude) <= threshold;
                cv = cv && bus.BusVoltage.Phase > 0 && (bus.Err.AErr / bus.BusVoltage.Phase) <= threshold;
                cv = cv && bus.Sbus.Imaginary > 0 && (bus.Err.QErr / bus.Sbus.Imaginary) <= threshold;
            }
            return cv;
        }

        static bool IsDiverged(IEnumerable<BusResult> buses, int iteration)
        {
            if (iteration % 5 != 0)
                return false;

            var b = false;
            foreach (var bus in buses)
            {
                b = bus.Err.VErr > bus.ErrRef.VErr;
                b = b && bus.Err.AErr > bus.ErrRef.AErr;
                b = b && bus.Err.QErr > bus.ErrRef.QErr;
            }
            return b;
        }

        #endregion
    }
}
