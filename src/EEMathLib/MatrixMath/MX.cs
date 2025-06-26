using MathNet.Numerics.LinearAlgebra;

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
    }
}
