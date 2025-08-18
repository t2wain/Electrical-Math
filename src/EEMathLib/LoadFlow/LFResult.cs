using System.Collections.Generic;

namespace EEMathLib.LoadFlow
{
    public class LFResult
    {
        public IEnumerable<BusResult> Buses { get; set; }
        public IEnumerable<LineResult> Lines { get; set; }
    }
}
