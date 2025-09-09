using EEMathLib.ShortCircuit.Data;
using EEMathLib.ShortCircuit.ZMX;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EEMathLib.ShortCircuit
{
    public static class SCAlgo
    {
        public static void Calc3PSym(ZNetwork nw, IZBus bus)
        {
            var vfn = Complex.FromPolarCoordinates(bus.Data.Voltage, 0);
            var kx = bus.BusIndex;
            var zk = nw.Z[kx, kx];
            var ik = vfn / zk;

            var lst = new List<SCBusResult>();
            foreach (var bn in nw.Buses.Values)
            {
                var nx = bn.BusIndex;
                var zn = nw.Z[kx, nx];
                var vfk = (1 - (zn / zk)) * vfn;
                lst.Add(new SCBusResult { BusData = bn, Voltage = vfk });
            }

            var dBusRes = lst.ToDictionary(b => b.BusData.ID);
            foreach (var line in nw.Elements.Values)
            {
                var b = dBusRes.TryGetValue(line.FromBus.ID, out var fb) &&
                    dBusRes.TryGetValue(line.ToBus.ID, out var tb);
                if (!b) continue;
            }

            var result = new SCResult
            {
                Bus = new SCBusResult { BusData = bus, Voltage = vfn },
                Current = ik,
                Buses = lst,
            };

        }
    }
}
