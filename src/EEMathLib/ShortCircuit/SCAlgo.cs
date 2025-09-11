using EEMathLib.ShortCircuit.ZMX;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit
{
    public static class SCAlgo
    {
        public static IDictionary<string, Complex> Calc3PhaseFaultCurrentAllBus(ZNetwork znw)
        {
            var res = znw.Buses.Values.Aggregate(new Dictionary<string, Complex>(), (acc, bus) =>
            {
                var v = bus.Data?.Voltage ?? 1.0;
                var z = znw.Z[bus.BusIndex, bus.BusIndex];
                acc.Add(bus.ID, v / z);
                return acc;
            });
            return res;
        }

        public static Complex Calc3PhaseFaultCurrent(ZNetwork znw, string faultedBusId)
        {
            var bfault = znw.Buses[faultedBusId];
            var z = znw.Z[bfault.BusIndex, bfault.BusIndex];
            var v = bfault.Data?.Voltage ?? 1.0;
            var ifault = v / z;
            return ifault;
        }

        public static MC Calc3PhaseFaultBusesVoltage(ZNetwork znw, string faultedBusId) 
        {
            var bfault = znw.Buses[faultedBusId];
            var z = znw.Z[bfault.BusIndex, bfault.BusIndex];
            var v = bfault.Data?.Voltage ?? 1.0;
            var ifault = v / z;

            var mxI = MC.Build.Dense(znw.Buses.Count, 1);
            mxI[bfault.BusIndex, 0] = -ifault;
            var mxV = znw.Z * mxI;
            mxV.MapInplace(e => v + e);
            return mxV;
        }

        public static MC Calc3PhaseFaultBusesVoltageV2(ZNetwork znw, string faultedBusId)
        {
            var bn = znw.Buses[faultedBusId];
            var znn = znw.Z[bn.BusIndex, bn.BusIndex];
            var vf = bn.Data?.Voltage ?? 1.0;
            var ifn = vf / znn;

            var mxV = znw.Buses.Values.Aggregate(MC.Build.Dense(znw.Buses.Count, 1), (acc, bus) =>
            {
                var bk = bus;
                var zkn = znw.Z[bk.BusIndex, bn.BusIndex];
                acc[bus.BusIndex, 0] = vf - zkn / znn;
                return acc;
            });
            return mxV;
        }

        public static MC Calc3PhaseFaultCurrentBranchFlow(ZNetwork znw, string faultedBusId)
        {
            var bn = znw.Buses[faultedBusId];
            var vf = bn.Data?.Voltage ?? 1.0;

            // voltage at each bus during fault
            var dBusV = Calc3PhaseFaultBusesVoltage(znw, faultedBusId);

            // voltage delta across branch 
            // between each bus to faulted bus
            dBusV.MapInplace(v => vf - v);

            // current from across branch
            // between each bus to faulted bus
            var mxI = znw.Z.Inverse() * dBusV;

            return mxI;
        }

    }
}
