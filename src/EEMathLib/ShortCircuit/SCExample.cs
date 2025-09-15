using EEMathLib.ShortCircuit.Data;
using EEMathLib.ShortCircuit.ZMX;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit
{
    public class SCExample
    {

        #region Z Matrix

        /// <summary>
        /// Build Z matrix based on a given test data
        /// </summary>
        public bool BuildZ1()
        {
            var res = true;
            var znwa = BuildZ1MethodA();

            var zref = znwa.RefZNetwork;
            zref.Buses = znwa.Buses;

            var v = znwa.EQ(zref);
            res &= v;

            var znwb = BuildZ1MethodB();

            // both Z matrices should equal
            // although elements are added
            // in different order
            v = znwb.EQ(zref);
            res &= v;

            return res;
        }

        /// <summary>
        /// Build Z matrix by adding individual element
        /// in a specific sequence so it can be checked
        /// against the reference Z matrix data
        /// </summary>
        public ZNetwork BuildZ1MethodA()
        {
            var znw = new ZNetwork1();

            var N = znw.Buses.Count;
            znw.Z = MC.Build.Dense(N, N);
            var dEl = znw.Elements;

            return znw.AddElement(dEl["1"])
                .AddElement(dEl["2"])
                .AddElement(dEl["3"])
                .AddElement(dEl["4"])
                .AddElement(dEl["5"])
                .AddElement(dEl["6"])
                .AddElement(dEl["7"]);
        }

        /// <summary>
        /// Build Z matrix by iterate through network graph
        /// </summary>
        public ZNetwork BuildZ1MethodB() => new ZNetwork1().BuildZMatrix();

        public bool BuildZ2()
        {
            var znw = new ZNetwork2().BuildZMatrix();
            var zref = znw.RefZNetwork;
            var res = znw.EQ(zref);
            return res;
        }

        #endregion

        #region 3-Phase Symmetrical Fault Calculation

        public void Calc3PhaseFault(ZNetwork znw)
        {
            // fault current at each bus
            IDictionary<string, Complex> res = SCSymAlgo.CalcCurrentAllBus(znw);
        }

        public void Calc3PhaseFaultBusesVoltage(ZNetwork znw, string busFaultId)
        {
            var res = SCSymAlgo.CalcBusesVoltage(znw, busFaultId);
            var dV = znw.Buses.Values.Aggregate(new Dictionary<string, Complex>(), (acc, bus) => 
            {
                acc.Add(bus.ID, res[bus.BusIndex, 0]);
                return acc;
            });
        }

        public void Calc3PhaseFaultBusFlowFromAllBus(ZNetwork znw, string busFaultId)
        {
            var mxI = SCSymAlgo.CalcBusFlow(znw, busFaultId);
            var dI = znw.Buses.Values.Aggregate(new Dictionary<string, Complex>(), (acc, bus) =>
            {
                acc.Add(bus.ID, mxI[bus.BusIndex, 0]);
                return acc;
            });
        }

        public void Calc3PhaseFaultElementFlow(ZNetwork znw, string busFaultId)
        {
            var mxV = SCSymAlgo.CalcBusesVoltage(znw, busFaultId);
            var dI = SCSymAlgo.CalcElementFlow(znw, mxV);
        }


        #endregion

    }
}
