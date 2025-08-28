using System;
using System.Linq;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.V2
{
    /// <summary>
    /// Algorithm to calculate Jacobian matrix for Newton-Raphson load flow.
    /// The formula are based on reference of another textbook.
    /// </summary>
    public class JacobianV2 : JacobianBase
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
            var skn = nrBuses.AllBuses
                .Where(bn => bn.BusData.BusIndex != jk)
                .Select(bn => {
                    var vn = bn.BusVoltage;
                    var ykn = Y[jk, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Sign(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            return -vk.Magnitude * skn;
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
            var a = vk.Phase - ykn.Phase - vn.Phase;
            var jkn = vk.Magnitude * ykn.Magnitude * vn.Magnitude * Math.Sign(a);
            return jkn;
        }

        #endregion

        #region J2

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// Diagonal entries
        /// </summary>
        public override double CalcJ2kk(BusResult bk, MC Y, NRBuses nRBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
            var skk = nRBuses.AllBuses
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[jk, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Cos(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            var i = vk.Magnitude * ykk.Real * Math.Cos(ykk.Phase);
            return i + skk;
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
            var a = vk.Phase - ykn.Phase - vn.Phase;
            var jkn = vk.Magnitude * ykn.Magnitude * Math.Cos(a);
            return jkn;
        }

        #endregion

        #region J3

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Diagonal entries.
        /// </summary>
        public override double CalcJ3kk(BusResult bk, MC Y, NRBuses nRBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var skk = nRBuses.AllBuses
                .Where(bn => bn.BusData.BusIndex != jk)
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[jk, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Cos(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            return vk.Magnitude * skk;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public override double CalcJ3kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var vn = bn.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var a = vk.Phase - ykn.Phase - vn.Phase;
            var jkn = -vk.Magnitude * ykn.Magnitude * vn.Magnitude * Math.Cos(a);
            return jkn;
        }

        #endregion

        #region J4

        /// <summary>
        /// Q/V derivative Jacobian matrix.
        /// Diagonal entries.
        /// </summary>
        public override double CalcJ4kk(BusResult bk, MC Y, NRBuses nRBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
            var skk = nRBuses.AllBuses
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[jk, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Sign(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            var i = -vk.Magnitude * ykk.Imaginary * Math.Sin(ykk.Phase);
            return i + skk;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public override double CalcJ4kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var vn = bn.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var a = vk.Phase - ykn.Phase - vn.Phase;
            var jkn = vk.Magnitude * ykn.Magnitude * Math.Sign(a);
            return jkn;
        }

        #endregion

    }
}
