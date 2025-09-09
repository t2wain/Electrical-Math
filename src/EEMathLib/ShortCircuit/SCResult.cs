using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.ShortCircuit
{
    public class SCResult
    {
        public SCBusResult Bus { get; set; }
        public Complex Current { get; set; }
        public IEnumerable<SCBusResult> Buses { get; set; }
        public IEnumerable<SCLineResult> Lines { get; set; }
    }
}
