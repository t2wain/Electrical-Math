using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Linq;
using N = System.Numerics;

namespace EEMathLib.MatrixMath
{
    public abstract class MX
    {
        #region Basic API

        //public static MatrixBuilder<double> MXBuilder => Matrix<double>.Build;
        //public static Matrix<double> BuildMX() => Matrix<double>.Build.Dense(3, 3, 0);
        //public static Matrix<double> BuildMX2() => CreateMatrix.Dense<double>(3, 3);
        //public static Vector<double> BuildVec() => VCBuilder.Dense(3, 0);
        //public static Vector<double> BuildVec2() => CreateVector.Dense<double>(3);
        //public static Vector<double> Solve(Matrix<double> A, Vector<double> b) => A.Solve(b);
        //public static VectorBuilder<double> VCBuilder => Vector<double>.Build;

        public static N.Complex C(double re, double im) => new N.Complex(re, im);

        public static Matrix<N.Complex> BuildMX(int rowCount, int colCount, params N.Complex[] vals) => 
            Matrix<N.Complex>.Build.DenseOfRowMajor(rowCount, colCount, vals);

        public static Matrix<double> BuildMX(int rowCount, int colCount, params double[] vals) =>
            Matrix<double>.Build.DenseOfRowMajor(rowCount, colCount, vals);

        #endregion

        public static Matrix<double> ParseMatrix(MxDTO<double> m) =>
            m.EntriesType == MxDTO<double>.COLUMN_ENTRIES ?
                Matrix<double>.Build.Dense(m.RowSize, m.ColumnSize, m.Entries) :
                Matrix<double>.Build.DenseOfRowMajor(m.RowSize, m.ColumnSize, m.Entries);

        public static Matrix<N.Complex> ParseMatrix(MxDTO<N.Complex> m) =>
            m.EntriesType == MxDTO<N.Complex>.COLUMN_ENTRIES ?
                Matrix<N.Complex>.Build.Dense(m.RowSize, m.ColumnSize, m.Entries) :
                Matrix<N.Complex>.Build.DenseOfRowMajor(m.RowSize, m.ColumnSize, m.Entries);

        /// <summary>
        /// Solve Ax = b using Gauss-Seidel iteration method.
        /// </summary>
        /// <param name="maxErr">default to 1e-4</param>
        /// <param name="maxIteration">default to 100</param>
        /// <returns></returns>
        public static Result<Matrix<double>> GaussSeidel(Matrix<double> A, Matrix<double> b, 
            double maxErr = 1e-4, int maxIteration = 100)
        {
            if (A.Diagonal().AsEnumerable().Any(x => x == 0))
                return new Result<Matrix<double>> 
                { 
                    Error = ErrorEnum.ZeroDiagEntry, 
                    ErrorMessage ="Zero(s) in diagonal entries",  
                };

            // x result, initial guess is zero
            var res = CreateMatrix.Dense(b.RowCount, b.ColumnCount, 0.0);
            // track error for each variable
            var verr = CreateVector.Dense(b.RowCount, 0.0);
            // exit condition
            var found = false;

            var i = 0; // iteration counter
            while (!found && i++ < maxIteration)
            {
                // reset
                found = true;
                var dvcnt = 0;

                // calculate new x values
                foreach (var k in Enumerable.Range(0, A.ColumnCount))
                {
                    var Akk = A[k, k];
                    var s = A.Row(k).AsEnumerable()
                        .Select((v, n) => n == k ? 0.0 : A[k, n] * res[n, 0])
                        .Sum();

                    // current xk value
                    var cv = res[k, 0];
                    // calculate next xk value
                    var nv = 1 / Akk * (b[k, 0] - s); 

                    // calculate difference between new estimate and previous estimate
                    var err = Math.Abs(nv - cv); 
                    found = found && err <= maxErr; // calculate exit condition
                    res[k, 0] = nv; // save new x estimate

                    // check for divergence
                    if (i == 1)
                        verr[k] = err; // save first error value
                    else if (i % 5 == 0)
                    {
                        var ce = verr[k];
                        if (err > ce)
                            dvcnt++;
                        else verr[k] = err;
                    }
                }

                // check for divergence
                if (dvcnt == res.RowCount)
                    return new Result<Matrix<double>>
                    {
                        IterationStop = i,
                        Error = ErrorEnum.Divergence,
                        ErrorMessage = "Divergence detected",
                    };
            }

            if (found)
                return new Result<Matrix<double>> { IterationStop = i, Data = res };
            else
                return new Result<Matrix<double>>
                {
                    IterationStop = i,
                    Error = ErrorEnum.MaxIteration,
                    ErrorMessage = "Max iterations reached without convergence",
                };
        }

