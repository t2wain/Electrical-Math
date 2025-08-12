using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using System.Linq;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using LFC = EEMathLib.LoadFlow.LFCommon;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.LFNewtonRaphson;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Tests for various calculations
    /// in the Newton-Raphson algorithm
    /// </summary>
    public class NRExample
    {
        #region Based on LFData dataset

        public static bool Calc_PQDelta_Partial(LFData data)
        {
            var nw = data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            var bus2 = nrBuses.Buses.FirstOrDefault(b => b.ID == "2");

            var nrRes = LFNR.CalcDeltaPQ(nw.YMatrix, nrBuses);
            var mxPQdelta = nrRes.PQDelta;
            var rowCount = mxPQdelta.RowCount;
            var c = rowCount == nrBuses.JSize.Row;

            var pdk = mxPQdelta[bus2.Pidx, 0];
            var res = -7.99972;
            c = c && Checker.EQ(pdk, res, 0.001);

            return c;
        }

        public static bool Calc_J1_Partial(LFData data)
        {
            var nw = data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = JC.ReIndexBusPQ(buses);

            var J1 = JC.CreateJ1(nw.YMatrix, nrBuses);
            var JRes = data.GetNewtonRaphsonData().JacobianData;
            var res = MX.ParseMatrix(JRes.J1Result);
            var bus2 = nrBuses.Buses.FirstOrDefault(b => b.ID == "2");
            var bus4 = nrBuses.Buses.FirstOrDefault(b => b.ID == "4");
            var j24 = J1[bus2.Pidx, bus4.Aidx];
            //var r24 = -9.91964;
            var d24 = JRes.GetJ1kn(bus2, bus4);
            var c = Checker.EQ(j24, d24, 0.001);
            c = c && J1.RowCount == nrBuses.J1Size.Row 
                && J1.ColumnCount == nrBuses.J1Size.Col;

            return c;
        }

        #endregion

        #region Iteration

        /// <summary>
        /// Validate selected calculation steps
        /// for a number of iteration in Newton-Raphson load flow
        /// </summary>
        public static bool Calc_Iteration(ILFData data, int iteration)
        {
            var nw = data.CreateNetwork();
            // YMatrix data provided (not calculated)
            var YMatrix = MX.ParseMatrix(data.YResult); 

            var c = true;
            var v = false;
            NRResult nrRes;
            if (iteration == 0)
            {
                var nrData = NRResult.Parse(data, 0);
                nrRes = new NRResult
                {
                    NRBuses = nrData.NRBuses
                };
                nrRes = NRValidator.LFIterate(YMatrix, nrRes, 2);
                v = NRValidator.Validate_PQCalc(nrRes, data);
                v = NRValidator.Validate_PQDelta(nrRes.PQDelta, data, iteration);
                c &= v;
            }
            
            if (iteration >= 1)
            {
                var nrData = NRResult.Parse(data, 1);
                nrRes = new NRResult
                {
                    // JMatrix data provided (not calculated)
                    JMatrix = nrData.JMatrix, 
                    NRBuses = nrData.NRBuses
                };
                nrRes = NRValidator.LFIterate(YMatrix, nrRes, 7);
                nrRes.Iteration = 1;
                v = NRValidator.Validate_AVDelta(nrRes, data);
                c &= v;

                if (c)
                {
                    v = NRValidator.Validate_AVBus(nrRes, data);
                    c &= v;
                }
                if (c)
                {
                    nrRes.PQDelta = null;
                    nrRes = NRValidator.LFIterate(YMatrix, nrRes, 2);
                    v = NRValidator.Validate_PQCalc(nrRes, data);
                    c &= v;
                }
                if (c)
                {
                    v = NRValidator.Validate_PQDelta(nrRes.PQDelta, data, iteration);
                    c &= v;
                }
            }

            if (iteration >= 2)
            {
                var nrData = NRResult.Parse(data, 2);
                nrRes = new NRResult
                {
                    // JMatrix data provided (not calculated)
                    JMatrix = nrData.JMatrix,
                    NRBuses = nrData.NRBuses
                };
                nrRes = NRValidator.LFIterate(YMatrix, nrRes, 7);
                nrRes.Iteration = 2;
                v = NRValidator.Validate_AVDelta(nrRes, data);
                c &= v;

                if (c)
                {
                    v = NRValidator.Validate_AVBus(nrRes, data);
                    c &= v;
                }
                if (c)
                {
                    nrRes.PQDelta = null;
                    nrRes = NRValidator.LFIterate(YMatrix, nrRes, 2);
                    v = NRValidator.Validate_PQCalc(nrRes, data);
                    c &= v;
                }
                if (c)
                {
                    v = NRValidator.Validate_PQDelta(nrRes.PQDelta, data, iteration);
                    c &= v;
                }
            }

            return c;
        }

        #endregion

        #region Solve

        public static bool LFSolve(ILFData data)
        {

            var nw = data.CreateNetwork();
            var threshold = 0.001;

            var res = LFNR.Solve(nw, threshold, 50);

            if (res.Error == ErrorEnum.Divergence)
                return false;


            var rbuses = LFC.CalcResult(res.Data).ToDictionary(bus => bus.ID);
            var dbuses = data.LFResult.ToDictionary(bus => bus.ID);

            var c = true;
            foreach (var dbus in dbuses.Values)
            {
                var rb = rbuses[dbus.ID];
                c = c && Checker.EQPct(rb.Voltage, dbus.Voltage, threshold);
                c = c && Checker.EQPct(rb.Angle, dbus.Angle, threshold);

                if (rb.BusType == BusTypeEnum.PQ
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    c = c && Checker.EQPct(rb.Pgen, dbus.Pgen, threshold);
                    c = c && Checker.EQPct(rb.Qgen, dbus.Qgen, threshold);
                }
            }

            return c;
        }

        #endregion
    }
}
