using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using LFC = EEMathLib.LoadFlow.LFCommon;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Newton-Raphson load flow algorithm
    /// </summary>
    public static class LFNewtonRaphson
    {
        #region Solve

        /// <summary>
        /// Calculate ewton-Raphson load flow
        /// </summary>
        public static Result<BU> Solve(EENetwork network,
            double threshold = 0.015, int maxIteration = 20) =>
            Solve(Initialize(network.Buses), network.YMatrix, threshold, maxIteration);

        /// <summary>
        /// Calculate Newton-Raphson load flow
        /// </summary>
        public static Result<BU> Solve(BU buses, MC YMatrix,
            double threshold = 0.015, int maxIteration = 20)
        {
            var Y = YMatrix;

            var i = 0;
            // min error found during iterations
            var lastMinErr = Double.MaxValue;
            NRResult res = null;
            while (i++ < maxIteration)
            {

                res = Iterate(buses, YMatrix, threshold);
                res.Iteration = i;
                if (res.IsSolution)
                    break;
                else if (i == 1 || i % 5 == 0) // check for divergence
                {
                    if (res.MaxErr > lastMinErr)
                        return new Result<BU>
                        {
                            Data = buses,
                            IterationStop = i,
                            Error = ErrorEnum.Divergence,
                            ErrorMessage = "Divergence detected during Gauss-Siedel iterations."
                        };
                    else lastMinErr = Math.Min(lastMinErr, res.MaxErr);
                }

            }

            // Calculate Pk, Qk for slack bus
            res.NRBuses.SlackBus.Sbus = LFC.CalcPower(res.NRBuses.SlackBus, Y, buses);

            return new Result<BU>
            {
                Data = buses,
                IterationStop = i,
                Error = res.IsSolution ? ErrorEnum.NoError : ErrorEnum.MaxIteration,
                ErrorMessage = res.IsSolution ? "" : "Maximum iterations reached without convergence."
            };
        }

        /// <summary>
        /// An iteration of Newton-Raphson load flow calculation.
        /// </summary>
        /// <returns>All calculated values for an iteration</returns>
        public static NRResult Iterate(BU buses, MC YMatrix, double threshold = 0.0001)
        {
            var res = new NRResult();
            var Y = YMatrix;

            // Step 1
            res.NRBuses = JC.ReIndexBusPQ(buses);

            // Step 2
            CalcDeltaPQ(Y, res.NRBuses, out NRResult temp); // delta P and Q
            res.PQDelta = temp.PQDelta;
            res.PCal = temp.PCal;
            res.QCal = temp.QCal;
                                                     
            res.MaxErr = res.PQDelta
                .ToColumnMajorArray()
                .Select(v => Math.Abs(v))
                .Max();

            if (res.MaxErr <= threshold)
            {
                res.IsSolution = true;
                return res;
            }

            // Step 3
            var pcnt = res.NRBuses.J1Size.Row;
            foreach(var b in res.NRBuses.AllPVBuses)
            {
                var dP = res.PQDelta[b.Pidx, 0];
                var dQ = res.PQDelta[b.Qidx + pcnt, 0];
                var dPQ = new Complex(dP, dQ);
                var sk = b.Sbus + dPQ;
                var (snxt, bt) = LFC.CalcMaxQk(b, sk);
                b.Sbus = snxt;
                b.BusType = bt;
            }

            // Step 4
            res.NRBuses = JC.ReIndexBusPQ(buses);

            // Step 5
            res.JMatrix = JC.CreateJMatrix(Y, res.NRBuses);

            // Step 6
            res.AVDelta = res.JMatrix.Solve(res.PQDelta); // delta A and V
            res.ADelta = new double[res.NRBuses.J1Size.Row];
            res.VDelta = new double[res.NRBuses.J3Size.Row];

            #region Update bus PQ and VA

            // Step 7
            pcnt = res.NRBuses.J1Size.Row;
            foreach (var b in res.NRBuses.Buses)
            {
                var ik = b.Pidx;
                var dA = res.AVDelta[b.Aidx, 0];
                res.ADelta[b.Aidx] = dA;

                if (b.BusType == BusTypeEnum.PQ)
                {
                    var dV = res.AVDelta[b.Vidx + pcnt, 0];
                    res.VDelta[b.Vidx] = dV;
                    var dAV = Complex.FromPolarCoordinates(dV, dA);
                    var vk = b.BusVoltage + dAV;
                    b.BusVoltage = vk;
                }

                else if (b.BusType == BusTypeEnum.PV)
                {
                    var phase = b.BusVoltage.Phase + dA;
                    b.BusVoltage = Complex.FromPolarCoordinates(b.BusData.Voltage, phase);
                }
            }

            #endregion

            return res;
        }

        #endregion

        #region Calculate

        public static BU Initialize(IEnumerable<EEBus> buses) =>
            buses
                .OrderBy(b => b.BusIndex)
                .Select(b => new BusResult
                {
                    BusData = b,
                    BusIndex = -1,
                    Aidx = -1,
                    Vidx = -1,
                    Qidx = -1,
                    Pidx = -1,
                    ID = b.ID,
                    BusType = b.BusType,
                    BusVoltage = new Complex(b.Voltage > 0 ? b.Voltage : 1.0, 0),
                    Sbus = new Complex(b.Pgen - b.Pload, -b.Qload)
                })
                .ToList();

        public static MD CalcDeltaPQ(MC Y, JC.NRBuses nrBuses)
        {
            CalcDeltaPQ(Y, nrBuses, out NRResult res);
            return res.PQDelta;
        }

        public static void CalcDeltaPQ(MC Y, JC.NRBuses nrBuses, out NRResult nrRes)
        {
            var res = new NRResult
            {
                PCal = new double[nrBuses.Buses.Count()],
                QCal = new double[nrBuses.PQBuses.Count()]
            };

            var buses = nrBuses.Buses;
            var N = nrBuses.J1Size.Row;
            var mx = MD.Build.Dense(nrBuses.JSize.Row, 1, 0.0);
            foreach (var bk in buses)
            {
                var sk = LFC.CalcPower(bk, Y, nrBuses.AllBuses);
                res.PCal[bk.Pidx] = sk.Real;

                var pdk = bk.Sbus.Real - sk.Real;
                mx[bk.Pidx, 0] = pdk; // delta P

                if (bk.BusType == BusTypeEnum.PQ)
                {
                    res.QCal[bk.Qidx] = sk.Imaginary;
                    var qdk = bk.Sbus.Imaginary - sk.Imaginary;
                    mx[bk.Qidx + N, 0] = qdk; // delta Q
                }
            }

            res.PQDelta = mx;
            nrRes = res;
        }

        #endregion
    }
}
