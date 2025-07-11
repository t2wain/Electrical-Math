using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Linq;

namespace EEMathLib.MatrixMath
{
    public abstract class MX
    {
        public static MatrixBuilder<double> MXBuilder => Matrix<double>.Build;

        public static Matrix<double> BuildMX() => Matrix<double>.Build.Dense(3, 3, 0);

        public static Matrix<double> BuildMX2() => CreateMatrix.Dense<double>(3, 3);

        public static VectorBuilder<double> VCBuilder => Vector<double>.Build;

        public static Vector<double> BuildVec() => VCBuilder.Dense(3, 0);

        public static Vector<double> BuildVec2() => CreateVector.Dense<double>(3);

        public static Vector<double> Solve(Matrix<double> A, Vector<double> b) => A.Solve(b);

        public static Matrix<double> ParseMatrix(MxDTO m) =>
            m.EntriesType == MxDTO.COLUMN_ENTRIES ?
                Matrix<double>.Build.Dense(m.RowSize, m.ColumnSize, m.Entries) :
                Matrix<double>.Build.DenseOfRowMajor(m.RowSize, m.ColumnSize, m.Entries);

        public static Matrix<double> GaussSeidel(Matrix<double> A, Matrix<double> b, double maxErr, int maxIteration)
        {
            var res = CreateMatrix.Dense(b.RowCount, b.ColumnCount, 0.0);
            var found = false;

            var icnt = 0;
            do
            {
                found = true;
                foreach (var k in Enumerable.Range(0, A.ColumnCount))
                {
                    var Akk = A[k, k];
                    var s = A.Row(k).AsEnumerable()
                        .Select((v, n) => n == k ? 0.0 : A[k, n] * res[n, 0])
                        .Sum();
                    var nvt = 1 / Akk * (b[k, 0] - s);
                    var err = Math.Abs(nvt - res[k, 0]);
                    found = found && err <= maxErr;
                    res[k, 0] = nvt;
                }
            }
            while (!found && icnt++ < maxIteration);
            return res;
        }
    }
}
