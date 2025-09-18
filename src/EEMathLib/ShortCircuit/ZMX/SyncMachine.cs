using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using EEMathLib.ShortCircuit.Data;
using System.Numerics;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit.ZMX
{
    public interface ISyncMachine
    {
        string BusID { get; }
        WindingEnum Winding { get; }
        Complex Es { get; }
        Complex Vt { get; }
        double Xs { get; }
        double Rs { get; }
        double Xm { get; }
        Complex Zn { get; }
    }

    internal static class SyncMachineExtensions
    {
        public static Complex Zs(this IEZGen m) => new Complex(m.Rs, m.Xs) + m.Zn;
        public static Complex Zm(this IEZGen m) => new Complex(0, m.Xm) + m.Zn;

        /// <summary>
        /// Total impedance in zero network
        /// </summary>
        public static Complex Z00(this IEZGen m) => m.Zs() + 2 * m.Zm();

        /// <summary>
        /// zero-sequence Z
        /// </summary>
        public static Complex Z0(this IEZGen m) => m.Z00() - 3 * m.Zn;

        /// <summary>
        /// Positive-sequence Z
        /// </summary>
        public static Complex Z1(this IEZGen m) => m.Zs() - m.Zm();

        /// <summary>
        /// Negative-sequence Z
        /// </summary>
        public static Complex Z2(this IEZGen m) => m.Z1();

        public static MC CalcSymZMatrix(this IEZGen m) =>
            MX.BuildMX(3, 3,
                m.Z00(), 0, 0,
                0, m.Z1(), 0,
                0, 0, m.Z2()
            );

        public static ISymComp CalcSymVoltage(this IEZGen m, ISymComp current)
        {
            var mxE = MX.BuildMX(3, 1, 0, MX.C(m.Bus.Voltage, 0), 0);
            var mxI = PhaseValue.ToMatrix(current);
            var zsym = m.CalcSymZMatrix();
            var mxVt = mxE - zsym * mxI;
            return PhaseValue.FromMatrix(mxVt);
        }
    }
}
