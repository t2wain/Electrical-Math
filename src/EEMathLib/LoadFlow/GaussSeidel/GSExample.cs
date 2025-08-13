using EEMathLib.DTO;
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

            if (res.Error == ErrorEnum.Divergence)
                return false;

            var rbuses = LFC.CalcResult(res.Data).ToDictionary(bus => bus.ID);
            var dbuses = data.LFResult.ToDictionary(bus => bus.ID);

            threshold = 0.01;
            var c = true;
            foreach (var dbus in dbuses.Values)
            {
                var rb = rbuses[dbus.ID];
                var v = Checker.EQ(rb.Voltage, dbus.Voltage, threshold);
                c &= v;
                v = Checker.EQ(rb.Angle, dbus.Angle, threshold);
                c &= v;

                if (rb.BusType == BusTypeEnum.PQ
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    v = Checker.EQ(rb.Pgen, dbus.Pgen, threshold);
                    c &= v;
                    v = Checker.EQ(rb.Qgen, dbus.Qgen, threshold);
                    c &= v;
                }
            }

            return !res.IsError;
        }

        #endregion
    }
}
