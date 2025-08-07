using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
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

        #region PQ

        public static bool Calc_PQ(ILFData data, int iteration)
        {
            var nw = data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            //var c = true;
            var YMatrix = MX.ParseMatrix(data.YResult);

            var nrRes = new NRResult 
            { 
                Iteration = iteration, 
                PCal = new double[nrBuses.Buses.Count()],
                QCal = new double[nrBuses.PQBuses.Count()]
            };
            foreach (var b in nrBuses.Buses) { 
                var sb = LFC.CalcPower(b, YMatrix, nrBuses.AllBuses);
                nrRes.PCal[b.Pidx] = sb.Real;
                if (b.BusType == BusTypeEnum.PQ)
                {
                    nrRes.QCal[b.Qidx] = sb.Imaginary;
                }
            }

            var c = Validate_PQCalc(nrRes, data, iteration);

            return c;
        }

        public static bool Calc_DeltaPQ(ILFData data, int iteration)
        {
            var nw = data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var YMatrix = MX.ParseMatrix(data.YResult);

            var mxDeltaPQ = LFNR.CalcDeltaPQ(YMatrix, nrBuses);
            var c = Validate_PQDelta(mxDeltaPQ, data, iteration);
            return c;
        }

        #endregion

        #region JMatrix

        /// <summary>
        /// Test the calculation of JMatrix
        /// </summary>
        public static bool Calc_J1_J2_J3_J4(ILFData data, 
            bool j1, bool j2, bool j3, bool j4, int iteration)
        {
            var nw = data.CreateNetwork();
            // using YMatrix data instead of calculated value
            nw.YMatrix = MX.ParseMatrix(data.YResult); 
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            Matrix<double> J1, J2, J3, J4;
            J1 = J2 = J3 = J4 = null;
            if (j1) J1 = JC.CreateJ1(nw.YMatrix, nrBuses);
            if (j2) J2 = JC.CreateJ2(nw.YMatrix, nrBuses);
            if (j3) J3 = JC.CreateJ3(nw.YMatrix, nrBuses);
            if (j4) J4 = JC.CreateJ4(nw.YMatrix, nrBuses);
            var c = Validate_JMatrix(J1, J2, J3, J4, data, iteration);

            return c;
        }

        /// <summary>
        /// Test the calculation of JMatrix
        /// </summary>
        public static bool Calc_J1_J2_J3_J4(ILFData data, int iteration) => 
            Calc_J1_J2_J3_J4(data, true, true, true, true, iteration);

        /// <summary>
        /// Test the calculation of JMatrix
        /// </summary>
        public static bool Calc_JMatrix(ILFData data, int iteration)
        {
            var nw = data.CreateNetwork();
            // using YMatrix data instead of calculated value
            nw.YMatrix = MX.ParseMatrix(data.YResult);
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            var J = JC.CreateJMatrix(nw.YMatrix, nrBuses);
            var nrRes = new NRResult
            {
                JMatrix = J,
                NRBuses = nrBuses
            };

            var c = Validate_JMatrix(nrRes, data, iteration);
            return c;
        }

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
        
        public static bool LFIterate3times(ILFData data)
        {
            var nw = data.CreateNetwork();
            nw.YMatrix = MX.ParseMatrix(data.YResult);
            var buses = LFNR.Initialize(nw.Buses);

            var c = true;

            {
                // iteration 1
                var res = LFNR.Iterate(buses, nw.YMatrix);
                res.Iteration = 1;
                var v = ValidateIteration(res, data);
                c &= v;
            }

            if (c)
            {
                // iteration 2
                var res = LFNR.Iterate(buses, nw.YMatrix);
                res.Iteration = 2;
                var v = ValidateIteration(res, data);
                c &= v;
            }

            if (c)
            {
                // iteration 3
                var res = LFNR.Iterate(buses, nw.YMatrix);
                res.Iteration = 3;
                var v = ValidateIteration(res, data);
                c &= v;
            }

            return true;
        }

        #endregion

        #region Validate

        public static bool ValidateIteration(NRResult res, ILFData data)
        {
            var v = Validate_JMatrix(res, data, res.Iteration);
            v = Validate_PQCalc(res, data, res.Iteration);
            v = Validate_PQDelta(res.PQDelta, data, res.Iteration);

            return true;
        }

        public static bool Validate_JMatrix(NRResult nrRes, ILFData data, int iteration)
        {
            var c = true;

            var nrBuses = nrRes.NRBuses;
            var J = nrRes.JMatrix;
            {
                c = c && J.RowCount == nrBuses.JSize.Row;
                c = c && J.ColumnCount == nrBuses.JSize.Col;
            }

            var J1 = J.SubMatrix(0, nrBuses.J1Size.Row, 0, nrBuses.J1Size.Col);
            var J2 = J.SubMatrix(0, nrBuses.J2Size.Row, J1.ColumnCount, nrBuses.J2Size.Col);
            var J3 = J.SubMatrix(J1.RowCount, nrBuses.J3Size.Row, 0, nrBuses.J3Size.Col);
            var J4 = J.SubMatrix(J1.RowCount, nrBuses.J4Size.Row, J1.ColumnCount, nrBuses.J4Size.Col);

            var v = Validate_JMatrix(J1, J2, J3, J4, data, iteration);
            c &= v;

            return c;
        }

        public static bool Validate_JMatrix(Matrix<double> J1, Matrix<double> J2, 
            Matrix<double> J3, Matrix<double> J4, ILFData data, int iteration)
        {
            var err = 0.001;
            var JRes = data.GetNewtonRaphsonData(iteration).JacobianData;

            var c = true;
            if (J1 != null)
            {
                var res = MX.ParseMatrix(JRes.J1Result);
                var v = Checker.EQ(J1, res, err);
                c = c && v.Valid;
            }

            if (J2 != null)
            {
                var res = MX.ParseMatrix(JRes.J2Result);
                var v = Checker.EQ(J2, res, err);
                c = c && v.Valid;
            }

            if (J3 != null)
            {
                var res = MX.ParseMatrix(JRes.J3Result);
                var v = Checker.EQ(J3, res, err);
                c = c && v.Valid;
            }

            if (J4 != null)
            {
                var res = MX.ParseMatrix(JRes.J4Result);
                var v = Checker.EQ(J4, res, err);
                c = c && v.Valid;
            }

            return c;

        }

        public static bool Validate_PQDelta(Matrix<double> mxPQDelta, ILFData data, int iteration)
        {
            var nrData = data.GetNewtonRaphsonData(iteration);
            var c = true;
            var rmx = Matrix<double>.Build.Dense(
                mxPQDelta.RowCount,
                mxPQDelta.ColumnCount, nrData.MDelta
            );
            var v = Checker.EQ(mxPQDelta, rmx, 0.001);
            c &= v.Valid;
            return c;
        }

        public static bool Validate_PQCalc(NRResult nrRes, ILFData data, int iteration)
        {
            var c = true;
            var JRes = data.GetNewtonRaphsonData(iteration);

            var v = Checker.EQ(nrRes.PCal, JRes.PCal, 0.0001);
            c &= v.Valid;

            v = Checker.EQ(nrRes.QCal, JRes.QCal, 0.0001);
            c &= v.Valid;

            return c;
        }

        public static bool Validate_AVDelta(NRResult nrRes, ILFData data, int iteration)
        {
            var c = true;
            var JRes = data.GetNewtonRaphsonData(iteration);

            var v = Checker.EQ(nrRes.ADelta, JRes.ADelta, 0.0001);
            c &= v.Valid;

            v = Checker.EQ(nrRes.VDelta, JRes.VDelta, 0.0001);
            c &= v.Valid;

            return c;
        }

        #endregion
    }
}
