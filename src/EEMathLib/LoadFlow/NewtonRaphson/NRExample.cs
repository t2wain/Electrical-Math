using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using System.Collections.Generic;
using System.Linq;
using FD = EEMathLib.LoadFlow.NewtonRaphson.LFFastDecoupled;
using LFC = EEMathLib.LoadFlow.LFCommon;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.LFNewtonRaphson;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    public class NRExample
    {
        #region Based on LFData dataset

        public static bool Calc_PQDelta(LFData data)
        {
            var nw = data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            var bus2 = nrBuses.Buses.FirstOrDefault(b => b.ID == "2");

            var mxPQdelta = LFNR.CalcDeltaPQ(nw.YMatrix, nrBuses);
            var rowCount = mxPQdelta.RowCount;
            var c = rowCount == nrBuses.JSize.Row;

            var pdk = mxPQdelta[bus2.Pidx, 0];
            var res = -7.99972;
            c = c && Checker.EQ(pdk, res, 0.001);

            return c;
        }

        public static bool Calc_J1_Partial(LFData data)
        {
            var nw = data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            var J1 = Jacobian.CreateJ1(nw.YMatrix, nrBuses);
            var res = MX.ParseMatrix(data.J1Result);
            var bus2 = nrBuses.Buses.FirstOrDefault(b => b.ID == "2");
            var bus4 = nrBuses.Buses.FirstOrDefault(b => b.ID == "4");
            var j24 = J1[bus2.Pidx, bus4.Aidx];
            //var r24 = -9.91964;
            var d24 = data.GetJ1kn(bus2, bus4);
            var c = Checker.EQ(j24, d24, 0.001);
            c = c && J1.RowCount == nrBuses.J1Size.Row 
                && J1.ColumnCount == nrBuses.J1Size.Col;

            return c;
        }

        #endregion

        #region JMatrix

        /// <summary>
        /// Test the calculation of JMatrix
        /// </summary>
        public static bool Calc_JMatrix(ILFData data, 
            bool j1, bool j2, bool j3, bool j4)
        {
            var nw = data.CreateNetwork();
            // using YMatrix data instead of calculated value
            nw.YMatrix = MX.ParseMatrix(data.YResult); 
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            var c = true;
            if (j1)
            {
                var J1 = Jacobian.CreateJ1(nw.YMatrix, nrBuses);
                var res = MX.ParseMatrix(data.J1Result);
                var v = Checker.EQ(J1, res, 0.0001);
                c = c && v.Valid;
            }
            if (j2)
            {
                var J2 = Jacobian.CreateJ2(nw.YMatrix, nrBuses);
                var res = MX.ParseMatrix(data.J2Result);
                var v = Checker.EQ(J2, res, 0.0001);
                c = c && v.Valid;
            }
            if (j3)
            {
                var J3 = Jacobian.CreateJ3(nw.YMatrix, nrBuses);
                var res = MX.ParseMatrix(data.J3Result);
                var v = Checker.EQ(J3, res, 0.0001);
                c = c && v.Valid;
            }
            if (j4)
            {
                var J4 = Jacobian.CreateJ4(nw.YMatrix, nrBuses);
                var res = MX.ParseMatrix(data.J4Result);
                var v = Checker.EQ(J4, res, 0.0001);
                c = c && v.Valid;
            }

            return c;
        }

        /// <summary>
        /// Test the calculation of JMatrix
        /// </summary>
        public static bool Calc_JMatrix(ILFData data) => 
            Calc_JMatrix(data, true, true, true, true);

        #endregion

        #region JMatrix Entries

        /// <summary>
        /// Test the calculation diagonal entries of JMatrix.
        /// </summary>
        public static bool Calc_Jkk(ILFData data,
            bool j1, bool j2, bool j3, bool j4)
        {
            var nw = data.CreateNetwork();
            // using YMatrix data instead of calculated value
            nw.YMatrix = MX.ParseMatrix(data.YResult);
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var lstErr = new List<(string JID, string BusID, double value)>();

            var c = true;

            #region J1

            if (j1)
            {
                var res = MX.ParseMatrix(data.J1Result);
                foreach (var bk in nrBuses.Buses)
                    foreach (var bn in nrBuses.Buses)
                    {
                        if (bk.Pidx != bn.Aidx)
                            continue;

                        var j1kk = Jacobian.CalcJ1kk(bk, nw.YMatrix, nrBuses.Buses);
                        var r1kk = data.GetJ1kk(bk, res);
                        var v = Checker.EQ(j1kk, r1kk, 0.5);
                        if (!v)
                            lstErr.Add(("J1", bk.ID, j1kk));
                        c = c && v;
                    }
            }

            #endregion

            #region J2

            if (j2)
            {
                var res = MX.ParseMatrix(data.J2Result);
                foreach (var bk in nrBuses.Buses)
                    foreach (var bn in nrBuses.PQBuses)
                    {
                        if (bk.Pidx != bn.Vidx)
                            continue;

                        var j2kk = Jacobian.CalcJ2kk(bk, nw.YMatrix, nrBuses.Buses);
                        var r2kk = res[bk.Pidx, bn.Vidx];
                        var v = Checker.EQ(j2kk, r2kk, 0.5);
                        if (!v)
                            lstErr.Add(("J2", bk.ID, j2kk));
                        c = c && v;
                    }
            }

            #endregion

            #region J3

            if (j3)
            {
                var res = MX.ParseMatrix(data.J3Result);
                foreach (var bk in nrBuses.PQBuses)
                    foreach (var bn in nrBuses.Buses)
                    {
                        if (bk.Qidx != bn.Aidx)
                            continue;

                        var j3kk = Jacobian.CalcJ3kk(bk, nw.YMatrix, nrBuses.Buses);
                        var r3kk = res[bk.Qidx, bn.Aidx];
                        var v = Checker.EQ(j3kk, r3kk, 0.5);
                        if (!v)
                            lstErr.Add(("J3", bk.ID, j3kk));
                        c = c && v;
                    }
            }

            #endregion

            #region J4

            if (j4)
            {
                var res = MX.ParseMatrix(data.J4Result);
                foreach (var bk in nrBuses.PQBuses)
                    foreach (var bn in nrBuses.PQBuses)
                    {
                        if (bk.Qidx != bn.Vidx)
                            continue;

                        var j4kk = Jacobian.CalcJ4kk(bk, nw.YMatrix, nrBuses.Buses);
                        var r4kk = res[bk.Qidx, bn.Vidx];
                        var v = Checker.EQ(j4kk, r4kk, 0.5);
                        if (!v)
                            lstErr.Add(("J4", bk.ID, j4kk));
                        c = c && v;
                    }
            }

            #endregion

            return c;
        }

        /// <summary>
        /// Test the calculation the off-diagonal entries of JMatrix.
        /// </summary>
        public static bool Calc_Jkn(ILFData data,
            bool j1, bool j2, bool j3, bool j4)
        {
            var nw = data.CreateNetwork();
            // using YMatrix data instead of calculated value
            nw.YMatrix = MX.ParseMatrix(data.YResult);
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var lstErr = new List<(string JID, string RowID, string ColID)>();

            var c = true;

            #region J1

            if (j1)
            {
                var res = MX.ParseMatrix(data.J1Result);
                foreach (var bk in nrBuses.Buses)
                    foreach(var bn in nrBuses.Buses.Where(b => b.ID != bk.ID))
                    {
                        var jk = bk.Pidx;
                        var j1kk = Jacobian.CalcJ1kn(bk, bn, nw.YMatrix);
                        var r1kk = data.GetJ1kn(bk, bn, res);
                        var v = Checker.EQ(j1kk, r1kk, 0.01);
                        if (!v)
                            lstErr.Add(("J1", bk.ID, bn.ID));
                        c = c && v;
                    }
            }

            #endregion

            #region J2

            if (j2)
            {
                var res = MX.ParseMatrix(data.J2Result);
                foreach (var bk in nrBuses.Buses)
                    foreach (var bn in nrBuses.PQBuses.Where(b => b.ID != bk.ID))
                    {
                        var jk = bk.Pidx;
                        var j1kk = Jacobian.CalcJ2kn(bk, bn, nw.YMatrix);
                        var r1kk = data.GetJ1kn(bk, bn, res);
                        var v = Checker.EQ(j1kk, r1kk, 0.01);
                        if (!v)
                            lstErr.Add(("J2", bk.ID, bn.ID));
                        c = c && v;
                    }
            }

            #endregion

            #region J3

            if (j3)
            {
                var res = MX.ParseMatrix(data.J3Result);
                foreach (var bk in nrBuses.PQBuses)
                    foreach (var bn in nrBuses.Buses.Where(b => b.ID != bk.ID))
                    {
                        var jk = bk.Pidx;
                        var j1kk = Jacobian.CalcJ1kn(bk, bn, nw.YMatrix);
                        var r1kk = data.GetJ1kn(bk, bn, res);
                        var v = Checker.EQ(j1kk, r1kk, 0.01);
                        if (!v)
                            lstErr.Add(("J3", bk.ID, bn.ID));
                        c = c && v;
                    }
            }

            #endregion

            #region J4

            if (j4)
            {
                var res = MX.ParseMatrix(data.J4Result);
                foreach (var bk in nrBuses.PQBuses)
                    foreach (var bn in nrBuses.PQBuses.Where(b => b.ID != bk.ID))
                    {
                        var jk = bk.Pidx;
                        var j1kk = Jacobian.CalcJ1kn(bk, bn, nw.YMatrix);
                        var r1kk = data.GetJ1kn(bk, bn, res);
                        var v = Checker.EQ(j1kk, r1kk, 0.01);
                        if (!v)
                            lstErr.Add(("J4", bk.ID, bn.ID));
                        c = c && v;
                    }
            }

            #endregion

            return c;
        }

        #endregion

        #region Solve

        public static bool LFSolve(ILFData data)
        {

            var nw = data.CreateNetwork();
            var threshold = 0.001;
            var res = LFNR.Solve(nw, threshold, 10, 3);

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

        public static bool LFSolve_FastDecoupled(ILFData data)
        {

            var nw = data.CreateNetwork();
            var threshold = 0.001;
            var res = FD.Solve(nw, threshold, 10, 3);

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
