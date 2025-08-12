using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using System.Linq;
using System.Numerics;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using LFC = EEMathLib.LoadFlow.LFCommon;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.LFNewtonRaphson;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Fast-Decoupled load flow algorithm
    /// </summary>
    public static class LFFastDecoupled
    {

        /// <summary>
        /// Calculate load flow
        /// </summary>
        public static Result<BU> Solve(EENetwork network,
            double threshold = 0.015, int maxIteration = 20, int minIteration = 5) =>
            Solve(LFNR.Initialize(network.Buses), network.YMatrix, threshold, maxIteration, minIteration);

        /// <summary>
        /// Calculate load flow
        /// </summary>
        public static Result<BU> Solve(BU buses, MC YMatrix,
            double threshold = 0.015, int maxIteration = 20, int minIteration = 5)
        {
            var Y = YMatrix;
            var nrBuses = JC.ReIndexBusPQ(buses);

            #region Iteration

            var isFound = false;
            var i = 0;
            while (i++ < maxIteration)
            {
                #region Calculate delta PQ and VA

                var nrRes = LFNR.CalcDeltaPQ(Y, nrBuses);
                var mxPQdelta = nrRes.PQDelta; // delta P and Q
                var J1 = JC.CreateJ1(Y, nrBuses); // P/A derivative Jacobian matrix
                var J4 = JC.CreateJ4(Y, nrBuses); // Q/V derivative Jacobian matrix

                var mxAdelta = J1.Solve(mxPQdelta.SubMatrix(0, J1.RowCount - 1, 0, 1)); // delta A
                var mxVdelta = J4.Solve(mxPQdelta.SubMatrix(J1.RowCount, J4.RowCount - 1, 0, 1)); // delta V

                #endregion

                #region Update bus PQ and VA

                foreach (var b in nrBuses.Buses)
                {
                    var ik = b.BusIndex;

                    var pcnt = nrBuses.J1Size.Row;
                    var dPQ = new Complex(mxPQdelta[ik, 0], mxPQdelta[ik + pcnt, 0]);
                    var sk = b.Sbus + dPQ;
                    var qgk = sk.Imaginary + b.BusData.Qload;

                    var acnt = J4.RowCount;
                    var dAV = Complex.FromPolarCoordinates(mxVdelta[ik + pcnt, 0], mxAdelta[ik, 0]);
                    var vk = b.BusVoltage + dAV;

                    if (b.BusData.BusType == BusTypeEnum.PQ)
                    {
                        //b.Sbus = sk;
                        //UpdatePErr(b, dPQ.Real, i);
                        //UpdateQErr(b, dPQ.Imaginary, i, false);
                        b.BusVoltage = vk;
                        b.UpdateVErr(dAV.Magnitude, i);
                        b.UpdateAErr(dAV.Phase, i);
                    }
                    else if (b.BusData.BusType == BusTypeEnum.PV
                        && qgk < b.BusData.Qmin)
                    {
                        b.BusType = BusTypeEnum.PQ;
                        //b.Sbus = new Complex(b.BusData.Pgen, b.BusData.Qmin);
                        //UpdateQErr(b, 0, i, true);
                        b.BusVoltage = vk;
                        b.UpdateVErr(dAV.Magnitude, i);
                        b.UpdateAErr(dAV.Phase, i);
                    }
                    else if (b.BusData.BusType == BusTypeEnum.PV
                        && qgk > b.BusData.Qmax)
                    {
                        b.BusType = BusTypeEnum.PQ;
                        //b.Sbus = new Complex(b.BusData.Pgen, b.BusData.Qmax);
                        //UpdateQErr(b, 0, i, true);
                        b.BusVoltage = vk;
                        b.UpdateVErr(dAV.Magnitude, i);
                        b.UpdateAErr(dAV.Phase, i);
                    }
                    else if (b.BusData.BusType == BusTypeEnum.PV)
                    {
                        b.BusType = BusTypeEnum.PV;
                        //b.Sbus = new Complex(b.BusData.Pgen, sk.Imaginary);
                        //UpdateQErr(b, dPQ.Imaginary, i, false);
                        b.BusVoltage = Complex.FromPolarCoordinates(b.BusData.Voltage, vk.Phase);
                        b.UpdateVErr(0, i, true);
                        b.UpdateAErr(dAV.Phase, i);
                    }
                }

                #endregion

                #region Check for solution and divergence

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
            slackBus.Sbus = LFC.CalcPower(slackBus, Y, buses);

            return new Result<BU>
            {
                Data = buses,
                IterationStop = i,
                Error = isFound ? ErrorEnum.NoError : ErrorEnum.MaxIteration,
                ErrorMessage = isFound ? "" : "Maximum iterations reached without convergence."
            };
        }

        #region Track error and convergence

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
