using EEMathLib.ShortCircuit.Data;
using System.Numerics;

namespace EEMathLib.ShortCircuit
{
    public class SCBusResult
    {
        public IZBus BusData { get; set; }
        public Complex Voltage { get; set; }
    }
}
