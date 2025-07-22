using EEMathLib.DTO;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow
{
    public static class LFNewtonRaphson2
    {
        #region Jacobian Matrix

        public static double CalcJ1kk(BusResult bk, MC Y, BU buses)
        {
            var jk = bk.Pidx;
            var vk = bk.BusVoltage;
            var skk = buses
                .Where(bn => bn.Pidx != jk)
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

        public static double CalcJ3kk(BusResult bk, MC Y, BU buses)
        {
            var jk = bk.Qidx;
            var vk = bk.BusVoltage;
            var skk = buses
                .Where(bn => bn.Qidx != jk)
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

        public static MD CreateJ1(MC Y, BU buses)
        {
            var lstBus = buses.Where(b => b.Pidx > -1).ToList();
            var N = lstBus.Count;
            var J = MD.Build.Dense(N, N);

            foreach (var bk in lstBus) // row
            {
                var jk = bk.Pidx;
                var vk = bk.BusVoltage;
                foreach (var bn in lstBus) // column
                {
                    var jn = bn.Pidx;
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

        public static MD CreateJ2(MC Y, BU buses)
        {
            var lstBus = buses.Where(b => b.Pidx > -1).ToList();
            var N = lstBus.Count();
            var J = MD.Build.Dense(N, N);

            foreach (var bk in lstBus) // row
            {
                var jk = bk.Pidx;
                var vk = bk.BusVoltage;
                foreach (var bn in lstBus) // column
                {
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.Pidx;
                    if (jk == jn)
                    {
                        var jkk = CalcJ2kk(bk, Y, lstBus);
                        J[jk, jk] = jkk;
                    }
                    else
                    {
                        var jkn = (vk * ykn).Real;
                        J[jk, jn] = jkn;
                    }
                }
            }
            return J;
        }

        public static MD CreateJ3(MC Y, BU buses)
        {
            var lstBus = buses.Where(b => b.Qidx > -1).ToList();
            var N = lstBus.Count;
            var J = MD.Build.Dense(N, N);

            foreach (var bk in lstBus) // row
            {
                var jk = bk.Qidx;
                var vk = bk.BusVoltage;
                foreach (var bn in lstBus) // column
                {
                    var jn = bn.Qidx;
                    if (jk == jn)
                    {
                        var jkk = CalcJ3kk(bk, Y, lstBus);
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
            var lstBus = buses.Where(b => b.Qidx > -1).ToList();
            var N = lstBus.Count;
            var J = MD.Build.Dense(N, N);

            foreach (var bk in lstBus) // row
            {
                var jk = bk.Qidx;
                var vk = bk.BusVoltage;
                foreach (var bn in lstBus) // column
                {
                    var ykn = Y[bk.BusData.BusIndex, bn.BusData.BusIndex];
                    var jn = bn.Qidx;
                    if (jk == jn)
                    {
                        var jkk = CalcJ4kk(bk, Y, lstBus);
                        J[jk, jk] = jkk;
                    }
                    else
                    {
                        var jkn = (vk * ykn).Imaginary;
                        J[jk, jn] = jkn;
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
            var busesPQ = ReIndexBusPQ(buses)
                .Where(b => b.BusIndex > -1)
                .ToList();

            #region Iteration

            var isFound = false;
            var i = 0;
            while (i++ < maxIteration)
            {
                #region Calculate delta PQ and VA

                var N = busesPQ.Count;
                var mxPQdelta = CalcDeltaPower(Y, busesPQ); // delta P and Q
                var J = CreateJacobianMatrix(Y, busesPQ);

                var mxAVdelta = J.Solve(mxPQdelta); // delta A and V

                #endregion

                #region Update bus PQ and VA

                foreach (var b in busesPQ)
                {
                    var ik = b.BusIndex;
                    var dPQ = new Complex(mxPQdelta[ik, 0], mxPQdelta[ik + N, 0]);
                    var sk = b.Sbus + dPQ;
                    var vk = b.BusVoltage + Complex.FromPolarCoordinates(mxAVdelta[ik + N, 0], mxAVdelta[ik, 0]);
                    if (b.BusData.BusType == BusTypeEnum.PQ)
                    {
                        b.Sbus = sk;
                        UpdatePErr(b, dPQ.Real, i);
                        UpdateQErr(b, dPQ.Imaginary, i, false);
                        b.BusVoltage = vk;
                    }
                    else if (b.BusData.BusType == BusTypeEnum.PV
                        && sk.Imaginary < b.BusData.Qmin)
                    {
                        b.BusType = BusTypeEnum.PQ;
                        b.Sbus = new Complex(b.BusData.Pgen, b.BusData.Qmin);
                        UpdateQErr(b, 0, i, true);
                        b.BusVoltage = vk;
                    }
                    else if (b.BusData.BusType == BusTypeEnum.PV
                        && sk.Imaginary > b.BusData.Qmax)
                    {
                        b.BusType = BusTypeEnum.PQ;
                        b.Sbus = new Complex(b.BusData.Pgen, b.BusData.Qmax);
                        UpdateQErr(b, 0, i, true);
                        b.BusVoltage = vk;
                    }
                    else if (b.BusData.BusType == BusTypeEnum.PV)
                    {
                        b.BusType = BusTypeEnum.PV;
                        b.Sbus = new Complex(b.BusData.Pgen, sk.Imaginary);
                        UpdateQErr(b, dPQ.Imaginary, i, false);
                        b.BusVoltage = Complex.FromPolarCoordinates(b.BusData.Voltage, vk.Phase);
                    }
                }

                #endregion

                #region Check for solution

                if (i < minIteration)
                    continue;
                else if (IsSolutionFound(buses, threshold))
                {
                    isFound = true;
                    break;
                }
                else if (IsDiverged(buses, i))
                {
                    return new Result<BU>
                    {
                        Data = buses,
                        IterationStop = i,
                        Error = ErrorEnum.Divergence,
                        ErrorMessage = "Divergence detected during Gauss-Siedel iterations."
                    };
                }

                #endregion

            }

            #endregion

            // Calculate Pk, Qk for slack bus
            var slackBus = buses.FirstOrDefault(b => b.BusType == BusTypeEnum.Slack);
            slackBus.Sbus = CalcP(slackBus, Y, buses);

            return new Result<BU>
            {
                Data = buses,
                IterationStop = i,
                Error = isFound ? ErrorEnum.NoError : ErrorEnum.MaxIteration,
                ErrorMessage = isFound ? "" : "Maximum iterations reached without convergence."
            };
        }

        #region Calculate

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
            var PAidx = 0;
            var QVidx = 0;
            var idx = 0;
            var lstBuses = buses.OrderBy(bus => bus.BusData.BusIndex);
            foreach (var b in lstBuses)
            {
                if (b.BusType == BusTypeEnum.Slack)
                    b.BusIndex = -1;
                else
                    b.BusIndex = idx++;

                if (b.BusType == BusTypeEnum.PQ)
                {
                    b.Pidx = b.Aidx = PAidx++;
                    b.Qidx = b.Vidx = QVidx++;
                }
                else if (b.BusType == BusTypeEnum.PV)
                {
                    b.Qidx = b.Aidx = QVidx++;
                    b.Pidx = b.Vidx = -1;
                }
                else
                {
                    b.Pidx = b.Qidx = b.Vidx = b.Aidx = b.BusIndex = -1;
                }
            }
            return buses;
        }

        static Complex CalcPower(BusResult bus, MC Y, BU buses)
        {
            var vk = bus.BusVoltage; // given bus voltage
            var jk = bus.BusData.BusIndex; // bus index in Y matrix

            var N = buses.Count();

            var sv = buses
                .Select(bn =>
                {
                    var jn = bn.BusData.BusIndex; // bus index in Y matrix
                    var ykn = Y[jk, jn];
                    var yv = ykn * bn.BusVoltage;
                    return yv;
                })
                .Aggregate((v1, v2) => v1 + v2);
            var sk = vk * sv.Conjugate();
            return sk;
        }

        public static double CalcP(BusResult bus, MC Y, BU buses) =>
            CalcPower(bus, Y, buses.Where(b => b.BusType == BusTypeEnum.PQ).ToList()).Real;

        public static double CalcQ(BusResult bus, MC Y, BU buses) =>
            CalcPower(bus, Y, buses).Imaginary;

        public static MD CalcDeltaPower(MC Y, BU buses)
        {
            var PN = buses.Max(b => b.Pidx) + 1;
            var QN = buses.Max(b => b.Qidx) + 1;
            var N = PN + QN;
            var mx = MD.Build.Dense(N, 1, 0.0);
            foreach (var b in buses)
            {
                if (b.BusType == BusTypeEnum.PQ)
                {
                    var pdk = b.Sbus.Real - CalcP(b, Y, buses);
                    mx[b.Pidx, 0] = pdk; // delta P
                }
                else if (b.BusType == BusTypeEnum.PV)
                {
                    var qdk = b.Sbus.Imaginary - CalcQ(b, Y, buses);
                    mx[b.Qidx + PN, 0] = qdk; // delta Q
                }
            }
            return mx;
        }

        public static IEnumerable<EEBus> CalcResult(BU buses) =>
            buses.Select(b => new EEBus
            {
                BusIndex = b.BusData.BusIndex,
                ID = b.ID,
                BusType = b.BusType,
                Voltage = b.BusVoltage.Magnitude,
                Angle = Phasor.ConvertRadianToDegree(b.BusVoltage.Phase),
                Pgen = b.Sbus.Real + b.BusData.Pload,
                Qgen = b.Sbus.Imaginary + b.BusData.Qload,
                Pload = b.BusData.Pload,
                Qload = b.BusData.Qload,
                Qmin = b.BusData.Qmin,
                Qmax = b.BusData.Qmax
            })
            .ToList();

        #endregion

        #region Track error and convergence

        static void UpdatePErr(BusResult bus, double pdelta, int iteration)
        {
            bus.Err.PErr = Math.Abs(pdelta);
            if (iteration == 1 || iteration % 6 == 0)
                bus.ErrRef.PErr = bus.Err.PErr;
        }

        static void UpdateQErr(BusResult bus, double qdelta, int iteration, bool setRef)
        {
            bus.Err.QErr = Math.Abs(qdelta);
            if (setRef)
                bus.Err.QErr = 0;
            if (iteration == 1 || iteration % 6 == 0 || setRef)
                bus.ErrRef.QErr = bus.Err.QErr;
        }

        static bool IsSolutionFound(BU buses, double threshold)
        {
            var cv = true;
            foreach (var bus in buses.Where(b => b.BusType != BusTypeEnum.Slack))
            {
                cv = cv && bus.Err.QErr < threshold;
                if (bus.BusData.BusType != BusTypeEnum.PV)
                {
                    cv = cv && bus.Err.PErr < threshold;
                }
            }
            return cv;
        }

        static bool IsDiverged(BU buses, int iteration)
        {
            if (iteration % 5 != 0)
                return false;

            var cv = true;
            foreach (var bus in buses.Where(b => b.BusType != BusTypeEnum.Slack))
            {
                cv = cv && bus.Err.QErr < bus.ErrRef.QErr;
                if (bus.BusData.BusType != BusTypeEnum.PV)
                    cv = cv && bus.Err.PErr < bus.ErrRef.PErr;
            }
            return !cv;
        }
        #endregion
    }
}
