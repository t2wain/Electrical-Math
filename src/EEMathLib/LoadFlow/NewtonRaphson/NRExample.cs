using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using LFC = EEMathLib.LoadFlow.LFCommon;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.LFNewtonRaphson;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

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

            var nrRes = LFNR.CalcDeltaPQ(nw.YMatrix, nrBuses);
            var mxPQdelta = nrRes.PQDelta;
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
            var YMatrix = MX.ParseMatrix(data.YResult);

            var c = true;
            var v = false;
            NRResult nrRes;
            if (iteration == 0)
            {
                var nrData = NRResult.Parse(data, 0);
                nrRes = new NRResult
                {
                    NRBuses = nrData.NRBuses
                };
                nrRes = LFIterate(YMatrix, nrRes, 2);
                v = Validate_PQCalc(nrRes, data);
                v = Validate_PQDelta(nrRes.PQDelta, data, iteration);
                c &= v;
            }
            
            if (iteration >= 1)
            {
                var nrData = NRResult.Parse(data, 1);
                nrRes = new NRResult
                {
                    JMatrix = nrData.JMatrix,
                    NRBuses = nrData.NRBuses
                };
                nrRes = LFIterate(YMatrix, nrRes, 7);
                nrRes.Iteration = 1;
                v = Validate_AVDelta(nrRes, data);
                c &= v;

                if (c)
                {
                    v = Validate_AVBus(nrRes, data);
                    c &= v;
                }
                if (c)
                {
                    nrRes.PQDelta = null;
                    nrRes = LFIterate(YMatrix, nrRes, 2);
                    v = Validate_PQCalc(nrRes, data);
                    c &= v;
                }
                if (c)
                {
                    v = Validate_PQDelta(nrRes.PQDelta, data, iteration);
                    c &= v;
                }
            }

            if (iteration >= 2)
            {
                var nrData = NRResult.Parse(data, 1);
                nrRes = new NRResult
                {
                    JMatrix = nrData.JMatrix,
                    NRBuses = nrData.NRBuses
                };
                nrRes = LFIterate(YMatrix, nrRes, 7);
                nrRes.Iteration = 1;
                v = Validate_AVDelta(nrRes, data);

                nrRes = LFIterate(YMatrix, nrRes, 2);
                v = Validate_PQCalc(nrRes, data);
                v = Validate_PQDelta(nrRes.PQDelta, data, iteration);
                c &= v;
            }



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

            var c = Validate_JMatrix(nrRes, data);
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

        #region Iteration

        public static bool LFSolve(ILFData data, int numberOfIteration)
        {
            var nw = data.CreateNetwork();
            nw.YMatrix = MX.ParseMatrix(data.YResult);
            var buses = LFNR.Initialize(nw.Buses);

            var c = true;

            if (numberOfIteration <= 1)
            {
                // iteration 1
                var nrResData = NRResult.Parse(data, 1);
                var res = new NRResult { Iteration = 1, JMatrix = nrResData.JMatrix };
                res.NRBuses = JC.ReIndexBusPQ(buses);
                var v = Validate_PQCalc(res, data);
                c &= v;
            }

            if (c && numberOfIteration <= 2)
            {
                // iteration 2
                var res = LFNR.Iterate(buses, nw.YMatrix);
                res.Iteration = 2;
                var v = ValidateIteration(res, data);
                c &= v;
            }

            if (c && numberOfIteration <= 3)
            {
                // iteration 3
                var res = LFNR.Iterate(buses, nw.YMatrix);
                res.Iteration = 3;
                var v = ValidateIteration(res, data);
                c &= v;
            }

            if (numberOfIteration > 3)
            {
                throw new System.Exception();
            }

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

        /// <summary>
        /// Each iteration include multiple sequential steps of calculation. The result
        /// of previous steps might be the input data for the next steps. This method
        /// allow the result of each step either to be provided from the data or to be
        /// calculated based on the input data of previous steps so that the individual 
        /// calculated result of various steps can be validated independently.
        /// </summary>
        /// <param name="data">The data for the current iteration. If a property 
        /// of the data is NOT included for this iteration, then it will be calculated 
        /// and such calculated result can be validated against the data.</param>
        /// <param name="steps">The number of sequential calculation steps to be executed 
        /// then exit the iteration. The remain steps will be skipped.</param>
        /// <returns>All the results of completed calculation steps.</returns>
        public static NRResult LFIterate(MC YMatrix, NRResult data, int steps, double threshold = 0.0001)
        {
            var res = new NRResult();
            res.Iteration = data.Iteration;
            var Y = YMatrix;

            // Step 1
            // Determine classification of each bus
            res.NRBuses = JC.ReIndexBusPQ(data.NRBuses.AllBuses);

            if (steps <= 1)
                return res;

            // Step 2
            if (data.PQDelta != null)
            {
                res.PQDelta = data.PQDelta;
                res.PCal = data.PCal;
                res.QCal = data.QCal;
            }
            else LFNR.CalcDeltaPQ(Y, res); // delta P and Q

            if (steps <= 2)
                return res;

            res.MaxErr = res.PQDelta
                .ToColumnMajorArray()
                .Select(v => Math.Abs(v))
                .Max();

            if (res.MaxErr <= threshold)
            {
                res.IsSolution = true;
                return res;
            }

            // Step 3
            LFNR.UpdatePVBusStatus(res);

            if (steps <= 3)
                return res;

            // Step 4
            // Determine classification of each bus
            // PV bus classification might have changed in step 3
            res.NRBuses = JC.ReIndexBusPQ(data.NRBuses.AllBuses);

            if (steps <= 4)
                return res;

            // Step 5
            // Calculate Jacobian matrix
            if (data.JMatrix != null)
                res.JMatrix = data.JMatrix;
            else res.JMatrix = JC.CreateJMatrix(Y, res.NRBuses);

            if (steps <= 5)
                return res;

            // Step 6
            LFNR.CalcAVDelta(res);

            if (steps <= 6)
                return res;

            // Step 7
            LFNR.UpdateBusAV(res);

            return res;
        }

        #endregion

        #region Validate

        public static bool ValidateIteration(NRResult res, ILFData data)
        {
            var v = Validate_JMatrix(res, data);
            v = Validate_PQCalc(res, data);
            v = Validate_PQDelta(res.PQDelta, data, res.Iteration);
            v = Validate_AVDelta(res, data);

            return true;
        }

        public static bool Validate_JMatrix(NRResult nrRes, ILFData data)
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

            var v = Validate_JMatrix(J1, J2, J3, J4, data, nrRes.Iteration);
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

        public static bool Validate_PQCalc(NRResult nrRes, ILFData data)
        {
            var c = true;
            var JRes = data.GetNewtonRaphsonData(nrRes.Iteration);

            var v = Checker.EQ(nrRes.PCal, JRes.PCal, 0.0001);
            c &= v.Valid;

            v = Checker.EQ(nrRes.QCal, JRes.QCal, 0.0001);
            c &= v.Valid;

            return c;
        }

        public static bool Validate_AVDelta(NRResult nrRes, ILFData data)
        {
            var c = true;
            var JRes = data.GetNewtonRaphsonData(nrRes.Iteration);

            var v = Checker.EQ(nrRes.ADelta, JRes.ADelta, 0.0001);
            c &= v.Valid;

            v = Checker.EQ(nrRes.VDelta, JRes.VDelta, 0.0001);
            c &= v.Valid;

            return c;
        }

        public static bool Validate_AVBus(NRResult nrRes, ILFData data)
        {
            var c = true;
            var JRes = data.GetNewtonRaphsonData(nrRes.Iteration);

            var v = Checker.EQ(nrRes.ABus, JRes.ABus, 0.0001);
            c &= v.Valid;

            v = Checker.EQ(nrRes.VBus, JRes.VBus, 0.0001);
            c &= v.Valid;

            return c;
        }

        #endregion
    }
}
