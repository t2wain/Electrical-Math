using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Linq;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.NewtonRaphsonBase;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;


namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Allow debuging and validation of all calculation steps
    /// in the Newton-Raphson load flow algorithm
    /// </summary>
    internal static class NRValidator
    {        
        #region Iterate

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
        internal static NRResult LFIterate(MC YMatrix, NRResult data, int steps, double threshold = 0.0001)
        {
            var solver = new LFNewtonRaphson();
            var res = new NRResult();
            res.Iteration = data.Iteration;
            var Y = YMatrix;

            // Determine classification of each bus
            res.NRBuses = JC.ReIndexBusPQ(data.NRBuses.AllBuses);

            // Step 1
            if (data.PQDelta != null)
            {
                res.PQDelta = data.PQDelta;
                res.PCal = data.PCal;
                res.QCal = data.QCal;
            }
            else LFNR.CalcDeltaPQ(Y, res); // delta P and Q

            res.MaxErr = res.PQDelta
                .ToColumnMajorArray()
                .Select(v => Math.Abs(v))
                .Max();

            if (res.MaxErr <= threshold)
            {
                res.IsSolution = true;
                return res;
            }

            if (steps <= 1)
                return res;

            // Step 2
            // Calculate Jacobian matrix
            if (data.JMatrix != null)
                res.JMatrix = data.JMatrix;
            else res.JMatrix = JC.CreateJMatrix(Y, res.NRBuses);

            if (steps <= 2)
                return res;

            // Step 3
            solver.CalcAVDelta(res);

            if (steps <= 3)
                return res;

            // Step 4
            LFNR.UpdateBusAV(res);

            if (steps <= 4)
                return res;

            // Step 5
            LFNR.UpdatePVBusStatus(Y, res);

            return res;
        }

        #endregion

        #region Validate

        /// <summary>
        /// Validate the result of various calculation steps
        /// against the data
        /// </summary>
        internal static bool ValidateIteration(NRResult res, ILFData data)
        {
            var c = true;
            var v = true;
            if (c)
            {
                v = Validate_PQCalc(res, data);
                c &= v;
            }
            if (c)
            {
                v = Validate_PQDelta(res.PQDelta, data, res.Iteration);
                c &= v;
            }
            if (c)
            {
                v = Validate_JMatrix(res, data);
                c &= v;
            }
            if (c)
            {
                v = Validate_AVDelta(res, data);
                c &= v;
            }
            if (c)
            {
                v = Validate_AVBus(res, data);
                c &= v;
            }

            return c;
        }

        /// <summary>
        /// Validate the result calculation of Jacobian matrix
        /// against the data
        /// </summary>
        internal static bool Validate_JMatrix(NRResult nrRes, ILFData data)
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

        /// <summary>
        /// Validate the calculation of Jacobian matrix against the data
        /// </summary>
        internal static bool Validate_JMatrix(Matrix<double> J1, Matrix<double> J2,
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

        /// <summary>
        /// Validate the result of the calculation step against the data
        /// </summary>
        internal static bool Validate_PQDelta(Matrix<double> mxPQDelta, ILFData data, int iteration)
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

        /// <summary>
        /// Validate the result of the calculation step against the data
        /// </summary>
        internal static bool Validate_PQCalc(NRResult nrRes, ILFData data)
        {
            var c = true;
            var JRes = data.GetNewtonRaphsonData(nrRes.Iteration);

            var v = Checker.EQ(nrRes.PCal, JRes.PCal, 0.0001);
            c &= v.Valid;

            v = Checker.EQ(nrRes.QCal, JRes.QCal, 0.0001);
            c &= v.Valid;

            return c;
        }

        /// <summary>
        /// Validate the result of the calculation step against the data
        /// </summary>
        internal static bool Validate_AVDelta(NRResult nrRes, ILFData data)
        {
            var c = true;
            var JRes = data.GetNewtonRaphsonData(nrRes.Iteration);

            var v = Checker.EQ(nrRes.ADelta, JRes.ADelta, 0.0001);
            c &= v.Valid;

            v = Checker.EQ(nrRes.VDelta, JRes.VDelta, 0.0001);
            c &= v.Valid;

            return c;
        }

        /// <summary>
        /// Validate the result of the calculation step against the data
        /// </summary>
        internal static bool Validate_AVBus(NRResult nrRes, ILFData data)
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
