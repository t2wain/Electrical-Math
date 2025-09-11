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

            var v = znwa.Validate(zref);
            res &= v;

            var znwb = BuildZ1MethodB();

            // both Z matrices should equal
            // although elements are added
            // in different order
            v = znwb.Validate(zref);
            res &= v;

            return res;
        }

        /// <summary>
        /// Build Z matrix by adding individual element
        /// in a specific sequence so it can be checked
        /// against the reference Z matrix data
        /// </summary>
        public ZNetwork BuildZ1MethodA() => new ZNetwork1().BuildZTest();

        /// <summary>
        /// Build Z matrix by iterate through network graph
        /// </summary>
        public ZNetwork BuildZ1MethodB() => new ZNetwork1().BuildZMatrix();

        public bool BuildZ2()
        {
            var znw = new ZNetwork2().BuildZMatrix();
            var zref = znw.RefZNetwork;
            var res = znw.Validate(zref);
            return res;
        }

        #endregion

        public void Calc3PhaseFault(ZNetwork znw)
        {
            var res = SCAlgo.Calc3PhaseFaultCurrentAllBus(znw);
        }

        public void Calc3PhaseFaultBusesVoltage(ZNetwork znw, string busFaultId)
        {
            var res1 = SCAlgo.Calc3PhaseFaultBusesVoltage(znw, busFaultId);
            var res2 = SCAlgo.Calc3PhaseFaultBusesVoltageV2(znw, busFaultId);
            var dV = znw.Buses.Values.Aggregate(new Dictionary<string, Complex>(), (acc, bus) => 
            {
                acc.Add(bus.ID, res2[bus.BusIndex, 0]);
                return acc;
            });
        }

        public void Calc3PhaseFaultCurrentFlowFromAllBus(ZNetwork znw, string busFaultId)
        {
            var mxI = SCAlgo.Calc3PhaseFaultCurrentBranchFlow(znw, busFaultId);
            var dI = znw.Buses.Values.Aggregate(new Dictionary<string, Complex>(), (acc, bus) =>
            {
                acc.Add(bus.ID, mxI[bus.BusIndex, 0]);
                return acc;
            });
        }

    }
}
