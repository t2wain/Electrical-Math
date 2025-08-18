using JCFD = EEMathLib.LoadFlow.NewtonRaphson.JacobianFD;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Fast-Decoupled load flow algorithm is based on the assumption that,
    /// under steady-state operation, all bus voltages are approximately 1.0
    /// </summary>
    public class LFFastDecoupled : NewtonRaphsonBase
    {
        /// <summary>
        /// Carry over the J1 and J4 Jacobian matrices for used in 
        /// the next iteration. However, if any PV bus changed state 
        /// in the previous iteration, J4 will be re-calculated.
        /// </summary>
        /// <param name="curRes"></param>
        /// <returns></returns>
        internal override NRResult InitResult(NRResult curRes)
        {
            var nrRes = new NRResult
            {
                Iteration = curRes.Iteration,
                J1Matrix = curRes.J1Matrix,
                J1LUMatrix = curRes.J1LUMatrix,
                J4Matrix = curRes.J4Matrix,
                J4LUMatrix = curRes.J4LUMatrix,
            };
            if (curRes.PVBusStatusChanged)
            {
                nrRes.J4Matrix = null;
                nrRes.J4LUMatrix = null;
            };
            return nrRes;
        }


        override internal void CalcJMatrix(MC YMatrix, NRResult nrRes)
        {
            if (nrRes.J1Matrix == null)
            {
                nrRes.J1Matrix = JCFD.CreateJ1(YMatrix, nrRes.NRBuses);
                nrRes.J1LUMatrix = nrRes.J1Matrix.LU();
            }
            if (nrRes.J4Matrix == null)
            {
                nrRes.J4Matrix = JCFD.CreateJ4(YMatrix, nrRes.NRBuses);
                nrRes.J4LUMatrix = nrRes.J4Matrix.LU();
            }
        }

        override internal void CalcAVDelta(NRResult nrRes)
        {
            var j1Size = nrRes.NRBuses.J1Size;
            var j4Size = nrRes.NRBuses.J4Size;

            var PDelta = nrRes.PQDelta.SubMatrix(0, j1Size.Row, 0, 1);
            var ADelta = nrRes.J1LUMatrix.Solve(PDelta);
            nrRes.ADelta = ADelta.ToColumnMajorArray();

            var QDelta = nrRes.PQDelta.SubMatrix(j1Size.Row, j4Size.Row, 0, 1);
            var VDelta = nrRes.J4LUMatrix.Solve(QDelta);
            nrRes.VDelta = VDelta.ToColumnMajorArray();

            var AVDelta = MD.Build.Dense(j1Size.Row + j4Size.Row, 1);
            AVDelta.SetSubMatrix(0, 0, ADelta);
            AVDelta.SetSubMatrix(j1Size.Row, 0, VDelta);
            nrRes.AVDelta = AVDelta; // delta A and V
        }
    }
}
