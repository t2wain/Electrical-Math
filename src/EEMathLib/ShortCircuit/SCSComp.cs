using EEMathLib.MatrixMath;
using EEMathLib.ShortCircuit.Data;
using MathNet.Numerics;
using System;
using System.Numerics;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit
{
    public static class SCSComp
    {
        // {a} operator where magnitude = 1.0 and phase = 120deg
        static readonly Complex a = Complex.FromPolarCoordinates(1, 2 * Math.PI / 3);
        // {a2} operator where magnitude = 1.0 and phase = 240deg
        static readonly Complex a2 = a.Power(2);
        // transform matrix of 3x3
        static readonly MC MXA;
        // transform matrix of 3x3
        static readonly MC MXAINV;

        static SCSComp()
        {
            MXA = MX.BuildMX(3, 3,
                    1, 1, 1,
                    1, a2, a,
                    1, a, a2
                );

            var mx2 = MX.BuildMX(3, 3, 
                    1, 1, 1,
                    1, a, a2,
                    1, a2, a
                );
            mx2.MapInplace(v => v / 3);
            MXAINV = mx2;
        }

        /// <summary>
        /// Calculate the symmetrical components of an asymmetrical value.
        /// </summary>
        public static ISymComp CalcSymComp(IAsymPhasor asymPhasor)
        {
            var mxAsymV = PhaseValue.ToMatrix(asymPhasor);
            var mxSymV = MXAINV * mxAsymV; // use transform matrix
            return PhaseValue.FromMatrix(mxSymV);
        }

        /// <summary>
        /// Calculate the asymmetrical values from the asymmetrical components.
        /// </summary>
        public static IAsymPhasor CalcAsymPhasor(ISymComp symComp)
        {
            var mxV = MXA * PhaseValue.ToMatrix(symComp); // use transform matrix
            return PhaseValue.FromMatrix(mxV);
        }

        /// <summary>
        /// Calculate three-phase apparent power of an asymmetrical system. 
        /// </summary>
        /// <param name="voltages">Asymmetrical voltages</param>
        /// <param name="current">Asymmetrical currents</param>
        /// <returns>Apparent three-phase power</returns>
        public static Complex CalcAsymPower(IAsymPhasor voltages, IAsymPhasor current)
        {
            // calculate symmetrical components of voltage
            var symV = CalcSymComp(voltages);
            var mxV = PhaseValue.ToMatrix(symV).Transpose();

            // calculate symmetrical components of current
            var symI = CalcSymComp(current);
            var mxI = PhaseValue.ToMatrix(symI);

            // calculate current conjugate
            mxI.MapInplace(v => v.Conjugate());
            
            // calculate power S = V * I.conjugate()
            var s = 3 * mxV * mxI;
            return s[0, 0];
        }
    }
}
