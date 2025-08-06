using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using System.Collections.Generic;
using System.Linq;
using FD = EEMathLib.LoadFlow.NewtonRaphson.LFFastDecoupled;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using LFC = EEMathLib.LoadFlow.LFCommon;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.LFNewtonRaphson;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Tests for various calculations
    /// in the Newton-Raphson algorithm
    /// </summary>
    public class NRExample
    {
        #region Based on LFData dataset

        public static bool Calc_PQDelta_Partial(LFData data)
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

            var J1 = JC.CreateJ1(nw.YMatrix, nrBuses);
            var JRes = data.GetNewtonRaphsonData().JacobianData;
            var res = MX.ParseMatrix(JRes.J1Result);
            var bus2 = nrBuses.Buses.FirstOrDefault(b => b.ID == "2");
            var bus4 = nrBuses.Buses.FirstOrDefault(b => b.ID == "4");
            var j24 = J1[bus2.Pidx, bus4.Aidx];
            //var r24 = -9.91964;
            var d24 = JRes.GetJ1kn(bus2, bus4);
            var c = Checker.EQ(j24, d24, 0.001);
            c = c && J1.RowCount == nrBuses.J1Size.Row 
                && J1.ColumnCount == nrBuses.J1Size.Col;

            return c;
        }

        #endregion

        public static bool Calc_PQ(ILFData data, int iteration)
        {
            var res = data.GetNewtonRaphsonData(iteration);
            var nw = data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var c = true;
            var YMatrix = MX.ParseMatrix(data.YResult);
            foreach (var b in nrBuses.Buses) { 
                var sb = LFC.CalcPower(b, YMatrix, nrBuses.AllBuses);
                var PCalc = sb.Real;
                var PRes = res.PCal[b.Pidx];
                c &= Checker.EQ(PCalc, PRes, 0.0001);
                if (b.BusType == BusTypeEnum.PQ)
                {
                    var QCalc = sb.Imaginary;
                    var QRes = res.QCal[b.Qidx];
                    c &= Checker.EQ(QCalc, QRes, 0.0001);
                }
            }
            return c;
        }

        #region JMatrix

        /// <summary>
        /// Test the calculation of JMatrix
        /// </summary>
        public static bool Calc_JMatrix(ILFData data, 
            bool j1, bool j2, bool j3, bool j4, int iteration)
        {
            var nw = data.CreateNetwork();
            // using YMatrix data instead of calculated value
            nw.YMatrix = MX.ParseMatrix(data.YResult); 
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var JRes = data.GetNewtonRaphsonData(iteration).JacobianData;

            var c = true;
            if (j1)
            {
                var J1 = JC.CreateJ1(nw.YMatrix, nrBuses);
                var res = MX.ParseMatrix(JRes.J1Result);
                var v = Checker.EQ(J1, res, 0.0001);
                c = c && v.Valid;
            }
            if (j2)
            {
                var J2 = JC.CreateJ2(nw.YMatrix, nrBuses);
                var res = MX.ParseMatrix(JRes.J2Result);
                var v = Checker.EQ(J2, res, 0.0001);
                c = c && v.Valid;
            }
            if (j3)
            {
                var J3 = JC.CreateJ3(nw.YMatrix, nrBuses);
                var res = MX.ParseMatrix(JRes.J3Result);
                var v = Checker.EQ(J3, res, 0.0001);
                c = c && v.Valid;
            }
            if (j4)
            {
                var J4 = JC.CreateJ4(nw.YMatrix, nrBuses);
                var res = MX.ParseMatrix(JRes.J4Result);
                var v = Checker.EQ(J4, res, 0.0001);
                c = c && v.Valid;
            }

            return c;
        }

        /// <summary>
        /// Test the calculation of JMatrix
        /// </summary>
        public static bool Calc_JMatrix(ILFData data, int iteration) => 
            Calc_JMatrix(data, true, true, true, true, iteration);

        #endregion

        #region JMatrix Jkk Entries

        /// <summary>
        /// Test the calculation diagonal entries of JMatrix.
        /// </summary>
        public static bool Calc_Jkk(ILFData data,
            bool j1, bool j2, bool j3, bool j4, int iteration)
        {
            var nw = data.CreateNetwork();
            // using YMatrix data instead of calculated value
            nw.YMatrix = MX.ParseMatrix(data.YResult);
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var lstErr = new List<(string JID, string BusID, double value)>();
            var JRes = data.GetNewtonRaphsonData(iteration).JacobianData;

            var c = true;
            var err = 0.001;
            #region J1

            if (j1)
            {
                var res = MX.ParseMatrix(JRes.J1Result);
                foreach (var bk in nrBuses.Buses)
                    foreach (var bn in nrBuses.Buses)
                    {
                        var pidx = bk.Pidx;
                        var aidx = bn.Aidx;

                        //if (bk.Pidx != bn.Aidx)
                        //    continue;
                        if (bk.BusData.BusIndex != bn.BusData.BusIndex)
                            continue;

                        var j1kk = JC.CalcJ1kk(bk, nw.YMatrix, nrBuses);
                        var r1kk = JRes.GetJ1kk(bk, res);
                        var v = Checker.EQ(j1kk, r1kk, err);
                        if (!v)
                            lstErr.Add(("J1", bk.ID, j1kk));
                        c = c && v;
                    }
            }

            #endregion

            #region J2

            if (j2)
            {
                var res = MX.ParseMatrix(JRes.J2Result);
                foreach (var bk in nrBuses.Buses)
                    foreach (var bn in nrBuses.PQBuses)
                    {
                        var pidx = bk.Pidx;
                        var vidx = bn.Vidx;

                        //if (bk.Pidx != bn.Vidx)
                        //    continue;
                        if (bk.BusData.BusIndex != bn.BusData.BusIndex)
                            continue;

                        var j2kk = JC.CalcJ2kk(bk, nw.YMatrix, nrBuses);
                        var r2kk = res[bk.Pidx, bn.Vidx];
                        var v = Checker.EQ(j2kk, r2kk, err);
                        if (!v)
                            lstErr.Add(("J2", bk.ID, j2kk));
                        c = c && v;
                    }
            }

            #endregion

            #region J3

            if (j3)
            {
                var res = MX.ParseMatrix(JRes.J3Result);
                foreach (var bk in nrBuses.PQBuses)
                    foreach (var bn in nrBuses.Buses)
                    {
                        var qidx = bk.Qidx;
                        var aidx = bn.Aidx;

                        //if (bk.Qidx != bn.Aidx)
                        //    continue;
                        if (bk.BusData.BusIndex != bn.BusData.BusIndex)
                            continue;

                        var j3kk = JC.CalcJ3kk(bk, nw.YMatrix, nrBuses);
                        var r3kk = res[bk.Qidx, bn.Aidx];
                        var v = Checker.EQ(j3kk, r3kk, err);
                        if (!v)
                            lstErr.Add(("J3", bk.ID, j3kk));
                        c = c && v;
                    }
            }

            #endregion

            #region J4

            if (j4)
            {
                var res = MX.ParseMatrix(JRes.J4Result);
                foreach (var bk in nrBuses.PQBuses)
                    foreach (var bn in nrBuses.PQBuses)
                    {
                        var qidx = bk.Qidx;
                        var vidx = bn.Vidx;

                        //if (bk.Qidx != bn.Vidx)
                        //    continue;
                        if (bk.BusData.BusIndex != bn.BusData.BusIndex)
                            continue;

                        var j4kk = JC.CalcJ4kk(bk, nw.YMatrix, nrBuses);
                        var r4kk = res[bk.Qidx, bn.Vidx];
                        var v = Checker.EQ(j4kk, r4kk, err);
                        if (!v)
                            lstErr.Add(("J4", bk.ID, j4kk));
                        c = c && v;
                    }
            }

            #endregion

            return c;
        }

        #endregion

        #region JMatrix Jkn Entries

        /// <summary>
        /// Test the calculation the off-diagonal entries of JMatrix.
        /// </summary>
        public static bool Calc_Jkn(ILFData data,
            bool j1, bool j2, bool j3, bool j4, int iteration)
        {
            var nw = data.CreateNetwork();
            // using YMatrix data instead of calculated value
            nw.YMatrix = MX.ParseMatrix(data.YResult);
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var lstErr = new List<(string JID, string RowID, string ColID)>();
            var JRes = data.GetNewtonRaphsonData(iteration).JacobianData;

            var c = true;
            var err = 0.001;

            #region J1

            if (j1)
            {
                var res = MX.ParseMatrix(JRes.J1Result);
                foreach (var bk in nrBuses.Buses)
                    foreach(var bn in nrBuses.Buses.Where(b => b.BusData.BusIndex != bk.BusData.BusIndex))
                    {
                        var jk = bk.Pidx;
                        var j1kk = JC.CalcJ1kn(bk, bn, nw.YMatrix);
                        var r1kk = JRes.GetJ1kn(bk, bn, res);
                        var v = Checker.EQ(j1kk, r1kk, err);
                        if (!v)
                            lstErr.Add(("J1", bk.ID, bn.ID));
                        c = c && v;
                    }
            }

            #endregion

            #region J2

            if (j2)
            {
                var res = MX.ParseMatrix(JRes.J2Result);
                foreach (var bk in nrBuses.Buses)
                    foreach (var bn in nrBuses.PQBuses.Where(b => b.BusData.BusIndex != bk.BusData.BusIndex))
                    {
                        var jk = bk.Pidx;
                        var j1kk = JC.CalcJ2kn(bk, bn, nw.YMatrix);
                        var r1kk = res[bk.Pidx, bn.Vidx];
                        var v = Checker.EQ(j1kk, r1kk, err);
                        if (!v)
                            lstErr.Add(("J2", bk.ID, bn.ID));
                        c = c && v;
                    }
            }

            #endregion

            #region J3

            if (j3)
            {
                var res = MX.ParseMatrix(JRes.J3Result);
                foreach (var bk in nrBuses.PQBuses)
                    foreach (var bn in nrBuses.Buses.Where(b => b.BusData.BusIndex != bk.BusData.BusIndex))
                    {
                        var jk = bk.Pidx;
                        var j1kk = JC.CalcJ3kn(bk, bn, nw.YMatrix);
                        var r1kk = res[bk.Qidx, bn.Aidx];
                        var v = Checker.EQ(j1kk, r1kk, err);
                        if (!v)
                            lstErr.Add(("J3", bk.ID, bn.ID));
                        c = c && v;
                    }
            }

            #endregion

            #region J4

            if (j4)
            {
                var res = MX.ParseMatrix(JRes.J4Result);
                foreach (var bk in nrBuses.PQBuses)
                    foreach (var bn in nrBuses.PQBuses.Where(b => b.BusData.BusIndex != bk.BusData.BusIndex))
                    {
                        var jk = bk.Pidx;
                        var j1kk = JC.CalcJ4kn(bk, bn, nw.YMatrix);
                        var r1kk = res[bk.Qidx, bn.Vidx];
                        var v = Checker.EQ(j1kk, r1kk, err);
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
            var res = LFNR.Solve(nw, threshold, 50);

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
