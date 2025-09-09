using EEMathLib.LoadFlow.Data;
using System.Numerics;

namespace EEMathLib.ShortCircuit
{
    public class SCLineResult
    {
        public EELine LineData { get; set; }
        public Complex Current { get; set; }
    }
}
