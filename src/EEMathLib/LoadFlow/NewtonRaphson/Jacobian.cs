using EEMathLib.LoadFlow.Data;
using System;
using System.Linq;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Algorithm to calculate Jacobian matrix for Newton-Raphson load flow
    /// </summary>
    public static class Jacobian
    {
        #region NRBuses

        /// <summary>
        /// The type of each bus (PQ or PV) for
        /// each iteration of NewtonRaphson
        /// </summary>
        public class NRBuses
        {
            /// <summary>
            /// All buses from input data including slack bus
            /// </summary>
            public BU AllBuses { get; set; }

            /// <summary>
            /// All PV buses based on input data
            /// </summary>
            public BU AllPVBuses { get; set; }

            /// <summary>
            /// Designated slack bus from input data
            /// </summary>
            public BusResult SlackBus { get; set; }

            /// <summary>
            /// PQ buses in the current iteration. 
            /// Calculate V and A, given P and Q
            /// </summary>
            public BU PQBuses { get; set; }

            /// <summary>
            /// PV buses in the current iteration. 
            /// Calculate Q and A, given P and V
            /// </summary>
            public BU PVBuses { get; set; }

            /// <summary>
            /// Include all buses except for slack bus.
            /// </summary>
            public BU Buses { get; set; }

            /// <summary>
            /// Matrix dimension of J1
            /// </summary>
            public (int Row, int Col) J1Size { get; set; }

            /// <summary>
            /// Matrix dimension of J2
            /// </summary>
            public (int Row, int Col) J2Size { get; set; }

            /// <summary>
            /// Matrix dimension of J3
            /// </summary>
            public (int Row, int Col) J3Size { get; set; }

            /// <summary>
            /// Matrix dimension of J4
            /// </summary>
            public (int Row, int Col) J4Size { get; set; }

            /// <summary>
            /// Matrix dimension of Jacobian matrix
            /// </summary>
            public (int Row, int Col) JSize { get; set; }
        }

        /// <summary>
        /// Assign each bus with an entry index for the 
        /// Jacobian matrix J1, J2, J3, and J4
        /// </summary>
        public static NRBuses ReIndexBusPQ(BU buses)
        {
            var lstBuses = buses
                .Where(b => b.BusType != BusTypeEnum.Slack) // exclude slack bus
                .OrderBy(b => b.BusType == BusTypeEnum.PV ? 0 : 1) // first sorted by PV then PQ
                .ThenBy(b => b.BusData.BusIndex) // then sorted by the original indices
                .ToList();

            var aidx = 0;
            var vidx = 0;
            foreach (var b in lstBuses)
            {
                // reset all indices
                b.Aidx = b.Vidx = b.Qidx = b.Pidx = -1;
                b.Aidx = b.Pidx = aidx++; // assign indices
                if (b.BusType == BusTypeEnum.PQ)
                {
                    b.Vidx = b.Qidx = vidx++; // assign indices
                }
            }

            var pqBuses = lstBuses
                .Where(b => b.BusType == BusTypeEnum.PQ)
                .ToList();
            var busCnt = lstBuses.Count;
            var pqCnt = pqBuses.Count;

            var ctx = new NRBuses
            {
                AllBuses = buses.ToList(),
                AllPVBuses = lstBuses.Where(b => b.BusData.BusType == BusTypeEnum.PV).ToList(),
                SlackBus = buses.Where(b => b.BusType == BusTypeEnum.Slack).First(),
                PQBuses = pqBuses, // calc V and A, given P and Q
                PVBuses = lstBuses
                    .Where(b => b.BusType == BusTypeEnum.PV)
                    .ToList(), // calc Q and A, given P and V
                Buses = lstBuses,
                J1Size = (busCnt, busCnt),
                J2Size = (busCnt, pqCnt),
                J3Size = (pqCnt, busCnt),
                J4Size = (pqCnt, pqCnt),
                JSize = (busCnt + pqCnt, busCnt + pqCnt)
            };

            return ctx;
        }

        #endregion

        #region J1

        /// <summary>
        /// P/A derivative Jacobian matrix.
        /// Diagonal entries
        /// </summary>
        public static double CalcJ1kk(BusResult bk, MC Y, NRBuses nrBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var skn = nrBuses.AllBuses
                .Where(bn => bn.BusData.BusIndex != jk)
                .Select(bn => {
                    var vn = bn.BusVoltage;
                    var ykn = Y[jk, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = vk.Magnitude * ykn.Magnitude * vn.Magnitude * Math.Sign(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            return -skn;
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
            var a = vk.Phase - ykn.Phase - vn.Phase;
            var jkn = vk.Magnitude * ykn.Magnitude * vn.Magnitude * Math.Sign(a);
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
                        var jkk = CalcJ1kk(bk, Y, nrBuses);
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

        #region J2

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// Diagonal entries
        /// </summary>
        public static double CalcJ2kk(BusResult bk, MC Y, NRBuses nRBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
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
            var i = 2 * vk.Magnitude * ykk.Real;
            return i + skk;
        }

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// Off-diagonal entries
        /// </summary>
        public static double CalcJ2kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var vn = bk.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var a = vk.Phase - ykn.Phase - vn.Phase;
            var jkn = vk.Magnitude * ykn.Magnitude * Math.Cos(a);
            return jkn;
        }

        /// <summary>
        /// P/V derivative Jacobian matrix.
        /// Off-diagonal entries
        /// </summary>
        public static MD CreateJ2(MC Y, NRBuses nrBuses)
        {
            var J = MD.Build.Dense(nrBuses.J2Size.Row, nrBuses.J2Size.Col);
            foreach (var bk in nrBuses.Buses) // row
            {
                var jk = bk.Pidx;
                var vk = bk.BusVoltage;
                var bkIdx = bk.BusData.BusIndex;
                foreach (var bn in nrBuses.PQBuses) // column
                {
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.Vidx;
                    if (bkIdx == bn.BusData.BusIndex)
                    {
                        var jkk = CalcJ2kk(bk, Y, nrBuses);
                        J[jk, jn] = jkk;
                    }
                    else
                    {
                        var jkn = CalcJ2kn(bk, bn, Y);
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        #endregion

        #region J3

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Diagonal entries.
        /// </summary>
        public static double CalcJ3kk(BusResult bk, MC Y, NRBuses nRBuses)
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
                    var s = vk.Magnitude * ykn.Magnitude * vn.Magnitude * Math.Cos(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            return skk;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public static double CalcJ3kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var vn = bn.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var a = vk.Phase - ykn.Phase - vn.Phase;
            var jkn = -vk.Magnitude * ykn.Magnitude * vn.Magnitude * Math.Cos(a);
            return jkn;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public static MD CreateJ3(MC Y, NRBuses nrBuses)
        {
            var J = MD.Build.Dense(nrBuses.J3Size.Row, nrBuses.J3Size.Col);

            foreach (var bk in nrBuses.PQBuses) // row
            {
                var jk = bk.Qidx;
                var vk = bk.BusVoltage;
                var bkIdx = bk.BusData.BusIndex;
                foreach (var bn in nrBuses.Buses) // column
                {
                    var jn = bn.Aidx;
                    var vn = bn.BusVoltage;
                    if (bkIdx == bn.BusData.BusIndex)
                    {
                        var jkk = CalcJ3kk(bk, Y, nrBuses);
                        J[jk, jn] = jkk;
                    }
                    else
                    {
                        var jkn = CalcJ3kn(bk, bn, Y);
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
        public static double CalcJ4kk(BusResult bk, MC Y, NRBuses nRBuses)
        {
            var jk = bk.BusData.BusIndex;
            var vk = bk.BusVoltage;
            var ykk = Y[jk, jk];
            var skk = nRBuses.AllBuses
                .Where(bn => bn.BusData.BusIndex != jk)
                .Select(bn =>
                {
                    var vn = bn.BusVoltage;
                    var ykn = Y[jk, bn.BusData.BusIndex];
                    var a = vk.Phase - ykn.Phase - vn.Phase;
                    var s = ykn.Magnitude * vn.Magnitude * Math.Sign(a);
                    return s;
                })
                .Aggregate((a, b) => a + b);
            var i = -2 * vk.Magnitude * ykk.Imaginary;
            return i + skk;
        }

        /// <summary>
        /// Q/A derivative Jacobian matrix.
        /// Off-diagonal entries.
        /// </summary>
        public static double CalcJ4kn(BusResult bk, BusResult bn, MC Y)
        {
            var vk = bk.BusVoltage;
            var vn = bn.BusVoltage;
            var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
            var a = vk.Phase - ykn.Phase - vn.Phase;
            var jkn = vk.Magnitude * ykn.Magnitude * Math.Sign(a);
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
                        var jkk = CalcJ4kk(bk, Y, nrBuses);
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

        /// <summary>
        /// Calculate Jacobian matrix
        /// </summary>
        public static MD CreateJMatrix(MC Y, NRBuses nrBuses)
        {
            var J1 = CreateJ1(Y, nrBuses);
            var J2 = CreateJ2(Y, nrBuses);
            var J3 = CreateJ3(Y, nrBuses);
            var J4 = CreateJ4(Y, nrBuses);
            return CreateJMatrix(J1, J2, J3, J4);
        }

        public static MD CreateJMatrix(MD J1, MD J2, MD J3, MD J4)
        {
            //var J = MD.Build.Dense(nrBuses.JSize.Row, nrBuses.JSize.Col);
            var J = MD.Build.Dense(J1.RowCount + J3.RowCount, J1.ColumnCount + J2.ColumnCount);
            J.SetSubMatrix(0, 0, J1);
            J.SetSubMatrix(0, J1.ColumnCount, J2);
            J.SetSubMatrix(J1.RowCount, 0, J3);
            J.SetSubMatrix(J1.RowCount, J1.ColumnCount, J4);

            return J;
        }
    }
}
