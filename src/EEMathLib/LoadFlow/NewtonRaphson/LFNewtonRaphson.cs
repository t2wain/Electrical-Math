using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.NewtonRaphson.JacobianMX;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.NewtonRaphson
{
    /// <summary>
    /// Newton-Raphson load flow algorithm
    /// </summary>
    public class LFNewtonRaphson : NewtonRaphsonBase
    {
        public LFNewtonRaphson() : this(new Jacobian()) { }
        public LFNewtonRaphson(JacobianBase jcm) : base(jcm) { }

        override internal void CalcJMatrix(MC YMatrix, NRResult nrRes)
        {
            nrRes.JMatrix = JCM.CreateJMatrix(YMatrix, nrRes.NRBuses);
        }

        override internal void CalcAVDelta(NRResult nrRes)
        {
            nrRes.AVDelta = nrRes.JMatrix.Solve(nrRes.PQDelta); // delta A and V
            var acnt = nrRes.NRBuses.J1Size.Row;
            nrRes.ADelta = new double[acnt];
            nrRes.VDelta = new double[nrRes.NRBuses.J3Size.Row];
            foreach (var b in nrRes.NRBuses.Buses)
            {
                var ik = b.Pidx;
                var dA = nrRes.AVDelta[b.Aidx, 0];
                nrRes.ADelta[b.Aidx] = dA; // save dA calculation

                if (b.BusType == BusTypeEnum.PQ)
                {
                    var dV = nrRes.AVDelta[b.Vidx + acnt, 0];
                    nrRes.VDelta[b.Vidx] = dV; // save dV calculation
                }
            }
        }

    }
}
