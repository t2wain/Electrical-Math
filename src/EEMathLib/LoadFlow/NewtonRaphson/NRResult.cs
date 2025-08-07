using MathNet.Numerics.LinearAlgebra;

namespace EEMathLib.LoadFlow.NewtonRaphson
{

    /// <summary>
    /// Store all the calculated result 
    /// per Newton-Raphson iteration
    /// </summary>
    public class NRResult
    {
        public int Iteration { get; set; }
        public Jacobian.NRBuses NRBuses { get; set; }
        public Matrix<double> JMatrix { get; set; }
        public Matrix<double> PQDelta { get; set; }
        public Matrix<double> AVDelta { get; set; }
        public double[] ADelta { get; set; }
        public double[] VDelta { get; set; }
        public double[] PCal { get; set; }
        public double[] QCal { get; set; }
        public double MaxErr { get; set; }
        public bool IsSolution { get; set; }
    }
}
