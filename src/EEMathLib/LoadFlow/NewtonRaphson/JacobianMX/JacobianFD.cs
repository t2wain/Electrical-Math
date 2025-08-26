using System;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.NewtonRaphson.JacobianMX
{
    public class JacobianFD : JacobianBase
    {

        #region J1

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// Diagonal entries
        /// </summary>
        public override double CalcJ1kk(BusResult bk, MC Y, NRBuses nrBuses = null)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            // basically just -B of Y (G + jB)
            // assuming all V is approxmiately 1.0
            var sk = -Y[jk, jk].Imaginary * Math.Pow(vk.Magnitude, 2);
            return sk;
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
            // basically just -B of Y (G + jB)
            // assuming all V is approxmiately 1.0
            var jkn = -vk.Magnitude * vn.Magnitude * ykn.Imaginary;
            return jkn;
        }

        #endregion

        #region J4

        /// <summary>
        /// Q/V derivative Jacobian matrix.
        /// Diagonal entries.
        /// </summary>
        public override double CalcJ4kk(BusResult bk, MC Y, NRBuses nrBuses = null)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
            var skk = -vk.Magnitude * ykk.Imaginary;
            return skk;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public override double CalcJ4kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var jkn = -vk.Magnitude * ykn.Imaginary;
            return jkn;
        }

        #endregion

    }
}
