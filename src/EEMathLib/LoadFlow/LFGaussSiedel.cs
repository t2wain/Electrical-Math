using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;

namespace EEMathLib.LoadFlow
{
    public abstract class LFGaussSiedel
    {
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
            public ErrVal Err { get; set; }
            public ErrVal ErrRef { get; set; }
        }

        public IEnumerable<BusResult> Calculate(EENetwork network)
        {
            var buses = new List<BusResult>(
                network.Buses
                    .OrderBy(b => b.BusIndex)
                    .Select(b => new BusResult { 
                        BusData = b, 
                        BusType = b.BusType,
                        BusVoltage = b.BusVoltage, 
                        Sbus = b.Sbus 
                    })
            );
            var N = buses.Count;
            var Y = network.YMatrix;

            var verr = CreateVector.Dense(N, new Complex(0, 0));

            var i = 0;
            while(i++ < 100)
            {
                foreach(var k in Enumerable.Range(0, N))
                {
                    var bus = buses[k];
                    if (bus.BusType == BusTypeEnum.PQ) // load bus
                    {
                        var vnxt = CalcVoltage(bus, Y, buses);
                        UpdateVErr(bus, bus.BusVoltage.Magnitude, vnxt.Magnitude, i);
                        UpdateAErr(bus, bus.BusVoltage.Phase, vnxt.Phase, i);
                        bus.BusVoltage = vnxt;
                    }
                    else if (bus.BusType == BusTypeEnum.PV) // voltage controlled bus
                    {
                        // calculate Qk
                        var sk = CalcPower(bus, Y, buses);
                        var (snxt, bt) = CalcMaxSbus(bus, sk);
                        UpdateQErr(bus, bus.Sbus.Imaginary, snxt.Imaginary, i);
                        bus.Sbus = snxt;

                        // calculate Vbus
                        var vnxt = CalcVoltage(bus, Y, buses);
                        UpdateAErr(bus, bus.BusVoltage.Phase, vnxt.Phase, i);

                        if (bt == BusTypeEnum.PV)
                        {
                            // update phase angle
                            bus.BusVoltage = Complex.FromPolarCoordinates(bus.BusVoltage.Magnitude, vnxt.Phase);
                        }
                        else
                        {
                            bus.BusType = BusTypeEnum.PQ; // change to PQ bus
                            UpdateVErr(bus, bus.BusVoltage.Magnitude, vnxt.Magnitude, i);
                            bus.BusVoltage = vnxt; // use the calculated voltage
                        }
                    }

                    if (IsConverged(buses) || IsDiverged(buses, i))
                        break;
                }
            }

            // Calculate Pk, Qk for slack bus
            var slackBus = buses[0];
            slackBus.Sbus = CalcPower(slackBus, Y, buses);

            return buses;
        }

        public Complex CalcVoltage(BusResult bus, Matrix<Complex> Y, IEnumerable<BusResult> buses)
        {
            var k = bus.BusIndex;
            var yk = Y[k, k];
            var pk = bus.Sbus;
            var vk = bus.BusVoltage;
            var sv = buses
                .Where(b => b.BusIndex != k)
                .Select(b =>
                {
                    var idx = b.BusIndex;
                    var yb = Y[k, idx];
                    return yb * b.BusVoltage;
                })
                .Aggregate((v1, v2) => v1 + v2);

            var vknxt = 1 / yk * (pk / vk.Conjugate() - sv);
            vknxt = 1 / yk * (pk / vknxt.Conjugate() - sv);
            return vknxt;
        }

        public Complex CalcPower(BusResult bus, Matrix<Complex> Y, IEnumerable<BusResult> buses)
        {
            var vk = bus.BusVoltage;
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

        protected (Complex SBus, BusTypeEnum BustType) CalcMaxSbus(BusResult bus, Complex sk)
        {
            var qk = sk.Imaginary;
            var qg = qk + bus.BusData.Qload;

            // calculate Sbus
            if (qk < bus.BusData.Qmin)
                return (new Complex(bus.BusData.Pload, bus.BusData.Qmin), BusTypeEnum.PQ);
            else if (qk > bus.BusData.Qmax)
                return (new Complex(bus.BusData.Pload, bus.BusData.Qmax), BusTypeEnum.PQ);
            else return (new Complex(bus.BusData.Pload, qk), BusTypeEnum.PV);
        }

        protected void UpdateVErr(BusResult bus, double vcur, double vnxt, int iteration)
        {
            bus.Err.VErr = Math.Abs(vcur - vnxt);
            if (iteration == 1 || iteration % 5 == 0)
                bus.ErrRef.VErr = bus.Err.VErr;
        }

        protected void UpdateAErr(BusResult bus, double acur, double anext, int iteration)
        {
            bus.Err.AErr = Math.Abs(anext - acur);
            if (iteration == 1 || iteration % 5 == 0)
                bus.ErrRef.AErr = bus.Err.AErr;
        }

        protected void UpdateQErr(BusResult bus, double qcur, double qnext, int iteration)
        {
            bus.Err.QErr = Math.Abs(qcur - qnext);
            if (iteration == 1 || iteration % 5 == 0)
                bus.ErrRef.QErr = bus.Err.QErr;
        }

        protected bool IsConverged(IEnumerable<BusResult> buses, double threshold = 0.001)
        {
            var b = false;
            foreach (var bus in buses)
            {
                b = (bus.Err.VErr / bus.BusVoltage.Magnitude) <= threshold;
                b = b && (bus.Err.AErr / bus.BusVoltage.Phase) <= threshold;
                b = b && (bus.Err.QErr / bus.Sbus.Imaginary) <= threshold;
            }
            return b;
        }

        protected bool IsDiverged(IEnumerable<BusResult> buses, int iteration)
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
    }
}
