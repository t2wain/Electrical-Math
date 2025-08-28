using EEMathLib.DTO;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Assume all bus voltage is 1.0 
    /// and calculate A usign J1 P/A only.
    /// </summary>
    public class LFDC : LFFastDecoupled
    {
        /// <summary>
        /// Only perform one iteration of calculation
        /// </summary>
        /// <param name="maxIteration">1</param>
        /// <returns></returns>
        public override Result<LFResult> Solve(EENetwork network, double threshold = 0.015, int maxIteration = 20)
        {
            return base.Solve(network, threshold, 1);
        }

        override internal void CalcJMatrix(MC YMatrix, NRResult nrRes)
        {
            // re-use J1
            if (nrRes.J1Matrix == null)
            {
                nrRes.J1Matrix = JCM.CreateJ1(YMatrix, nrRes.NRBuses);
                nrRes.J1LUMatrix = nrRes.J1Matrix.LU();
            }
        }

        override internal void CalcAVDelta(NRResult nrRes)
        {
            var j1Size = nrRes.NRBuses.J1Size;
            var j4Size = nrRes.NRBuses.J4Size;

            var PDelta = nrRes.PQDelta.SubMatrix(0, j1Size.Row, 0, 1);
            var ADelta = nrRes.J1LUMatrix.Solve(PDelta);
            nrRes.ADelta = ADelta.ToColumnMajorArray();

            var AVDelta = MD.Build.Dense(j1Size.Row + j4Size.Row, 1);
            AVDelta.SetSubMatrix(0, 0, ADelta);
            nrRes.AVDelta = AVDelta; // delta A and V
        }

    }
}
