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

            if (res.IsError)
                return false;

            var rbuses = res.Data.Buses.ToDictionary(bus => bus.ID);
            var dbuses = data.Busses.ToDictionary(bus => bus.ID);

            threshold = 0.05;
            var c = true;
            foreach (var dbus in dbuses.Values)
            {
                var rb = rbuses[dbus.ID];

                // check voltage and phase
                bool v;
                if (rb.BusType == BusTypeEnum.PV
                    || rb.BusType == BusTypeEnum.PQ)
                {
                    v = Checker.EQPct(rb.BusVoltage.Magnitude, dbus.VoltageResult, threshold);
                    c &= v;
                    var phaseDeg = Phasor.ConvertRadianToDegree(rb.BusVoltage.Phase);
                    v = Checker.EQPct(phaseDeg, dbus.AngleResult, threshold);
                    c &= v;
                }

                // check Q
                if (rb.BusType == BusTypeEnum.PV
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    v = Checker.EQPct(rb.Sbus.Imaginary, dbus.QTransmitResult, threshold);
                    c &= v;
                }

                // check P
                if (rb.BusType == BusTypeEnum.Slack)
                {
                    v = Checker.EQPct(rb.Sbus.Real, dbus.PTransmitResult, threshold);
                    c &= v;
                }
            }

            return c;
        }

        #endregion
    }
}
