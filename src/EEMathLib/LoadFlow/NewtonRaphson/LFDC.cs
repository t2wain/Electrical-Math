using EEMathLib.DTO;
using System.Linq;
using System.Numerics;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// DC-like version of Newton-Raphson load flow algorithm
    /// </summary>
    public class LFDC : NewtonRaphsonBase
    {
        override protected Result<BU> Solve(BU buses, MC YMatrix,
            double threshold = double.NaN, int maxIteration = 1)
        {
            var res = new NRResult();
            var Y = YMatrix;

            // Step 1
            // Determine classification of each bus
            res.NRBuses = JC.ReIndexBusPQ(buses);

            // Step 2
            // Calculate J1 matrix
            var J1Matrix = JC.CreateJ1(YMatrix, res.NRBuses);

            // Step 3
            // Prepare the matrix of injected power at each bus
            var busesP = res.NRBuses.Buses.Select(b => b.Sbus.Real).ToArray();
            var mxP = MD.Build.Dense(busesP.Length, 1, busesP);

            // Step 4
            // Solve for the voltage phase of each bus
            var mxA = J1Matrix.Solve(mxP);

            // Step 5
            // Update bus voltage with calculated phase
            foreach(var b in res.NRBuses.Buses)
            {
                var phase = mxA[b.Pidx, 0];
                b.BusVoltage = Complex.FromPolarCoordinates(1.0, phase);
            }

            //return res.NRBuses.AllBuses;
            return new Result<BU>
            {
                Data = buses,
                IterationStop = 1,
                Error = ErrorEnum.NoError,
            };
        }
    }
}
