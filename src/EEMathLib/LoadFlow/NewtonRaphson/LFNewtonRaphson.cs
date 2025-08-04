using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using LFC = EEMathLib.LoadFlow.LFCommon;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Newton-Rappson load flow algorithm
    /// </summary>
    public static class LFNewtonRaphson
    {

        /// <summary>
        /// Calculate load flow
        /// </summary>
        public static Result<BU> Solve(EENetwork network,
            double threshold = 0.015, int maxIteration = 20, int minIteration = 5) =>
            Solve(Initialize(network.Buses), network.YMatrix, threshold, maxIteration, minIteration);

        /// <summary>
        /// Calculate load flow
        /// </summary>
        public static Result<BU> Solve(BU buses, MC YMatrix,
            double threshold = 0.015, int maxIteration = 20, int minIteration = 5)
        {
            var Y = YMatrix;

            #region Iteration

            var isFound = false;
            var i = 0;
            while (i++ < maxIteration)
            {
                var nrBuses = JC.ReIndexBusPQ(buses);

                #region Calculate delta PQ and VA

                var mxPQdelta = CalcDeltaPQ(Y, nrBuses); // delta P and Q
                var J = Jacobian.CreateJMatrix(Y, nrBuses);

                var mxAVdelta = J.Solve(mxPQdelta); // delta A and V

                #endregion

                #region Update bus PQ and VA

                foreach (var b in nrBuses.Buses)
                {
                    var ik = b.Pidx;
                    var pcnt = nrBuses.J1Size.Row;

                    var dA = mxAVdelta[b.Aidx, 0];
                    if (b.BusData.BusType == BusTypeEnum.PQ)
                    {
                        var dV = mxAVdelta[b.Vidx + pcnt, 0];
                        var dAV = Complex.FromPolarCoordinates(dV, dA);
                        var vk = b.BusVoltage + dAV;
                        b.BusVoltage = vk;
                        b.UpdateVErr(dAV.Magnitude, i);
                        b.UpdateAErr(dAV.Phase, i);
                    }

                    else if (b.BusData.BusType == BusTypeEnum.PV)
                    {
                        var dP = mxPQdelta[b.Pidx, 0];
                        var dQ = mxPQdelta[b.Qidx + pcnt, 0];
                        var dPQ = new Complex(dP, dQ);
                        var sk = b.Sbus + dPQ;
                        var (snxt, bt) = LFC.CalcMaxQk(b, sk);

                        b.Sbus = snxt;
                        b.BusType = bt;
                        if (bt == BusTypeEnum.PQ)
                        {
                            var dV = mxAVdelta[b.Vidx + pcnt, 0];
                            var dAV = Complex.FromPolarCoordinates(dV, dA);
                            var vk = b.BusVoltage + dAV;
                            b.BusVoltage = vk;
                            b.UpdateVErr(dAV.Magnitude, i);
                            b.UpdateAErr(dAV.Phase, i);
                        }
                        else
                        {
                            var phase = b.BusVoltage.Phase + dA;
                            b.BusVoltage = Complex.FromPolarCoordinates(b.BusData.Voltage, phase);
                            b.UpdateVErr(0, i);
                            b.UpdateAErr(dA, i);
                        }
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
            var buses = nrBuses.Buses;
            var N = nrBuses.J1Size.Row;
            var mx = MD.Build.Dense(nrBuses.JSize.Row, 1, 0.0);
            foreach (var b in buses)
            {
                var sk = LFC.CalcPower(b, Y, buses);

                var pdk = b.Sbus.Real - sk.Real;
                mx[b.Pidx, 0] = pdk; // delta P

                if (b.BusType == BusTypeEnum.PQ)
                {
                    var qdk = b.Sbus.Imaginary - sk.Imaginary;
                    mx[b.Qidx + N, 0] = qdk; // delta Q
                }
            }
            return mx;
        }

        #endregion

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
