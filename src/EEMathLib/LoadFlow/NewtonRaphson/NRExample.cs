using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.GaussSeidel;
using EEMathLib.LoadFlow.NewtonRaphson.JacobianMX;
using EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.V2;
using EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.V3;
using EEMathLib.MatrixMath;
using System.Linq;
using JC = EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.Jacobian;
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

            var jc = new Jacobian();
            var J1 = jc.CreateJ1(nw.YMatrix, nrBuses);
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
                nrRes = NRValidator.LFIterate(YMatrix, nrRes, 5);
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

        public static bool LFSolve(ILFData data, int algoVer, bool validate = false, int maxIteration = 50)
        {

            var nw = data.CreateNetwork();
            var threshold = 0.001;

            ILFSolver solver;
            switch (algoVer)
            {
                case 2:
                    solver = new LFNewtonRaphsonV2();
                    break;
                case 3:
                    solver = new LFNewtonRaphsonV3();
                    break;
                default:
                    solver = new LFNR();
                    break;
            }

            var res = solver.Solve(nw, threshold, maxIteration);

            if (res.IsError)
                return false;

            if (!validate)
                return true;

            var c = LFC.ValidateLFResult(nw, res.Data, 0.05);
            return c;
        }


        public static bool LFSolve_InitCond(ILFData data, bool validate = false, int maxIteration = 50)
        {

            var nw = data.CreateNetwork();
            var threshold = 0.001;

            // Run GS with fix iteration
            // to set the initial bus condition
            var gsSolver = new LFGaussSeidel();
            var res = gsSolver.Solve(nw, threshold, 50);
            
            if (res.Error == DTO.ErrorEnum.Divergence)
                return false;

            // Run NR using bus intial condition for GS
            var nrSolver = new LFNR();
            res = nrSolver.Solve(nw, res.Data.Buses, threshold, maxIteration);

            if (res.IsError)
                return false;

            if (!validate)
                return true;

            var c = LFC.ValidateLFResult(nw, res.Data, 0.05);
            return c;
        }

        #endregion

        #region Approximate

        public static bool LFSolve_Decoupled(ILFData data)
        {

            var nw = data.CreateNetwork();
            var threshold = 0.001;

            var solver = new LFDecoupled();
            var res = solver.Solve(nw, threshold, 50);

            return !res.IsError;
        }

        public static bool LFSolve_FastDecoupled(ILFData data)
        {

            var nw = data.CreateNetwork();
            var threshold = 0.001;

            var solver = new LFFastDecoupled();
            var res = solver.Solve(nw, threshold, 50);

            return !res.IsError;
        }

        /// <summary>
        /// Only calculate 1 iteration to get an estimated solution
        /// </summary>
        public static bool LFSolve_FastDecoupled_Approximation(ILFData data)
        {

            var nw = data.CreateNetwork();
            var threshold = 0.1;

            var solver = new LFFastDecoupled();
            var res = solver.Solve(nw, threshold, 1);

            return true;
        }

        /// <summary>
        /// Only calculate 1 iteration to get an estimated solution
        /// </summary>
        public static bool LFSolve_DCLike_Approximation(ILFData data)
        {

            var nw = data.CreateNetwork();
            var solver = new LFDC();
            var res = solver.Solve(nw);
            return true;
        }


        #endregion
    }
}
