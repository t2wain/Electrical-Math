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
            // Determine classification of each bus
            res.NRBuses = JC.ReIndexBusPQ(buses);

            // Step 2
            CalcDeltaPQ(Y, res); // delta P and Q
                                                     
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
            UpdatePVBusStatus(res);

            // Step 4
            // Determine classification of each bus
            // PV bus classification might have changed in step 3
            res.NRBuses = JC.ReIndexBusPQ(buses); 

            // Step 5
            // Calculate Jacobian matrix
            res.JMatrix = JC.CreateJMatrix(Y, res.NRBuses);

            // Step 6
            CalcAVDelta(res);

            // Step 7
            UpdateBusAV(res);

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

        public static NRResult CalcDeltaPQ(MC Y, JC.NRBuses nrBuses)
        {
            var nrRes = new NRResult
            {
                NRBuses = nrBuses,
            };
            CalcDeltaPQ(Y, nrRes);
            return nrRes;
        }

        public static void CalcDeltaPQ(MC Y, NRResult nrRes)
        {
            var nrBuses = nrRes.NRBuses;
            nrRes.PCal = new double[nrBuses.Buses.Count()];
            nrRes.QCal = new double[nrBuses.PQBuses.Count()];

            var buses = nrBuses.Buses;
            var pcnt = nrBuses.J1Size.Row;
            var mx = MD.Build.Dense(nrBuses.JSize.Row, 1, 0.0);
            nrRes.PQDelta = mx;
            foreach (var bk in buses)
            {
                var sk = LFC.CalcPower(bk, Y, nrBuses.AllBuses);
                nrRes.PCal[bk.Pidx] = sk.Real; // save dP

                var pdk = bk.Sbus.Real - sk.Real;
                mx[bk.Pidx, 0] = pdk; // save dP to DeltaPQ

                if (bk.BusType == BusTypeEnum.PQ)
                {
                    nrRes.QCal[bk.Qidx] = sk.Imaginary; // save dQ calc
                    var qdk = bk.Sbus.Imaginary - sk.Imaginary;
                    mx[bk.Qidx + pcnt, 0] = qdk; // save dQ to DeltaPQ
                }
            }
        }

        public static void UpdatePVBusStatus(JC.NRBuses nrBuses, MD mxPQDelta)
        {
            var nrRes = new NRResult
            {
                NRBuses = nrBuses,
                PQDelta = mxPQDelta
            };
            UpdatePVBusStatus(nrRes);
        }

        public static void UpdatePVBusStatus(NRResult nrRes)
        {
            var pcnt = nrRes.NRBuses.J1Size.Row;
            foreach (var b in nrRes.NRBuses.AllPVBuses)
            {
                var dP = nrRes.PQDelta[b.Pidx, 0];
                var dQ = nrRes.PQDelta[b.Qidx + pcnt, 0];
                var dPQ = new Complex(dP, dQ);
                var sk = b.Sbus + dPQ;
                var (snxt, bt) = LFC.CalcMaxQk(b, sk);
                b.Sbus = snxt;
                // Determine if PV bus should be
                // switched to PQ bus or back to PV bus
                b.BusType = bt;

            }
        }

        public static void CalcAVDelta(MD JMatrix, JC.NRBuses nrBuses, MD mxPQDelta)
        {
            var nrRes = new NRResult
            {
                NRBuses = nrBuses,
                PQDelta = mxPQDelta,
                JMatrix = JMatrix
            };
            CalcAVDelta(nrRes);
        }

        public static void CalcAVDelta(NRResult nrRes)
        {
            nrRes.AVDelta = nrRes.JMatrix.Solve(nrRes.PQDelta); // delta A and V
            var acnt = nrRes.NRBuses.J1Size.Row;
            nrRes.ADelta = new double[acnt];
            nrRes.VDelta = new double[nrRes.NRBuses.J3Size.Row];
            foreach (var b in nrRes.NRBuses.Buses)
            {
                var ik = b.Pidx;
                var dA = nrRes.AVDelta[b.Aidx, 0];
                nrRes.ADelta[b.Aidx] = dA; // save dA calculation

                if (b.BusType == BusTypeEnum.PQ)
                {
                    var dV = nrRes.AVDelta[b.Vidx + acnt, 0];
                    nrRes.VDelta[b.Vidx] = dV; // save dV calculation
                }
            }
        }

        public static void UpdateBusAV(JC.NRBuses nrBuses, MD mxAVDelta)
        {
            var nrRes = new NRResult
            {
                NRBuses = nrBuses,
                AVDelta = mxAVDelta
            };
            UpdateBusAV(nrRes);
        }

        public static void UpdateBusAV(NRResult nrRes)
        {
            var acnt = nrRes.NRBuses.J1Size.Row;
            nrRes.ABus = new double[nrRes.NRBuses.AllBuses.Count()];
            nrRes.VBus = new double[nrRes.NRBuses.AllBuses.Count()];

            // slack bus
            var sb = nrRes.NRBuses.SlackBus;
            nrRes.ABus[sb.BusData.BusIndex] = sb.BusVoltage.Phase;
            nrRes.VBus[sb.BusData.BusIndex] = sb.BusVoltage.Magnitude;

            foreach (var b in nrRes.NRBuses.Buses)
            {
                var ik = b.Pidx;
                var dA = nrRes.AVDelta[b.Aidx, 0];

                // update V and A
                if (b.BusType == BusTypeEnum.PQ)
                {
                    var dV = nrRes.AVDelta[b.Vidx + acnt, 0];
                    var vk = Complex.FromPolarCoordinates(
                        b.BusVoltage.Magnitude + dV, 
                        b.BusVoltage.Phase + dA);
                    nrRes.ABus[b.BusData.BusIndex] = vk.Phase;
                    nrRes.VBus[b.BusData.BusIndex] = vk.Magnitude;
                    b.BusVoltage = vk; // update voltage
                }

                // update A only
                else if (b.BusType == BusTypeEnum.PV)
                {
                    var phase = b.BusVoltage.Phase + dA;
                    b.BusVoltage = Complex.FromPolarCoordinates(b.BusData.Voltage, phase); // update Angle
                    nrRes.ABus[b.BusData.BusIndex] = b.BusVoltage.Phase;
                    nrRes.VBus[b.BusData.BusIndex] = b.BusVoltage.Magnitude;
                }
            }
        }

        #endregion
    }
}
