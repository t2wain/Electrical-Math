using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.LFNewtonRaphson;
using JC = EEMathLib.LoadFlow.NewtonRaphson.Jacobian;

namespace EEMathLib.LoadFlow.NewtonRaphson
{

    /// <summary>
    /// Store all the calculated result 
    /// per Newton-Raphson iteration
    /// </summary>
    public class NRResult
    {
        public int Iteration { get; set; }
        public JC.NRBuses NRBuses { get; set; }
        public MD JMatrix { get; set; }
        public MD PQDelta { get; set; }
        public MD AVDelta { get; set; }
        public double[] ADelta { get; set; }
        public double[] VDelta { get; set; }
        public double[] VBus { get; set; }
        public double[] ABus { get; set; }
        public double[] PCal { get; set; }
        public double[] QCal { get; set; }
        public double MaxErr { get; set; }
        public bool IsSolution { get; set; }

        public void ClearResult()
        {
            NRBuses = new JC.NRBuses { AllBuses = NRBuses.AllBuses };
            JMatrix = null;
            PQDelta = null;
            AVDelta = null;
            ADelta = null;
            VDelta = null;
            VBus = null;
            ABus = null;
            PCal = null;
            QCal = null;
            MaxErr = 0;
            IsSolution = false;
        }

        public static NRResult Parse(ILFData data, int iteration)
        {
            var nrData = data.GetNewtonRaphsonData(iteration);

            MD J = null;
            if (nrData.JacobianData is JacobianData jcData) { 
                var J1 = MX.ParseMatrix(jcData.J1Result);
                var J2 = MX.ParseMatrix(jcData.J2Result);
                var J3 = MX.ParseMatrix(jcData.J3Result);
                var J4 = MX.ParseMatrix(jcData.J4Result);
                J = JC.CreateJMatrix(J1, J2, J3, J4);
            }

            var buses = LFNR.Initialize(data.Busses);

            return new NRResult
            {
                NRBuses = JC.ReIndexBusPQ(buses),
                JMatrix = J,
                PCal = nrData.PCal,
                QCal = nrData.QCal,
                ADelta = nrData.ADelta,
                VDelta = nrData.VDelta,
            };
        }
    }
}
