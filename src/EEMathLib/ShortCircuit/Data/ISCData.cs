using EEMathLib.DTO;
using EEMathLib.ShortCircuit.ZMX;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EEMathLib.ShortCircuit.Data
{
    public class ZNetwork1 : ZNetwork
    {
        public ZNetwork1()
        {
            Buses = new Dictionary<string, IZBus>
            {
                { "1", new ZBus { ID = "1" } },
                { "2", new ZBus { ID = "2" } },
                { "3", new ZBus { ID = "3" } },
                { "4", new ZBus { ID = "4" } },
            };

            Elements = new Dictionary<string, IEZElement>
            {
                { "1", new EZElement { ID = "1", ToBus = Buses["1"], Z = new Complex(0, 0.1) } },
                { "2", new EZElement { ID = "2", ToBus = Buses["2"], Z = new Complex(0, 0.1) } },
                { "3", new EZElement { ID = "3", FromBus = Buses["1"], ToBus = Buses["2"], Z = new Complex(0, 0.2) } },
                { "4", new EZElement { ID = "4", FromBus = Buses["2"], ToBus = Buses["3"], Z = new Complex(0, 0.3) } },
                { "5", new EZElement { ID = "5", FromBus = Buses["3"], ToBus = Buses["4"], Z = new Complex(0, 0.15) } },
                { "6", new EZElement { ID = "6", FromBus = Buses["1"], ToBus = Buses["4"], Z = new Complex(0, 0.25) } },
                { "7", new EZElement { ID = "7", FromBus = Buses["2"], ToBus = Buses["4"], Z = new Complex(0, 0.4) } },
            };

        }
    }
}
