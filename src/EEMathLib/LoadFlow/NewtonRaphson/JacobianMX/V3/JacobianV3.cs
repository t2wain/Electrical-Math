using System;
using LFC = EEMathLib.LoadFlow.LFCommon;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.V3
{
    /// <summary>
    /// Algorithm to calculate Jacobian matrix for Newton-Raphson load flow.
    /// The formula are based on reference of another textbook.
    /// </summary>
    public class JacobianV3 : JacobianBase
    {
        #region J1

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// Diagonal entries
        /// </summary>
        public override double CalcJ1kk(BusResult bk, MC Y, NRBuses nrBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
            var Q = LFC.CalcBusPower(bk, Y, nrBuses.AllBuses).Imaginary;
            return -Q - ykk.Imaginary * Math.Pow(vk.Magnitude, 2) ;
        }

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// Off-diagonal entries
        /// </summary>
        public override double CalcJ1kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var vn = bn.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var a = vk.Phase - vn.Phase;
            var jkn = vk.Magnitude * vn.Magnitude 
                * (ykn.Real * Math.Sin(a) - ykn.Imaginary * Math.Cos(a));
            return jkn;
        }

        #endregion

        #region J2

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// Diagonal entries
        /// </summary>
        public override double CalcJ2kk(BusResult bk, MC Y, NRBuses nrBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
            var P = LFC.CalcBusPower(bk, Y, nrBuses.AllBuses).Real;
            return P + ykk.Real * Math.Pow(vk.Magnitude, 2);
        }

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// Off-diagonal entries
        /// </summary>
        public override double CalcJ2kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var vn = bk.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var a = vk.Phase - vn.Phase;
            var jkn = vk.Magnitude * ykn.Magnitude 
                * (ykn.Real * Math.Cos(a) + ykn.Imaginary * Math.Sin(a));
            return jkn;
        }

        #endregion

        #region J3

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Diagonal entries.
        /// </summary>
        public override double CalcJ3kk(BusResult bk, MC Y, NRBuses nrBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
            var P = LFC.CalcBusPower(bk, Y, nrBuses.AllBuses).Real;
            return P - ykk.Real * Math.Pow(vk.Magnitude, 2);
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public override double CalcJ3kn(BusResult bk, BusResult bn, MC Y) => 
            CalcJ2kn(bk, bn, Y);

        #endregion

        #region J4

        /// <summary>
        /// Q/V derivative Jacobian matrix.
        /// Diagonal entries.
        /// </summary>
        public override double CalcJ4kk(BusResult bk, MC Y, NRBuses nrBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
            var Q = LFC.CalcBusPower(bk, Y, nrBuses.AllBuses).Imaginary;
            return Q - ykk.Imaginary * Math.Pow(vk.Magnitude, 2);
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public override double CalcJ4kn(BusResult bk, BusResult bn, MC Y) => 
            CalcJ1kn(bk, bn, Y);

        #endregion

    }
}
