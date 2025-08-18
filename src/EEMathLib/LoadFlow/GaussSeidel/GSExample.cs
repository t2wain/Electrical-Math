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
        #region Gauss-Seidel examples

        /// <summary>
        /// Calculate bus "2" voltage at iteration 1.
        /// </summary>
        public static bool CalcVoltage(ILFData data)
        {
            var nw = data.CreateNetwork();
            var buses = LFGS.Initialize(nw.Buses);
            var bus = buses.FirstOrDefault(b => b.ID == "2");
            var v = LFC.CalcVoltage(bus, nw.YMatrix, buses);

            var v2 = new Phasor(0.8746, -15.675);
            var e2 = (Phasor)v;

            var c = Checker.EQ(v2, e2, 0.0001, 0.001);

            return c;
        }

        /// <summary>
        /// Solve load flow using Gauss-Seidel method.
        /// </summary>
        public static bool Solve(ILFData data)
        {
            var nw = data.CreateNetwork();
            var threshold = 0.0001;

            var solver = new LFGaussSeidel();
            var res = solver.Solve(nw, threshold, 100);

            if (res.IsError)
                return false;

            var c = LFC.ValidateLFResult(nw, res.Data, 0.05) ;
            return c;
        }

        #endregion
    }
}
