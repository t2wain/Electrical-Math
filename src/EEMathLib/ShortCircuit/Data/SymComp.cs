using EEMathLib.MatrixMath;
using System;
using System.Numerics;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit.Data
{
    /// <summary>
    /// Represents a set of values for three phase system.
    /// </summary>
    public interface IPhaseValue
    {
        Complex P1 { get; }
        Complex P2 { get; }
        Complex P3 { get; }
    }

    /// <summary>
    /// Represents a symmetrical component of zero (A0), positive (A1), 
    /// and negative (A2) sequence. Phase A is used as a reference phase
    /// and other phases (B and C) can be derived from phase A.
    /// </summary>
    public interface ISymComp : IPhaseValue
    {
        /// <summary>
        /// Phase A of zero sequence
        /// </summary>
        Complex A0 { get; }
        /// <summary>
        /// Phase A of positive sequence
        /// </summary>
        Complex A1 { get; }
        /// <summary>
        /// Phase A of negative sequence
        /// </summary>
        Complex A2 { get; }
    }

    /// <summary>
    /// Represents a asymmetrical values of a three phase system. 
    /// </summary>
    public interface IAsymPhasor : IPhaseValue
    {
        Complex A { get; }
        Complex B { get; }
        Complex C { get; }
    }

    /// <summary>
    /// Represents a set of values for three phase system.
    /// </summary>
    public class PhaseValue : IPhaseValue, ISymComp, IAsymPhasor
    {
        public Complex P1 { get; set; }
        public Complex P2 { get; set; }
        public Complex P3 { get; set; }

        #region Utility methods

        /// <summary>
        /// Convert to a column matrix of dimension 3x1
        /// </summary>
        public static MC ToMatrix(IPhaseValue phValue) => 
            MX.BuildMX(3, 1, phValue.P1, phValue.P2, phValue.P3);

        /// <summary>
        /// Convert to a set of values of three phase system
        /// </summary>
        /// <param name="mxValue">A column matrix of dimension 3x1</param>
        public static PhaseValue FromMatrix(MC mxValue)
        {
            if (mxValue.RowCount == 3 && mxValue.ColumnCount == 1)
            {
                return new PhaseValue
                {
                    P1 = mxValue[0, 0],
                    P2 = mxValue[1, 0],
                    P3 = mxValue[2, 0],
                };
            }
            else throw new Exception("Expect column matrix of 3x1");
        }

        #endregion

        #region ISymComp interface

        Complex ISymComp.A0 => P1;
        Complex ISymComp.A1 => P2;
        Complex ISymComp.A2 => P3;

        #endregion

        #region IAsymPhasor interface

        Complex IAsymPhasor.A => P1;
        Complex IAsymPhasor.B => P2;
        Complex IAsymPhasor.C => P3;

        #endregion
    }
}
