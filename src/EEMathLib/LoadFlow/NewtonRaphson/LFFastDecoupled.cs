using MathNet.Numerics.LinearAlgebra;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Fast-Decoupled load flow algorithm
    /// </summary>
    public class LFFastDecoupled : NewtonRaphsonBase
    {
        public LFFastDecoupled() : this(false) { }

        public LFFastDecoupled(bool calcJMatrixOnce)
        {
            CalcJMatrixOnce = calcJMatrixOnce;
        }

        public bool CalcJMatrixOnce { get; protected set; }

        override internal void CalcJMatrix(MC YMatrix, NRResult nrRes)
        {
            if (CalcJMatrixOnce && nrRes.J1Matrix != null)
                return;
            nrRes.J1Matrix = JC.CreateJ1(YMatrix, nrRes.NRBuses);
            nrRes.J4Matrix = JC.CreateJ4(YMatrix, nrRes.NRBuses);
        }

        override internal void CalcAVDelta(NRResult nrRes)
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
