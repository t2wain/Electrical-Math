using EEMathLib.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow
{
    public static class LFNewtonRaphson
    {
        #region Jacobian Matrix

        public static Complex CalcJ13kk(BusResult bk, MC Y, BU buses)
        {
            var jk = bk.BusIndex;
            var skk = buses
                .Where(bn => bn.BusIndex != jk)
                .Select(bn => Y[bk.BusData.BusIndex, bn.BusData.BusIndex] * bn.BusVoltage)
                .Aggregate((a, b) => a + b);
            return skk;
        }

        public static Complex CalcJ24kk(BusResult bk, MC Y, BU buses)
        {
            var skk = buses
                .Select(bn => Y[bn.BusData.BusIndex, bn.BusData.BusIndex] * bn.BusVoltage)
                .Aggregate((a, b) => a + b);
            return skk;
        }

        public static MD CreateJ1(MC Y, BU buses)
        {
            var N = buses.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in buses) // row
            {
                var jk = bk.BusIndex;
                var vk = bk.BusVoltage;
                foreach (var bn in buses) // column
                {
                    var jn = bn.BusIndex;
                    if (jk == jn)
                    {
                        var ykk = (-vk * CalcJ13kk(bk, Y, buses)).Imaginary;
                        J[jk, jk] = ykk;
                    }
                    else
                    {
                        var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                        var jkn = (vk * ykn * bn.BusVoltage).Imaginary;
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        public static MD CreateJ2(MC Y, BU buses)
        {
            var N = buses.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in buses) // row
            {
                var jk = bk.BusIndex;
                var vk = bk.BusVoltage;
                foreach (var bn in buses) // column
                {
                    var ykk = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.BusIndex;
                    if (jk == jn)
                    {
                        var jkk = (vk * ykk).Real + CalcJ24kk(bk, Y, buses).Real;
                        J[jk, jk] = jkk;
                    }
                    else
                    {
                        var jkn = (vk * Y[jk, jn]).Real;
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        public static MD CreateJ3(MC Y, BU buses)
        {
            var N = buses.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in buses) // row
            {
                var jk = bk.BusIndex;
                var vk = bk.BusVoltage;
                foreach (var bn in buses) // column
                {
                    var jn = bn.BusIndex;
                    if (jk == jn)
                    {
                        var jkk = (vk * CalcJ13kk(bk, Y, buses)).Real;
                        J[jk, jk] = jkk;
                    }
                    else
                    {
                        var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                        var jkn = (-vk * ykn * bn.BusVoltage).Real;
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        public static MD CreateJ4(MC Y, BU buses)
        {
            var N = buses.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in buses) // row
            {
                var jk = bk.BusIndex;
                var vk = bk.BusVoltage;
                foreach (var bn in buses) // column
                {
                    var ykk = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.BusIndex;
                    if (jk == jn)
                    {
                        var jkk = (-vk * ykk).Imaginary + CalcJ24kk(bk, Y, buses).Imaginary;
                        J[jk - 1, jk - 1] = jkk;
                    }
                    else
                    {
                        var jkn = (vk * ykk).Imaginary;
                        J[jk - 1, jn - 1] = jkn;
                    }
                }
            }
            return J;
        }

        public static MD CreateJacobianMatrix(MC Y, BU buses)
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

        #endregion

        public static Result<BU> Solve(EENetwork network,
            double threshold = 0.015, int maxIteration = 100, int minIteration = 50) =>
            Solve(Initialize(network.Buses), network.YMatrix, threshold, maxIteration, minIteration);

        public static Result<BU> Solve(BU buses, MC YMatrix,
            double threshold = 0.015, int maxIteration = 100, int minIteration = 50)
        {
            var Y = YMatrix;
            var busPQ = ReIndexBusPQ(buses)
                .Where(b => b.BusIndex > -1)
                .ToList();

            var J = CreateJacobianMatrix(Y, busPQ);


            throw new NotImplementedException("LFNewtonRaphson.Solve is not implemented yet.");
        }

        public static BU Initialize(IEnumerable<EEBus> buses) =>
            buses.Select(b => new BusResult
            {
                BusData = b,
                BusIndex = b.BusIndex,
                ID = b.ID,
                BusType = b.BusType,
                BusVoltage = new Complex(b.Voltage > 0 ? b.Voltage : 1.0, 0),
                Sbus = new Complex(-b.Pload, -b.Qload)
            })
            .ToList();

        public static BU ReIndexBusPQ(BU buses) 
        {
            var idx = 0;
            var lstBuses = buses.OrderBy(bus => bus.BusData.BusIndex);
            foreach (var b in lstBuses)
                if (b.BusType == BusTypeEnum.PQ)
                    b.BusIndex = idx++;
                else
                    b.BusIndex = -1; // Mark non-PQ buses with -1
            return buses;
        }

    }
}
