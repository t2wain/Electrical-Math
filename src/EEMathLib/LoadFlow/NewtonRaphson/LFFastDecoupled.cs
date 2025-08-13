using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using LFC = EEMathLib.LoadFlow.LFCommon;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.LFNewtonRaphson;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Fast-Decoupled load flow algorithm
    /// </summary>
    public static class LFFastDecoupled
    {

        #region Solve

        /// <summary>
        /// Calculate ewton-Raphson load flow
        /// </summary>
        public static Result<BU> Solve(EENetwork network, double threshold = 0.001, 
            int maxIteration = 20, bool calcJMatrixOnce = false) =>
            Solve(LFNR.Initialize(network.Buses), 
                network.YMatrix, threshold, maxIteration, calcJMatrixOnce);

        /// <summary>
        /// Calculate Newton-Raphson load flow
        /// </summary>
        public static Result<BU> Solve(BU buses, MC YMatrix, double threshold = 0.001, 
            int maxIteration = 20, bool calcJMatrixOnce = false)
        {
            var Y = YMatrix;

            var i = 0;
            // min error found during iterations
            var lastMinErr = Double.MaxValue;
            NRResult res = null;
            while (i++ < maxIteration)
            {
                res = Iterate(buses, YMatrix, threshold, calcJMatrixOnce);
                if (res.IsSolution)
                {
                    // solution is checked before
                    // calculating new result
                    res.Iteration = i - 1;
                    break;
                }
                else if (i == 1 || i % 5 == 0) // check for divergence
                {
                    if (res.MaxErr > lastMinErr)
                        return new Result<BU>
                        {
                            Data = buses,
                            IterationStop = res.Iteration,
                            Error = ErrorEnum.Divergence,
                            ErrorMessage = "Divergence detected during Gauss-Siedel iterations."
                        };
                    else lastMinErr = Math.Min(lastMinErr, res.MaxErr);
                }
                res.Iteration = i; // iteration completed without solution
            }

            // Calculate Pk, Qk for slack bus
            res.NRBuses.SlackBus.Sbus = LFC.CalcPower(res.NRBuses.SlackBus, Y, buses);

            return new Result<BU>
            {
                Data = buses,
                IterationStop = res.Iteration,
                Error = res.IsSolution ? ErrorEnum.NoError : ErrorEnum.MaxIteration,
                ErrorMessage = res.IsSolution ? "" : "Maximum iterations reached without convergence."
            };
        }

        /// <summary>
        /// An iteration of Newton-Raphson load flow calculation.
        /// </summary>
        /// <returns>All calculated values for an iteration</returns>
        internal static NRResult Iterate(BU buses, MC YMatrix, double threshold = 0.0001, bool calcJMatrixOnce = false)
        {
            var res = new NRResult();
            var Y = YMatrix;

            // Step 1
            // Determine classification of each bus
            res.NRBuses = JC.ReIndexBusPQ(buses);

            // Step 2
            LFNR.CalcDeltaPQ(Y, res); // delta P and Q

            if (LFNR.CheckSolution(res, threshold))
                return res;

            // Step 3
            LFNR.UpdatePVBusStatus(res);

            // Step 4
            // Determine classification of each bus
            // PV bus classification might have changed in step 3
            res.NRBuses = JC.ReIndexBusPQ(buses);

            // Step 5
            // Calculate Jacobian matrix
            CalcJMatrix(Y, res, calcJMatrixOnce);

            // Step 6
            CalcAVDelta(res);

            // Step 7
            LFNR.UpdateBusAV(res);

            return res;
        }

        #endregion

        internal static void CalcJMatrix(MC YMatrix, NRResult nrRes, bool calcJMatrixOnce)
        {
            if (calcJMatrixOnce && nrRes.J1Matrix != null)
                return;
            nrRes.J1Matrix = JC.CreateJ1(YMatrix, nrRes.NRBuses);
            nrRes.J4Matrix = JC.CreateJ4(YMatrix, nrRes.NRBuses);
        }

        internal static void CalcAVDelta(NRResult nrRes)
        {
            var j1Size = nrRes.NRBuses.J1Size;
            var j4Size = nrRes.NRBuses.J4Size;

            var PDelta = nrRes.PQDelta.SubMatrix(0, j1Size.Row, 0, 1);
            var ADelta = nrRes.J1Matrix.Solve(PDelta);
            nrRes.ADelta = ADelta.ToColumnMajorArray();

            var QDelta = nrRes.PQDelta.SubMatrix(j1Size.Row, j4Size.Row, 0, 1);
            var VDelta = nrRes.J4Matrix.Solve(QDelta);
            nrRes.VDelta = VDelta.ToColumnMajorArray();

            var AVDelta = Matrix<double>.Build.Dense(j1Size.Row + j4Size.Row, 1);
            AVDelta.SetSubMatrix(0, 0, ADelta);
            AVDelta.SetSubMatrix(j1Size.Row, 0, VDelta);
            nrRes.AVDelta = AVDelta; // delta A and V
        }

    }
}
