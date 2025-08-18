using EEMathLib.LoadFlow.Data;
using System.Numerics;

namespace EEMathLib.LoadFlow
{
    public class LineResult
    {
        public EELine LineData { get; set; }
        public Complex SLine { get; set; }
        public double P { get; set; }
        public double Q { get; set; }
    }
}
