using MathNet.Numerics;
using System;
using System.Linq;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow.NR
{
    public static class Jacobian
    {
        #region Jkk

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// </summary>
        public static double CalcJ1kk(BusResult bk, MC Y, BU buses)
        {
            var jk = bk.Pidx;
            var vk = bk.BusVoltage;
            var skk = buses
                .Where(bn => bn.Aidx != jk)
                .Select(bn => {
                    var vn = bn.BusVoltage;
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = vk.Magnitude * ykn.Magnitude * vn.Magnitude * Math.Sign(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            return -skk;
        }

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// </summary>
        public static double CalcJ2kk(BusResult bk, MC Y, BU pqBuses)
        {
            var jk = bk.Pidx;
            var vk = bk.BusVoltage;
            var ykk = Y[bk.BusData.BusIndex, bk.BusData.BusIndex];
            var skk = pqBuses
                .Where(bn => bn.Vidx != jk)
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Cos(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            var i = 2 * vk.Magnitude * ykk.Real;
            return i + skk;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// </summary>
        public static double CalcJ3kk(BusResult bk, MC Y, BU buses)
        {
            var jk = bk.Qidx;
            var vk = bk.BusVoltage;
            var skk = buses
                .Where(bn => bn.Aidx != jk)
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = vk.Magnitude * ykn.Magnitude * vn.Magnitude * Math.Cos(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            return skk;
        }

        /// <summary>
        /// Q/V derivative Jacobian matrix.
        /// </summary>
        public static double CalcJ4kk(BusResult bk, MC Y, BU pqBuses)
        {
            var jk = bk.Qidx;
            var vk = bk.BusVoltage;
            var ykk = Y[bk.BusData.BusIndex, bk.BusData.BusIndex];
            var skk = pqBuses
                .Where(bn => bn.Vidx != jk)
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Sign(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            var i = -2 * vk.Magnitude * ykk.Imaginary;
            return i + skk;
        }

        #endregion

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// </summary>
        public static MD CreateJ1(MC Y, NRBuses nrBuses)
        {
            var J = MD.Build.Dense(nrBuses.J1Size.Row, nrBuses.J1Size.Col);
            foreach (var bk in nrBuses.Buses) // row
            {
                var jk = bk.Pidx;
                var vk = bk.BusVoltage;
                foreach (var bn in nrBuses.Buses) // column
                {
                    var jn = bn.Aidx;
                    if (jk == jn)
                    {
                        var jkk = CalcJ1kk(bk, Y, nrBuses.Buses);
                        J[jk, jk] = jkk;
                    }
                    else
                    {
                        var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                        var jkn = (vk * ykn.Conjugate() * bn.BusVoltage.Conjugate()).Imaginary;
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// </summary>
        public static MD CreateJ2(MC Y, NRBuses nrBuses)
        {
            var J = MD.Build.Dense(nrBuses.J2Size.Row, nrBuses.J2Size.Col);
            foreach (var bk in nrBuses.Buses) // row
            {
                var jk = bk.Pidx;
                var vk = bk.BusVoltage;
                foreach (var bn in nrBuses.PQBuses) // column
                {
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.Vidx;
                    if (jk == jn)
                    {
                        var jkk = CalcJ2kk(bk, Y, nrBuses.PQBuses);
                        J[jk, jk] = jkk;
                    }
                    else
                    {
                        var a = vk.Phase - ykn.Phase - bn.BusVoltage.Phase;
                        var jkn = vk.Magnitude * ykn.Magnitude * Math.Cos(a);
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// </summary>
        public static MD CreateJ3(MC Y, NRBuses nrBuses)
        {
            var J = MD.Build.Dense(nrBuses.J3Size.Row, nrBuses.J3Size.Col);

            foreach (var bk in nrBuses.PQBuses) // row
            {
                var jk = bk.Qidx;
                var vk = bk.BusVoltage;
                foreach (var bn in nrBuses.Buses) // column
                {
                    var jn = bn.Aidx;
                    var vn = bn.BusVoltage;
                    if (jk == jn)
                    {
                        var jkk = CalcJ3kk(bk, Y, nrBuses.Buses);
                        J[jk, jk] = jkk;
                    }
                    else
                    {
                        var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                        var jkn = (-vk * ykn.Conjugate() * bn.BusVoltage.Conjugate()).Real;
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        /// <summary>
        /// Q/V derivative Jacobian matrix.
        /// </summary>
        public static MD CreateJ4(MC Y, NRBuses nrBuses)
        {
            var J = MD.Build.Dense(nrBuses.J4Size.Row, nrBuses.J4Size.Col);
            foreach (var bk in nrBuses.PQBuses) // row
            {
                var jk = bk.Qidx;
                var vk = bk.BusVoltage;
                foreach (var bn in nrBuses.PQBuses) // column
                {
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.Vidx;
                    if (jk == jn)
                    {
                        var jkk = CalcJ4kk(bk, Y, nrBuses.PQBuses);
                        J[jk, jk] = jkk;
                    }
                    else
                    {
                        var a = vk.Phase - ykn.Phase - bn.BusVoltage.Phase;
                        var jkn = vk.Magnitude * ykn.Magnitude * Math.Sign(a);
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        public static MD CreateJMatrix(MC Y, NRBuses nrBuses)
        {
            var J1 = CreateJ1(Y, nrBuses);
            var J2 = CreateJ2(Y, nrBuses);
            var J3 = CreateJ3(Y, nrBuses);
            var J4 = CreateJ4(Y, nrBuses);

            var J = MD.Build.Dense(nrBuses.JSize.Row, nrBuses.JSize.Col);
            J.SetSubMatrix(0, 0, J1);
            J.SetSubMatrix(0, J1.ColumnCount, J2);
            J.SetSubMatrix(J1.RowCount, 0, J3);
            J.SetSubMatrix(J1.RowCount, J1.ColumnCount, J4);

            return J;
        }
    }
}
