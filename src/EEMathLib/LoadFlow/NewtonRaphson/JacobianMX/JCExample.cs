using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using JC = EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.Jacobian;

namespace EEMathLib.LoadFlow.NewtonRaphson.JacobianMX
{
    /// <summary>
    /// Validate the calculation of Jacobian matrix
    /// </summary>
    public static class JCExample
    {

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
            var buses = NewtonRaphsonBase.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            Matrix<double> J1, J2, J3, J4;
            J1 = J2 = J3 = J4 = null;
            var jc = new Jacobian();
            if (j1) J1 = jc.CreateJ1(nw.YMatrix, nrBuses);
            if (j2) J2 = jc.CreateJ2(nw.YMatrix, nrBuses);
            if (j3) J3 = jc.CreateJ3(nw.YMatrix, nrBuses);
            if (j4) J4 = jc.CreateJ4(nw.YMatrix, nrBuses);
            var c = NRValidator.Validate_JMatrix(J1, J2, J3, J4, data, iteration);

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
            var buses = NewtonRaphsonBase.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            var jc = new Jacobian();
            var J = jc.CreateJMatrix(nw.YMatrix, nrBuses);
            var nrRes = new NRResult
            {
                JMatrix = J,
                NRBuses = nrBuses
            };

            var c = NRValidator.Validate_JMatrix(nrRes, data);
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
            var buses = NewtonRaphsonBase.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var lstErr = new List<(string JID, string BusID, double value)>();
            var JRes = data.GetNewtonRaphsonData(iteration).JacobianData;

            var c = true;
            var err = 0.001;
            var jc = new Jacobian();

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

                        var j1kk = jc.CalcJ1kk(bk, nw.YMatrix, nrBuses);
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

                        var j2kk = jc.CalcJ2kk(bk, nw.YMatrix, nrBuses);
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

                        var j3kk = jc.CalcJ3kk(bk, nw.YMatrix, nrBuses);
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

                        var j4kk = jc.CalcJ4kk(bk, nw.YMatrix, nrBuses);
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
            var buses = NewtonRaphsonBase.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);
            var lstErr = new List<(string JID, string RowID, string ColID)>();
            var JRes = data.GetNewtonRaphsonData(iteration).JacobianData;

            var c = true;
            var err = 0.001;
            var jc = new Jacobian();

            #region J1

            if (j1)
            {
                var res = MX.ParseMatrix(JRes.J1Result);
                foreach (var bk in nrBuses.Buses)
                    foreach (var bn in nrBuses.Buses.Where(b => b.BusData.BusIndex != bk.BusData.BusIndex))
                    {
                        var jk = bk.Pidx;
                        var j1kk = jc.CalcJ1kn(bk, bn, nw.YMatrix);
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
                        var j1kk = jc.CalcJ2kn(bk, bn, nw.YMatrix);
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
                        var j1kk = jc.CalcJ3kn(bk, bn, nw.YMatrix);
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
                        var j1kk = jc.CalcJ4kn(bk, bn, nw.YMatrix);
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

    }
}
