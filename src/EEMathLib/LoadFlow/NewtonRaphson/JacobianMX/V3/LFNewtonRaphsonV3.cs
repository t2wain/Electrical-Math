using EEMathLib.LoadFlow.Data;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.V3
{
    /// <summary>
    /// Newton-Raphson load flow algorithm
    /// </summary>
    public class LFNewtonRaphsonV3 : LFNewtonRaphson
    {
        JacobianBase _jc;

        public LFNewtonRaphsonV3()
        {
            _jc = new JacobianV3();
        }

        override internal void CalcJMatrix(MC YMatrix, NRResult nrRes)
        {
            nrRes.JMatrix = _jc.CreateJMatrix(YMatrix, nrRes.NRBuses);
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
                    nrRes.VDelta[b.Vidx] = dV * b.BusVoltage.Magnitude; // save dV calculation
                }
            }
        }

    }
}
