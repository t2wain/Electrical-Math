using EEMathLib.LoadFlow.Data;
using System.Linq;
using LFC = EEMathLib.LoadFlow.LFCommon;
using LFGS = EEMathLib.LoadFlow.GaussSeidel.LFGaussSeidel;

namespace EEMathLib.LoadFlow.GaussSeidel
{
    /// <summary>
    /// Tests for various calculations
    /// in Gauss-Seidel algorithm
    /// </summary>
    public static class GSExample
    {

        /// <summary>
        /// Calculate bus "2" voltage at iteration 1.
        /// </summary>
        public static bool CalcVoltage(ILFData data)
        {
            var nw = data.CreateNetwork();
            var buses = LFGS.Initialize(nw.EBuses);
            var bus = buses.FirstOrDefault(b => b.ID == "2");
            var v = LFGS.CalcBusVoltage(bus, nw.YMatrix, buses);

            var v2 = new Phasor(0.8746, -15.675);
            var e2 = (Phasor)v;

            var c = Checker.EQ(v2, e2, 0.0001, 0.001);

            return c;
        }

        /// <summary>
        /// Solve load flow using Gauss-Seidel method.
        /// </summary>
        public static bool Solve(ILFData data, bool validate = false, int maxIteration = 100)
        {
            var nw = data.CreateNetwork();
            var threshold = 0.0001;

            var solver = new LFGaussSeidel();
            var res = solver.Solve(nw, threshold, maxIteration);

            if (res.IsError)
                return false;

            if (!validate)
                return true;

            var c = LFC.ValidateLFResult(nw, res.Data, 0.05) ;
            return c;
        }
    }
}
