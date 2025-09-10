using EEMathLib.ShortCircuit.Data;
using EEMathLib.ShortCircuit.ZMX;
using System.Collections.Generic;
using System.Linq;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit
{
    public class SCExample
    {
        public bool BuildZ1()
        {
            var znwa = BuildZ1a();
            var znwb = BuildZ1b();
            var lstBus = znwa.Buses.Keys
                .Aggregate(new List<(IZBus BusA, IZBus BusB)>(), (acc, bid) => 
                {
                    acc.Add((znwa.Buses[bid], znwb.Buses[bid]));
                    return acc;
                });
            var res = true;
            foreach (var i in lstBus)
            {
                var za = znwa.Z[i.BusA.BusIndex, i.BusA.BusIndex];
                var zb = znwb.Z[i.BusB.BusIndex, i.BusB.BusIndex];
                res &= (za - zb).Magnitude < 0.0001;
            }
            return res;
        }

        public ZNetwork BuildZ1a()
        {
            var znw = new ZNetwork1();
            var N = znw.Buses.Count;
            znw.Z = MC.Build.Dense(N, N);

            var dEl = znw.Elements;

            var el1 = dEl["1"];
            if (el1.ValidateAddElementRefToNewBus())
                znw.AddElementRefToNewBus(el1);

            var el2 = dEl["2"];
            if (el2.ValidateAddElementRefToNewBus())
                znw.AddElementRefToNewBus(el2);

            var el3 = dEl["3"];
            if (el3.ValidateAddElementExistToExistBus())
                znw.AddElementExistToExistBus(el3);

            var el4 = dEl["4"];
            if (el4.ValidateAddElementNewToExistBus())
                znw.AddElementNewToExistBus(el4);

            var el5 = dEl["5"];
            if (el5.ValidateAddElementNewToExistBus())
                znw.AddElementNewToExistBus(el5);

            var el6 = dEl["6"];
            if (el6.ValidateAddElementExistToExistBus())
                znw.AddElementExistToExistBus(el6);

            var el7 = dEl["7"];
            if (el7.ValidateAddElementExistToExistBus())
                znw.AddElementExistToExistBus(el7);

            return znw;

        }

        public ZNetwork BuildZ1b()
        {
            var znw = new ZNetwork1();
            var N = znw.Buses.Count;
            znw.Z = MC.Build.Dense(N, N);
            znw.BuildZMatrix();
            return znw;
        }

    }
}
