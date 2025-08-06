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
            var threshold = 0.0125;
            var res = LFGS.Solve(nw, threshold, 100, 20);

            if (res.Error == ErrorEnum.Divergence)
                return false;

            var rbuses = LFC.CalcResult(res.Data).ToDictionary(bus => bus.ID);
            var dbuses = data.LFResult.ToDictionary(bus => bus.ID);

            var c = true;
            foreach (var dbus in dbuses.Values)
            {
                var rb = rbuses[dbus.ID];
                c = c && Checker.EQPct(rb.Voltage, dbus.Voltage, threshold);
                c = c && Checker.EQPct(rb.Angle, dbus.Angle, threshold);

                if (rb.BusType == BusTypeEnum.PQ
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    c = c && Checker.EQPct(rb.Pgen, dbus.Pgen, threshold);
                    c = c && Checker.EQPct(rb.Qgen, dbus.Qgen, threshold);
                }
            }

            return c;
        }

        #endregion
    }
}
