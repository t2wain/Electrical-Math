using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LFC = EEMathLib.LoadFlow.LFCommon;

namespace EEMathLib.LoadFlow.GaussSiedel
{
    public static class LFGaussSiedel
    {
        public static Result<IEnumerable<BusResult>> Solve(EENetwork network,
            double threshold = 0.015, int maxIteration = 100, int minIteration = 50) =>
            Solve(Initialize(network.Buses), network.YMatrix, threshold, maxIteration, minIteration);

        public static Result<IEnumerable<BusResult>> Solve(IEnumerable<BusResult> buses, Matrix<Complex> YMatrix, 
            double threshold = 0.015, int maxIteration = 100, int minIteration = 50)
        {
            var Y = YMatrix;

            #region Iteration

            BusResult slackBus = null;
            var i = 0;
            var isFound = false;
            while (i++ < maxIteration)
            {
                // calculate Vk, Ak, Qk for each bus
                foreach (var bus in buses)
                {
                    #region Calulate Bus

                    // load bus
                    if (bus.BusData.BusType == BusTypeEnum.PQ)
                    {
                        var vnxt = LFC.CalcVoltage(bus, Y, buses);
                        bus.UpdateVErr(bus.BusVoltage.Magnitude, vnxt.Magnitude, i);
                        bus.UpdateAErr(bus.BusVoltage.Phase, vnxt.Phase, i);
                        // update Vk, Ak
                        bus.BusVoltage = vnxt;
                    }

                    // voltage controlled bus
                    else if (bus.BusData.BusType == BusTypeEnum.PV)
                    {
                        // calculate Qk
                        var sk = LFC.CalcPower(bus, Y, buses);
                        bus.UpdatePErr(bus.Sbus.Real, sk.Real, i);

                        var (snxt, bt) = LFC.CalcMaxQk(bus, sk);
                        bus.UpdateQErr(bus.Sbus.Imaginary, snxt.Imaginary, i, bt != bus.BusType);
                        bus.BusType = bt;
                        // update Sbus
                        bus.Sbus = snxt;

                        // calculate Vbus
                        var vnxt = LFC.CalcVoltage(bus, Y, buses);
                        bus.UpdateAErr(bus.BusVoltage.Phase, vnxt.Phase, i);

                        if (bt == BusTypeEnum.PV)
                        {
                            // maintain bus controlled voltage and update Ak
                            bus.BusVoltage = Complex.FromPolarCoordinates(bus.BusData.Voltage, vnxt.Phase);
                        }
                        else
                        {
                            // required switching to PQ bus
                            // since Qk is outside limits of Qgen range (min and max)
                            bus.BusType = BusTypeEnum.PQ;
                            bus.UpdateVErr(bus.BusVoltage.Magnitude, vnxt.Magnitude, i);
                            // update Vk, Ak
                            bus.BusVoltage = vnxt;
                        }
                    }

                    // save slack bus for later calculation
                    else if (bus.BusData.BusType == BusTypeEnum.Slack)
                    {
                        slackBus = bus;
                    }

                    #endregion
                }

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
                    return new Result<IEnumerable<BusResult>>
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
            slackBus.Sbus = LFC.CalcPower(slackBus, Y, buses);

            return new Result<IEnumerable<BusResult>>
            {
                Data = buses,
                IterationStop = i,
                Error = isFound ? ErrorEnum.NoError : ErrorEnum.MaxIteration,
                ErrorMessage = isFound ? "" : "Maximum iterations reached without convergence."
            };
        }

        #region Calculation

        public static IEnumerable<BusResult> Initialize(IEnumerable<EEBus> buses) =>
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

        #endregion

        #region Track error and convergence

        static bool IsSolutionFound(IEnumerable<BusResult> buses, double threshold)
        {
            var cv = true;
            foreach (var bus in buses.Where(b => b.BusType != BusTypeEnum.Slack))
            {
                cv = cv && Math.Abs(bus.BusVoltage.Magnitude) > 0
                    && Math.Abs(bus.Err.VErr / bus.BusVoltage.Magnitude) < threshold;
                cv = cv && Math.Abs(bus.BusVoltage.Phase) > 0
                    && Math.Abs(bus.Err.AErr / bus.BusVoltage.Phase) < 0.1 * threshold;
                if (bus.BusData.BusType == BusTypeEnum.PV)
                {
                    cv = cv && Math.Abs(bus.Sbus.Imaginary) > 0
                        && Math.Abs(bus.Err.QErr / bus.Sbus.Imaginary) < threshold;
                }
            }
            return cv;
        }

        static bool IsDiverged(IEnumerable<BusResult> buses, int iteration)
        {
            if (iteration % 5 != 0)
                return false;

            var cv = true;
            foreach (var bus in buses.Where(b => b.BusType != BusTypeEnum.Slack))
            {
                cv = cv && (bus.Err.VErr < 0.1 || bus.ErrRef.VErr < 0.1 || bus.Err.VErr < bus.ErrRef.VErr * 3);
                cv = cv && (bus.Err.AErr < 0.1 || bus.ErrRef.AErr < 0.1 || bus.Err.AErr < bus.ErrRef.AErr * 3);
                if (bus.BusData.BusType == BusTypeEnum.PV)
                    cv = cv && (bus.Err.QErr < 0.1 || bus.ErrRef.QErr < 0.1 || bus.Err.QErr < bus.ErrRef.QErr * 3);
            }
            return !cv;
        }

        #endregion
    }
}
