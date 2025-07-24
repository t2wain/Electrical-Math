using EEMathLib.DTO;
using EEMathLib.MatrixMath;
using System.Linq;
using GS = EEMathLib.LoadFlow.LFGaussSiedel;
using LFC = EEMathLib.LoadFlow.LFCommon;

namespace EEMathLib.LoadFlow
{
    public class GSExample
    {
        private readonly ILFData _data;

        public GSExample(ILFData data)
        {
            this._data = data;
        }

        #region Y Matrix

        /// <summary>
        /// Build Y matrix
        /// </summary>
        public bool BuildYMatrix_Partial()
        {
            var nw = this._data.CreateNetwork();
            var Y = nw.YMatrix;
            var res = MX.ParseMatrix(this._data.YResult);

            var y25 = new Phasor(19.9195, 95.143);
            var e25 = Phasor.Convert(Y[1, 4]);
            var c = Checker.EQ(y25, e25, 0.0001, 0.001);

            var y24 = new Phasor(9.95972, 95.143);
            var e24 = Phasor.Convert(Y[1, 3]);
            c = c && Checker.EQ(y24, e24, 0.0001, 0.001);

            var y22 = new Phasor(28.5847, -84.624);
            var e22 = Phasor.Convert(Y[1, 1]);
            c = c && Checker.EQ(y22, e22, 0.0001, 0.001);

            return c;
        }

        /// <summary>
        /// Build Y matrix
        /// </summary>
        public bool BuildYMatrix()
        {
            var nw = this._data.CreateNetwork();
            var Y = nw.YMatrix;
            var res = MX.ParseMatrix(this._data.YResult);

            var d = Y.Transpose().AsColumnMajorArray();
            var q = d.Zip(this._data.YResult.Entries, (y, r) => new { y, r });
            var c = true;
            foreach(var i in q)
            {
                c = c && Checker.EQ(i.y, i.r, 0.01, 0.01);
                if (!c) break;
            }
            return c;
        }

        #endregion

        #region Gauss-Siedel examples

        /// <summary>
        /// Calculate bus "2" voltage at iteration 1.
        /// </summary>
        public bool CalcVoltage()
        {
            var nw = this._data.CreateNetwork();
            var buses = GS.Initialize(nw.Buses);
            var bus = buses.FirstOrDefault(b => b.ID == "2");
            var v = LFC.CalcVoltage(bus, nw.YMatrix, buses);

            var v2 = new Phasor(0.8746, -15.675);
            var e2 = (Phasor)v;

            var c = Checker.EQ(v2, e2, 0.0001, 0.001);

            return c;
        }

        /// <summary>
        /// Solve load flow using Gauss-Siedel method.
        /// </summary>
        public bool Solve()
        {
            var nw = this._data.CreateNetwork();
            var threshold = 0.0125;
            var res = GS.Solve(nw, threshold, 100, 20);

            if (res.Error == ErrorEnum.Divergence)
                return false;

            var rbuses = LFC.CalcResult(res.Data).ToDictionary(bus => bus.ID);
            var dbuses = this._data.LFResult.ToDictionary(bus => bus.ID);

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
