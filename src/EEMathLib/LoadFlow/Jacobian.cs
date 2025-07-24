using MathNet.Numerics;
using System;
using System.Linq;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow
{
    public static class Jacobian
    {

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// </summary>
        public static double CalcJ1kk(BusResult bk, MC Y, BU buses)
        {
            var jk = bk.BusIndex;
            var vk = bk.BusVoltage;
            var skk = buses
                .Where(bn => bn.BusIndex != jk)
                .Select(bn => {
                    var vn = bn.BusVoltage;
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Sign(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            return -vk.Magnitude * skk;
        }

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// </summary>
        public static double CalcJ2kk(BusResult bk, MC Y, BU buses)
        {
            var vk = bk.BusVoltage;
            var ykk = Y[bk.BusData.BusIndex, bk.BusData.BusIndex];
            var skk = buses
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Cos(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            var i = vk.Magnitude * ykk.Magnitude * Math.Cos(ykk.Phase);
            return i + skk;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// </summary>
        public static double CalcJ3kk(BusResult bk, MC Y, BU buses)
        {
            var jk = bk.BusIndex;
            var vk = bk.BusVoltage;
            var skk = buses
                .Where(bn => bn.BusIndex != jk)
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Cos(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            return vk.Magnitude * skk;
        }

        /// <summary>
        /// Q/V derivative Jacobian matrix.
        /// </summary>
        public static double CalcJ4kk(BusResult bk, MC Y, BU buses)
        {
            var vk = bk.BusVoltage;
            var ykk = Y[bk.BusData.BusIndex, bk.BusData.BusIndex];
            var skk = buses
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Sign(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            var i = -vk.Magnitude * ykk.Magnitude * Math.Sign(ykk.Phase);
            return i + skk;
        }

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// </summary>
        public static MD CreateJ1(MC Y, BU buses)
        {
            var lstBus = buses;
            var N = lstBus.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in lstBus) // row
            {
                var jk = bk.BusIndex;
                var vk = bk.BusVoltage;
                foreach (var bn in lstBus) // column
                {
                    var jn = bn.BusIndex;
                    if (jk == jn)
                    {
                        var jkk = CalcJ1kk(bk, Y, lstBus);
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
        public static MD CreateJ2(MC Y, BU buses)
        {
            var lstBus = buses;
            var N = lstBus.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in lstBus) // row
            {
                var jk = bk.BusIndex;
                var vk = bk.BusVoltage;
                foreach (var bn in lstBus) // column
                {
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.BusIndex;
                    if (jk == jn)
                    {
                        var jkk = CalcJ2kk(bk, Y, lstBus);
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
        public static MD CreateJ3(MC Y, BU buses)
        {
            var lstBus = buses;
            var N = lstBus.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in lstBus) // row
            {
                var jk = bk.BusIndex;
                var vk = bk.BusVoltage;
                foreach (var bn in lstBus) // column
                {
                    var jn = bn.BusIndex;
                    var vn = bn.BusVoltage;
                    if (jk == jn)
                    {
                        var jkk = CalcJ3kk(bk, Y, lstBus);
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
        public static MD CreateJ4(MC Y, BU buses)
        {
            var lstBus = buses; //.Where(b => b.BusIndex > -1).ToList();
            var N = lstBus.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in lstBus) // row
            {
                var jk = bk.BusIndex;
                var vk = bk.BusVoltage;
                foreach (var bn in lstBus) // column
                {
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.BusIndex;
                    if (jk == jn)
                    {
                        var jkk = CalcJ4kk(bk, Y, lstBus);
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

        public static MD CreateJMatrix(MC Y, BU buses)
        {
            var J1 = CreateJ1(Y, buses);
            var J2 = CreateJ2(Y, buses);
            var J3 = CreateJ3(Y, buses);
            var J4 = CreateJ4(Y, buses);

            var J = MD.Build.Dense(2 * J1.RowCount, 2 * J1.ColumnCount);
            J.SetSubMatrix(0, 0, J1);
            J.SetSubMatrix(0, J1.ColumnCount, J2);
            J.SetSubMatrix(J1.RowCount, 0, J3);
            J.SetSubMatrix(J1.RowCount, J1.ColumnCount, J4);

            return J;
        }


    }
}
