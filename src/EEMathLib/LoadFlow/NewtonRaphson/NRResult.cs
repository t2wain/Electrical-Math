using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using MD = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using LFNR = EEMathLib.LoadFlow.NewtonRaphson.LFNewtonRaphson;
using JC = EEMathLib.LoadFlow.NewtonRaphson.JacobianMX.Jacobian;
using MathNet.Numerics.LinearAlgebra.Factorization;
using EEMathLib.LoadFlow.NewtonRaphson.JacobianMX;

namespace EEMathLib.LoadFlow.NewtonRaphson
{

    /// <summary>
    /// Store all the calculated result 
    /// per Newton-Raphson iteration
    /// </summary>
    internal class NRResult
    {
        public int Iteration { get; set; }
        public JC.NRBuses NRBuses { get; set; }
        public MD JMatrix { get; set; }
        public MD J1Matrix { get; set; }
        public LU<double> J1LUMatrix { get; set; }
        public MD J4Matrix { get; set; }
        public LU<double> J4LUMatrix { get; set; }
        public MD PQDelta { get; set; }
        public MD AVDelta { get; set; }
        public double MaxErr { get; set; }
        public bool IsSolution { get; set; }
        public bool PVBusStatusChanged { get; set; }

        #region Output for test validation

        public double[] ADelta { get; set; }
        public double[] VDelta { get; set; }
        public double[] VBus { get; set; }
        public double[] ABus { get; set; }
        public double[] PCal { get; set; }
        public double[] QCal { get; set; }

        #endregion

        public void ClearResult()
        {
            JMatrix = null;
            J1Matrix = null;
            J1LUMatrix = null;
            J4Matrix = null;
            J4LUMatrix = null;
            PQDelta = null;
            AVDelta = null;
        }

        /// <summary>
        /// Parse input data of the load flow result
        /// of a selected iteration for validation during testing.
        /// </summary>
        public static NRResult Parse(ILFData data, int iteration)
        {
            var nrData = data.GetNewtonRaphsonData(iteration);
            var jc = new Jacobian();

            MD J = null;
            if (nrData.JacobianData is JacobianData jcData) { 
                var J1 = MX.ParseMatrix(jcData.J1Result);
                var J2 = MX.ParseMatrix(jcData.J2Result);
                var J3 = MX.ParseMatrix(jcData.J3Result);
                var J4 = MX.ParseMatrix(jcData.J4Result);
                J = JacobianBase.CreateJMatrix(J1, J2, J3, J4);
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
