namespace EEMathLib.LoadFlow.GaussSeidel
{
    public class GSResult
    {
        public int Iteration { get; set; }
        public double[] VBus { get; set; }
        public double[] ABus { get; set; }
        public double[] QCalc { get; set; }
        public double MaxVErr { get; set; }
        public bool IsSolution { get; set; }
    }
}
