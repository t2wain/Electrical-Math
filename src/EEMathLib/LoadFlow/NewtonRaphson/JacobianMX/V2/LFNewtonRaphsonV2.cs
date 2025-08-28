using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.V2
{
    /// <summary>
    /// Newton-Raphson load flow algorithm
    /// </summary>
    public class LFNewtonRaphsonV2 : LFNewtonRaphson
    {
        JacobianBase _jc;

        public LFNewtonRaphsonV2()
        {
            _jc = new JacobianV2();
        }

        override internal void CalcJMatrix(MC YMatrix, NRResult nrRes)
        {
            nrRes.JMatrix = _jc.CreateJMatrix(YMatrix, nrRes.NRBuses);
        }

    }
}
