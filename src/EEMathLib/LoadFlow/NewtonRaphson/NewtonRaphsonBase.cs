using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
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
    public abstract class NewtonRaphsonBase : ILFSolver
    {
        #region Solve

        ///// <summary>
        ///// Calculate ewton-Raphson load flow
        ///// </summary>
        //public virtual Result<BU> Solve(EENetwork network,
        //    double threshold = 0.015, int maxIteration = 20) =>
        //    Solve(Initialize(network.Buses), network.YMatrix, threshold, maxIteration);

        /// <summary>
        /// Calculate Newton-Raphson load flow
        /// </summary>
        public virtual Result<LFResult> Solve(EENetwork network,
            double threshold = 0.015, int maxIteration = 20)
        {
            var Y = network.YMatrix;
            var buses = Initialize(network.Buses);

            var i = 0;
            // min error found during iterations
            var lastMinErr = Double.MaxValue;
            NRResult nrRes = new NRResult();
            while (i++ < maxIteration)
            {
                // allow some data from reulst of previous iteration
                // to be available in the next iteration
                nrRes = InitResult(nrRes);
                Iterate(nrRes, buses, Y, threshold);
                if (nrRes.IsSolution)
                {
                    // solution is checked before
                    // calculating new result
                    nrRes.Iteration = i - 1;
                    break;
                }
                else if (i == 1 || i % 5 == 0) // check for divergence
                {
                    if (nrRes.MaxErr > lastMinErr)
                        return new Result<LFResult>
                        {
                            Data = new LFResult { Buses = buses, Lines = null },
                            IterationStop = nrRes.Iteration,
                            Error = ErrorEnum.Divergence,
                            ErrorMessage = "Divergence detected during Gauss-Siedel iterations."
                        };
                    else lastMinErr = Math.Min(lastMinErr, nrRes.MaxErr);
                }
                nrRes.Iteration = i; // iteration completed without solution
            }

            // Calculate Pk, Qk for slack bus
            nrRes.NRBuses.SlackBus.Sbus = LFC.CalcPower(nrRes.NRBuses.SlackBus, Y, buses);

            // Prepare solution result
            var lfrres = CalcResult(network, buses, nrRes.IsSolution);

            return new Result<LFResult>
            {
                Data = lfrres,
                IterationStop = nrRes.Iteration,
                Error = nrRes.IsSolution ? ErrorEnum.NoError : ErrorEnum.MaxIteration,
                ErrorMessage = nrRes.IsSolution ? "" : "Maximum iterations reached without convergence."
            };
        }

        /// <summary>
        /// An iteration of Newton-Raphson load flow calculation.
        /// </summary>
        /// <param name="nrRes">Result of this iteration will be saved to this object.
        /// This object may have data from the result of previous iteration if needed
        /// in the current iteration.</param>
        /// <returns>All calculated values for an iteration</returns>
        internal void Iterate(NRResult nrRes, BU buses, MC YMatrix, double threshold = 0.0001)
        {
            //var res = InitResult(prevRes);
            var Y = YMatrix;

            // Step 1
            // Determine classification of each bus
            nrRes.NRBuses = JC.ReIndexBusPQ(buses);

            // Step 2
            CalcDeltaPQ(Y, nrRes); // delta P and Q

            if (CheckSolution(nrRes, threshold))
                return;

            // Step 3
            UpdatePVBusStatus(nrRes);

            // Step 4
            // Determine classification of each bus
            // PV bus classification might have changed in step 3
            nrRes.NRBuses = JC.ReIndexBusPQ(buses);

            // Step 5
            // Calculate Jacobian matrix
            CalcJMatrix(Y, nrRes);

            // Step 6
            CalcAVDelta(nrRes);

            // Step 7
            UpdateBusAV(nrRes);

            //return res;
        }

        #endregion

        #region Calculate

        internal static BU Initialize(IEnumerable<EEBus> buses) =>
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

        /// <summary>
        /// Create a new result object for the next iteration.
        /// Provide the option to save data from the result of previous iteration
        /// that might be needed in the next iteration.
        /// </summary>
        /// <param name="curRes">Result from previous iteration</param>
        /// <returns>Result for use in the next iteration</returns>
        internal virtual NRResult InitResult(NRResult curRes) =>
            new NRResult { Iteration = curRes.Iteration };

        /// <summary>
        /// Calculation step for each iteration
        /// </summary>
        internal static NRResult CalcDeltaPQ(MC Y, JC.NRBuses nrBuses)
        {
            var nrRes = new NRResult
            {
                NRBuses = nrBuses,
            };
            CalcDeltaPQ(Y, nrRes);
            return nrRes;
        }

        /// <summary>
        /// Calculation step for each iteration
        /// </summary>
        internal static void CalcDeltaPQ(MC Y, NRResult nrRes)
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

        /// <summary>
        /// Calculation step for each iteration
        /// </summary>
        internal static bool CheckSolution(NRResult res, double threshold)
        {
            res.MaxErr = res.PQDelta
                .ToColumnMajorArray()
                .Select(v => Math.Abs(v))
                .Max();
            res.IsSolution = res.MaxErr <= threshold;
            return res.IsSolution;
        }

        internal static void UpdatePVBusStatus(JC.NRBuses nrBuses, MD mxPQDelta)
        {
            var nrRes = new NRResult
            {
                NRBuses = nrBuses,
                PQDelta = mxPQDelta
            };
            UpdatePVBusStatus(nrRes);
        }

        /// <summary>
        /// Calculation step for each iteration
        /// </summary>
        internal static void UpdatePVBusStatus(NRResult nrRes)
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
                var statusChanged = b.BusType != bt;
                nrRes.PVBusStatusChanged |= statusChanged;
                b.BusType = bt;

            }
        }

        /// <summary>
        /// Not implmented. Calculation step for each iteration.
        /// Require sub-class to override the method as needed.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        virtual internal void CalcJMatrix(MC YMatrix, NRResult nrRes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implmented. Calculation step for each iteration.
        /// Require sub-class to override the method as needed.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        virtual internal void CalcAVDelta(NRResult nrRes)
        {
            throw new NotImplementedException();
        }

        internal static void UpdateBusAV(JC.NRBuses nrBuses, MD mxAVDelta)
        {
            var nrRes = new NRResult
            {
                NRBuses = nrBuses,
                AVDelta = mxAVDelta
            };
            UpdateBusAV(nrRes);
        }

        /// <summary>
        /// Calculation step for each iteration
        /// </summary>
        internal static void UpdateBusAV(NRResult nrRes)
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

        internal static LFResult CalcResult(EENetwork network, BU buses, bool isSolution)
        {
            // Calculate power flow in lines
            IEnumerable<LineResult> lineRes = null;
            if (isSolution)
                lineRes = LFC.CalcPower(network.Lines, buses);

            // Prepare solution result
            var lfrres = new LFResult { Buses = buses, Lines = lineRes };
            return lfrres;
        }

        #endregion
    }
}
