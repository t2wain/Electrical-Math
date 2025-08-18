using EEMathLib.LoadFlow.Data;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static EEMathLib.LoadFlow.NewtonRaphson.Jacobian;

namespace EEMathLib.LoadFlow
{
    public static class LFCommon
    {
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
            var vkIdx = bus.BusData.BusIndex;
            var sv = buses
                .Select(bn =>
                {
                    var bnIdx = bn.BusData.BusIndex;
                    var vn = bn.BusVoltage;
                    var yn = Y[vkIdx, bnIdx];
                    return yn * vn;
                })
                .Aggregate((v1, v2) => v1 + v2);
            var sk = vk * sv.Conjugate();
            return sk;
        }

        public static IEnumerable<LineResult> CalcPower(IEnumerable<EELine> lines, IEnumerable<BusResult> buses)
        {
            var dbuses = buses.ToDictionary(b => b.ID);
            var lst = lines
                .Select(l => new { Line = l, S = CalcPower(l, dbuses), SRev = CalcPower(l, dbuses, true) })
                .Select(o => new LineResult 
                {
                    LineData = o.Line,
                    SLine = o.S,
                    P = o.S.Real,
                    Q = o.S.Imaginary,
                    SLineReverse = o.SRev,
                    PReverse = o.SRev.Real,
                    QReverse = o.SRev.Imaginary,
                }).ToList();
            return lst;
        }

        public static Complex CalcPower(EELine line, IDictionary<string, BusResult> buses, bool isReverse = false)
        {
            Complex vi, vj;
            if (isReverse)
            {
                vj = buses[line.FromBus.ID].BusVoltage;
                vi = buses[line.ToBus.ID].BusVoltage;
            }
            else
            {
                vi = buses[line.FromBus.ID].BusVoltage;
                vj = buses[line.ToBus.ID].BusVoltage;
            }
            var yseries = 1 / line.ZImpSeries;
            var yshunt = line.YImpShunt;
            var sseries = vi * (vi - vj).Conjugate() * yseries.Conjugate();
            var sshunt = Complex.Zero;
            if (line.YImpShunt.Magnitude > 0)
                sshunt = vi * (vi * yshunt / 2).Conjugate();
            var sline = sseries + sshunt;
            return sline;
        }

        /// <summary>
        /// Calculate Qk for given Sk based on Qgen limits.
        /// </summary>
        /// <returns>Tuples of Sk and the bus type</returns>
        public static (Complex SBus, BusTypeEnum BusType, double Qgen) CalcMaxQk(BusResult bus, Complex sk)
        {
            var qk = sk.Imaginary;

            // required Qgen to maintain given bus voltage
            var qgen = qk + bus.BusData.Qload;
            var pk = bus.BusData.Pgen - bus.BusData.Pload;

            // calculate Sbus
            if (qgen < bus.BusData.Qmin)
                // use definded Qgen min and change to PQ bus
                return (new Complex(pk, bus.BusData.Qmin + bus.BusData.Qload), BusTypeEnum.PQ, bus.BusData.Qmin);
            else if (qgen > bus.BusData.Qmax)
                // use definded Qgen max and change to PQ bus
                return (new Complex(pk, bus.BusData.Qmax + bus.BusData.Qload), BusTypeEnum.PQ, bus.BusData.Qmax);
            else
                // required Qgen is within limits to maintain bus voltage
                return (new Complex(pk, sk.Imaginary), BusTypeEnum.PV, qgen);
        }

        public static bool ValidateLFResult(EENetwork network, LFResult res, double thresholdPct)
        {
            var c = true;
            bool v;

            #region Validate buses

            var rbuses = res.Buses.ToDictionary(b => b.ID);
            foreach (var dbus in network.Buses)
            {
                var rb = rbuses[dbus.ID];

                // check voltage and phase
                if (rb.BusType == BusTypeEnum.PV
                    || rb.BusType == BusTypeEnum.PQ)
                {
                    v = Checker.EQPct(rb.BusVoltage.Magnitude, dbus.VoltageResult, thresholdPct);
                    c &= v;
                    var phaseDeg = Phasor.ConvertRadianToDegree(rb.BusVoltage.Phase);
                    v = Checker.EQPct(phaseDeg, dbus.AngleResult, thresholdPct);
                    c &= v;
                }

                // check Q
                if (rb.BusType == BusTypeEnum.PV
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    v = Checker.EQPct(rb.Sbus.Imaginary, dbus.QResult, thresholdPct);
                    c &= v;
                }

                // check Q gen
                if (rb.BusType == BusTypeEnum.PV)
                {
                    v = Checker.EQPct(rb.Qgen, dbus.QgenResult, thresholdPct);
                    c &= v;
                }

                // check P
                if (rb.BusType == BusTypeEnum.Slack)
                {
                    v = Checker.EQPct(rb.Sbus.Real, dbus.PResult, thresholdPct);
                    c &= v;
                }
            }

            #endregion

            #region Validate lines

            var rlines = res.Lines.ToDictionary(l => l.LineData.ID);
            foreach (var dline in network.Lines)
            {
                var rl = rlines[dline.ID];

                // check P and Q flow FromBus-ToBus
                if (dline.PResult.HasValue)
                {
                    v = Checker.EQPct(rl.P, dline.PResult.Value, thresholdPct);
                    c &= v;
                }
                if (dline.QResult.HasValue)
                {
                    v = Checker.EQPct(rl.Q, dline.QResult.Value, thresholdPct);
                    c &= v;
                }

                // check P and Q flow ToBus-FromBus
                if (dline.PResultReverse.HasValue)
                {
                    v = Checker.EQPct(rl.PReverse, dline.PResultReverse.Value, thresholdPct);
                    c &= v;
                }
                if (dline.QResultReverse.HasValue)
                {
                    v = Checker.EQPct(rl.QReverse, dline.QResultReverse.Value, thresholdPct);
                    c &= v;
                }

            }

            #endregion

            return c;

        }
    }
}