        /// <summary>
        /// Solve Ax = b with Gauss-Seidel iteration method using matrix operations.
        /// </summary>
        /// <param name="maxErr">default to 1e-4</param>
        /// <param name="maxIteration">default to 100</param>
        public static Result<Matrix<double>> GaussSeidelByMatrix(Matrix<double> A, Matrix<double> b,
            double maxErr = 1e-4, int maxIteration = 100)
        {
            var d = A.Diagonal().AsArray();
            if (d.Any(x => x == 0))
                return new Result<Matrix<double>>
                {
                    Error = ErrorEnum.ZeroDiagEntry,
                    ErrorMessage = "Zero(s) in diagonal entries",
                };

            var D = A.LowerTriangle();
            var Dinv = D.Inverse();
            var M = Dinv * (D - A);

            // x result, initial guess is zero
            var res = CreateMatrix.Dense(b.RowCount, b.ColumnCount, 0.0);
            // track error for each variable
            var verr = CreateMatrix.Dense(b.RowCount, b.ColumnCount, 0.0);
            // exit condition
            var found = false;

            var i = 0; // iteration counter
            while (i++ < maxIteration)
            {
                // calculate next x value
                var resNext = M*res + Dinv*b;

                // calculate difference between new estimate and previous estimate
                var err = resNext - res;
                err.MapInplace(v => Math.Abs(v));
                if (err.ForAll(v => v <= maxErr)) {
                    found = true;
                    break;
                }

                res.SetSubMatrix(0, 0, resNext); // save new x estimate

                // check for divergence
                if (i == 1)
                    verr.SetSubMatrix(0, 0, err); // save first error value
                else if (i % 5 == 0)
                {
                    var ecnt = err.Column(0).AsEnumerable().Zip(
                        verr.Column(0).AsEnumerable(),
                        (ne, ce) => ne < ce ? 0 : 1
                    ).Sum();

                    if (ecnt == 0)
                        verr.SetSubMatrix(0, 0, err);
                    else
                        return new Result<Matrix<double>>
                        {
                            IterationStop = i,
                            Error = ErrorEnum.Divergence,
                            ErrorMessage = "Divergence detected",
                        };

                }
            }

            if (found)
                return new Result<Matrix<double>> { IterationStop = i, Data = res };
            else
                return new Result<Matrix<double>>
                {
                    IterationStop = i,
                    Error = ErrorEnum.MaxIteration,
                    ErrorMessage = "Max iterations reached without convergence",
                };
        }

        /// <summary>
        /// Solve Ax = b with Newton-Raphson iteration method
        /// </summary>
        /// <param name="J">Function to calculate Jacobian matrix given x</param>
        /// <param name="maxErr">default to 1e-4</param>
        /// <param name="maxIteration">default to 100</param>
        public static Result<Matrix<double>> NewtonRaphson(Matrix<double> A, Matrix<double> b,
            Func<Matrix<double>, Matrix<double>> J,
            double maxErr = 1e-4, int maxIteration = 100)
        {
            // x result, initial guess is zero
            var res = CreateMatrix.Dense(b.RowCount, b.ColumnCount, 0.0);
            // track error for each variable
            var verr = CreateMatrix.Dense(b.RowCount, b.ColumnCount, 0.0);
            // exit condition
            var found = false;

            var i = 0; // iteration counter
            while (i++ < maxIteration)
            {
                // main calculation of each iteration
                var dbk = b - (A * res); // calculate delta y
                var jk = J(res); // calculate Jacobian matrix
                var dxk = jk.Inverse() * dbk; // calculate delta x
                var resNext = res + dxk; // calculate next x value

                // calculate difference between new estimate and previous estimate
                var err = resNext - res;
                err.MapInplace(v => Math.Abs(v));
                if (err.ForAll(v => v <= maxErr))
                {
                    found = true;
                    break;
                }

                // save new x estimate
                res.SetSubMatrix(0, 0, resNext); 

                // check for divergence
                if (i == 1)
                    verr.SetSubMatrix(0, 0, err); // save first error value
                else if (i % 5 == 0)
                {
                    var ecnt = err.Column(0).AsEnumerable().Zip(
                        verr.Column(0).AsEnumerable(),
                        (ne, ce) => ne < ce ? 0 : 1
                    ).Sum();

                    if (ecnt == 0)
                        verr.SetSubMatrix(0, 0, err);
                    else
                        return new Result<Matrix<double>>
                        {
                            IterationStop = i,
                            Error = ErrorEnum.Divergence,
                            ErrorMessage = "Divergence detected",
                        };

                }

            }

            if (found)
                return new Result<Matrix<double>> { IterationStop = i, Data = res };
            else
                return new Result<Matrix<double>>
                {
                    IterationStop = i,
                    Error = ErrorEnum.MaxIteration,
                    ErrorMessage = "Max iterations reached without convergence",
                };
        }
    }
}
