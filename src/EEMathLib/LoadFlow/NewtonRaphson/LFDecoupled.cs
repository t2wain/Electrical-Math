using EEMathLib.LoadFlow.NewtonRaphson.JacobianMX;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;


namespace EEMathLib.LoadFlow.NewtonRaphson
{ 
    
    /// <summary>
    /// Decoupled load flow algorithm is based on the assumption that
    /// real-power is primary dependent on bus phase angle (Jacobian J1 P/A), and
    /// reactive-ppower is primary dependent on bus voltage (Jacobian J4 Q/V).
    /// </summary>
    public class LFDecoupled : NewtonRaphsonBase
    {
        public LFDecoupled() : base(new Jacobian()) { }

        
        /// <summary>
        /// Calculate on J1 and J4 Jacobian matrix
        /// </summary>
        override internal void CalcJMatrix(MC YMatrix, NRResult nrRes)
        {
            nrRes.J1Matrix = JCM.CreateJ1(YMatrix, nrRes.NRBuses);
            nrRes.J4Matrix = JCM.CreateJ4(YMatrix, nrRes.NRBuses);
        }

        /// <summary>
        /// Calculate AV delta using on J1 and J4 Jacobian matrix
        /// </summary>
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

            var AVDelta = MD.Build.Dense(j1Size.Row + j4Size.Row, 1);
            AVDelta.SetSubMatrix(0, 0, ADelta);
            AVDelta.SetSubMatrix(j1Size.Row, 0, VDelta);
            nrRes.AVDelta = AVDelta; // delta A and V
        }

    }
}
