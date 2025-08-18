using System;
using static EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    public static class JacobianFD
    {

        #region J1

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// Diagonal entries
        /// </summary>
        public static double CalcJ1kk(BusResult bk, MC Y)
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
        public static double CalcJ1kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var vn = bn.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            // basically just -B of Y (G + jB)
            // assuming all V is approxmiately 1.0
            var jkn = -vk.Magnitude * vn.Magnitude * ykn.Imaginary;
            return jkn;
        }

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// Off-diagonal entries
        /// </summary>
        public static MD CreateJ1(MC Y, NRBuses nrBuses)
        {
            var J = MD.Build.Dense(nrBuses.J1Size.Row, nrBuses.J1Size.Col);
            foreach (var bk in nrBuses.Buses) // row
            {
                var jk = bk.Pidx;
                var vk = bk.BusVoltage;
                var bkIdx = bk.BusData.BusIndex;
                foreach (var bn in nrBuses.Buses) // column
                {
                    var jn = bn.Aidx;
                    if (bkIdx == bn.BusData.BusIndex)
                    {
                        var jkk = CalcJ1kk(bk, Y);
                        J[jk, jn] = jkk;
                    }
                    else
                    {
                        var jkn = CalcJ1kn(bk, bn, Y);
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        #endregion

        #region J4

        /// <summary>
        /// Q/V derivative Jacobian matrix.
        /// Diagonal entries.
        /// </summary>
        public static double CalcJ4kk(BusResult bk, MC Y)
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
        public static double CalcJ4kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var jkn = -vk.Magnitude * ykn.Imaginary;
            return jkn;
        }

        /// <summary>
        /// Q/V derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public static MD CreateJ4(MC Y, NRBuses nrBuses)
        {
            var J = MD.Build.Dense(nrBuses.J4Size.Row, nrBuses.J4Size.Col);
            foreach (var bk in nrBuses.PQBuses) // row
            {
                var jk = bk.Qidx;
                var vk = bk.BusVoltage;
                var bkIdx = bk.BusData.BusIndex;
                foreach (var bn in nrBuses.PQBuses) // column
                {
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.Vidx;
                    if (bkIdx == bn.BusData.BusIndex)
                    {
                        var jkk = CalcJ4kk(bk, Y);
                        J[jk, jn] = jkk;
                    }
                    else
                    {
                        var jkn = CalcJ4kn(bk, bn, Y);
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        #endregion

    }
}
