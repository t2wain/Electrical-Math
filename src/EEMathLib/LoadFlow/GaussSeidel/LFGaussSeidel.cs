using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using LFC = EEMathLib.LoadFlow.LFCommon;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.GaussSeidel
{
    /// <summary>
    /// Gauss-Siedel load flow algorithm
    /// </summary>
    public class LFGaussSeidel : ILFSolver
    {
        #region Solve

        /// <summary>
        /// Calculate load flow
        /// </summary>
        public Result<LFResult> Solve(EENetwork network, 
            double threshold = 0.0001, int maxIteration = 100)
        {
            var Y = network.YMatrix;
            var buses = Initialize(network.Buses);

            var slackBus = buses
                .Where(b => b.BusData.BusType == BusTypeEnum.Slack)
                .First();

            #region Iteration

            GSResult res = null;
            var lastErr = double.MaxValue;
            foreach (var i in Enumerable.Range(0, maxIteration))
            {
                res = Iterate(Y, buses);
                res.Iteration = i;

                if (res.MaxVErr <= threshold)
                {
                    res.IsSolution = true;
                    break;
                }
                
                if (i == 1)
                {
                    lastErr = res.MaxVErr;
                }
                else if (i % 5 == 0)
                {
                    if (res.MaxVErr > lastErr)
                        return new Result<LFResult>
                        {
                            Data = new LFResult { Buses = buses, Lines = null },
                            IterationStop = i,
                            Error = ErrorEnum.Divergence,
                            ErrorMessage = "Divergence detected during Gauss-Siedel iterations."
                        };
                    else lastErr = res.MaxVErr;
                }

            }

            #endregion

            // Calculate Pk, Qk for slack bus
            slackBus.Sbus = LFC.CalcBusPower(slackBus, Y, buses);

            // Calculate power flow in lines
            IEnumerable<LineResult> lineRes = null;
            if (res.IsSolution)
                lineRes = LFC.CalcLinePower(network.Lines, buses);

            // Prepare solution result
            var lfrres = new LFResult { Buses = buses, Lines = lineRes };

            return new Result<LFResult>
            {
                Data = lfrres,
                IterationStop = res.Iteration,
                Error = res.IsSolution ? ErrorEnum.NoError : ErrorEnum.MaxIteration,
                ErrorMessage = res.IsSolution ? "" : "Maximum iterations reached without convergence."
            };
        }

        internal GSResult Iterate(MC YMatrix, BU buses)
        {
            var Y = YMatrix;

            var bcnt = buses.Count();
            var res = new GSResult
            {
                ABus = new double[bcnt],
                VBus = new double[bcnt],
                QCalc = new double[bcnt],
            };

            foreach (var bus in buses)
            {
                // load bus
                if (bus.BusData.BusType == BusTypeEnum.PQ)
                {
                    var vnxt = LFC.CalcBusVoltage(bus, Y, buses);
                    res.ABus[bus.BusIndex] = vnxt.Phase;
                    res.VBus[bus.BusIndex] = vnxt.Magnitude;
                    UpdateErr(res, bus.BusVoltage, vnxt);
                    // update Vk, Ak
                    bus.BusVoltage = vnxt;

                }

                // voltage controlled bus
                else if (bus.BusData.BusType == BusTypeEnum.PV)
                {
                    // calculate Qk
                    var sk = LFC.CalcBusPower(bus, Y, buses);
                    var (snxt, bt, qgen) = LFC.CalcMaxQk(bus, sk);
                    bus.BusType = bt;
                    // update Sbus
                    bus.Sbus = snxt;
                    bus.Qgen = qgen;
                    bus.BusType = bt;

                    // calculate Vbus
                    var vnxt = LFC.CalcBusVoltage(bus, Y, buses);
                    //bus.UpdateAErr(bus.BusVoltage.Phase, vnxt.Phase, i);

                    if (bt == BusTypeEnum.PV)
                    {
                        // maintain bus controlled voltage and update Ak
                        var vnext = Complex.FromPolarCoordinates(bus.BusData.Voltage, vnxt.Phase);
                        UpdateErr(res, bus.BusVoltage, vnext);
                        bus.BusVoltage = Complex.FromPolarCoordinates(bus.BusData.Voltage, vnxt.Phase);
                    }
                    else
                    {
                        // since Qk is outside limits of Qgen range (min and max)
                        // update Vk, Ak
                        UpdateErr(res, bus.BusVoltage, vnxt);
                        bus.BusVoltage = vnxt;
                    }
                }
            }

            return res;
        }

        #endregion

        internal static BU Initialize(IEnumerable<EEBus> buses) =>
            buses
                .OrderBy(b => b.BusIndex)
                .Select(b => new BusResult
                {
                    BusData = b,
                    BusIndex = b.BusIndex,
                    ID = b.ID,
                    BusType = b.BusType,
                    BusVoltage = new Complex(b.Voltage > 0 ? b.Voltage : 1.0, 0),
                    Sbus = new Complex(b.Pgen - b.Pload, -b.Qload)
                })
                .ToList();

        static void UpdateErr(GSResult res, Complex vcur, Complex vnext)
        {
            var e = Math.Abs((vcur - vnext).Magnitude);
            res.MaxVErr = Math.Max(res.MaxVErr, e);
        }

    }
}
