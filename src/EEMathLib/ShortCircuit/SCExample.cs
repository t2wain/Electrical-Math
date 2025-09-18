using EEMathLib.MatrixMath;
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
            IDictionary<string, Complex> res = znw.CalcCurrentAllBus();
        }

        public void Calc3PhaseFaultBusesVoltage(ZNetwork znw, string busFaultId)
        {
            var res = znw.CalcBusesVoltage(busFaultId);
            var dV = znw.Buses.Values.Aggregate(new Dictionary<string, Complex>(), (acc, bus) => 
            {
                acc.Add(bus.ID, res[bus.BusIndex, 0]);
                return acc;
            });
        }

        public void Calc3PhaseFaultBusFlowFromAllBus(ZNetwork znw, string busFaultId)
        {
            var mxI = znw.CalcBusFlow(busFaultId);
            var dI = znw.Buses.Values.Aggregate(new Dictionary<string, Complex>(), (acc, bus) =>
            {
                acc.Add(bus.ID, mxI[bus.BusIndex, 0]);
                return acc;
            });
        }

        public void Calc3PhaseFaultElementFlow(ZNetwork znw, string busFaultId)
        {
            var mxV = znw.CalcBusesVoltage(busFaultId);
            var dI = znw.CalcElementFlow(mxV);
        }


        #endregion

        #region Symmetrical Components

        public bool CalcSymVoltage()
        {
            // asymmetrical Va, Vb, Vc
            IAsymPhasor asymV = new PhaseValue
            {
                P1 = new Phasor(7.3, 12.5).ToComplex(),
                P2 = new Phasor(0.4, -100).ToComplex(),
                P3 = new Phasor(4.4, 154).ToComplex(),
            };

            // decompose asym to sym components
            ISymComp symV = SCSComp.CalcSymComp(asymV);

            var res = true;

            // zero-sequence Va
            var va0 = Phasor.Convert(symV.A0);
            var v = Checker.EQ(va0, new Phasor(1.47, 45.1), 0.1, 0.1);
            res &= v;

            // positive-sequence Va
            var va1 = Phasor.Convert(symV.A1);
            v = Checker.EQ(va1, new Phasor(3.97, 20.5), 0.1, 0.1);
            res &= v;

            // negative-sequence Va
            var va2 = Phasor.Convert(symV.A2);
            v = Checker.EQ(va2, new Phasor(2.52, -19.7), 0.1, 0.1);
            res &= v;

            return res;
        }

        public bool CalcAsymPower()
        {
            // asym voltages Va, Vb, Vc
            IAsymPhasor asymV = new PhaseValue
            {
                P1 = 0,
                P2 = 50,
                P3 = -50
            };

            // asym currents Ia, Ib, Ic
            IAsymPhasor asymI = new PhaseValue
            {
                P1 = -5,
                P2 = new Complex(0, 5),
                P3 = -5,
            };

            // three-phase apparent power
            var s3 = SCSComp.CalcAsymPower(asymV, asymI);
            var res = Checker.EQ(s3, new Phasor(353.5534, -45), 0.01, 0.1);

            return res;
        }

        public bool CalcZSeqMatrices()
        {
            var nw1 = new ZNetwork3Z1();

            var nw2 = new ZNetwork3Z2();
            nw2.Buses = nw1.Buses.Values
                .Select(b => new ZBus { ID = b.ID, Data = b.Data })
                .Cast<IZBus>()
                .ToDictionary(b => b.ID);
            nw2.Init();

            var nw0 = new ZNetwork3Z0();
            nw0.Buses = nw1.Buses.Values
                .Select(b => new ZBus { ID = b.ID, Data = b.Data })
                .Cast<IZBus>()
                .ToDictionary(b => b.ID);
            nw0.Init();

            var res = true;

            var znw1 = nw1.BuildZMatrix();
            var b3 = znw1.Buses["3"];
            var z3 = znw1.Z[b3.BusIndex, b3.BusIndex];
            var v = Checker.EQ(z3, MX.C(0, 0.2618), 0.001, 0.001);
            res &= v;

            var znw2 = nw2.BuildZMatrix();
            b3 = znw2.Buses["3"];
            z3 = znw2.Z[b3.BusIndex, b3.BusIndex];
            v = Checker.EQ(z3, MX.C(0, 0.2317), 0.001, 0.001);
            res &= v;

            var znw0 = nw0.BuildZMatrix();
            b3 = znw0.Buses["3"];
            z3 = znw0.Z[b3.BusIndex, b3.BusIndex];
            v = Checker.EQ(z3, MX.C(0, 0.56), 0.01, 0.01);
            res &= v;

            return res;
        }

        #endregion

    }
}
