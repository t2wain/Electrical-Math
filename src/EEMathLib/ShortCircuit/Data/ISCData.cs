using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EEMathLib.ShortCircuit.Data
{
    public abstract class NetworkAbstract : IENetwork
    {
        public IEnumerable<IEBus> Buses { get; set; } = Enumerable.Empty<IEBus>();
        public IEnumerable<IELine> Lines { get; set; } = Enumerable.Empty<IELine>();
        public IEnumerable<IETransformer> Transformers { get; set; } = Enumerable.Empty<IETransformer>();
        public IEnumerable<IEGen> Generators { get; set; } = Enumerable.Empty<IEGen>();
        public IEnumerable<IELoad> Loads { get; set; } = Enumerable.Empty<IELoad>();
        public Matrix<Complex> YMatrix { get; set; }

    }

    public class Network1 : NetworkAbstract
    {
        public Network1()
        {
            Buses = new List<IEBus>
            {
                new EBus { ID = "1", BusIndex = 0 },
                new EBus { ID = "2", BusIndex = 1 },
                new EBus { ID = "3", BusIndex = 2 },
                new EBus { ID = "4", BusIndex = 3 },
                new EBus { ID = "5", BusIndex = 4 },
            };

            var dbus = Buses.ToDictionary(b =>  b.ID);

            Lines = new List<IELine>
            {
                new ELine 
                { 
                    ID = "1", 
                    ZSeries = new Complex(0, 0.1), 
                    ToBus = dbus["1"] 
                },
                new ELine 
                { 
                    ID = "2", 
                    ZSeries = new Complex(0, 0.1), 
                    ToBus = dbus["2"] 
                },
                new ELine 
                { 
                    ID = "3", 
                    ZSeries = new Complex(0, 0.2), 
                    FromBus = dbus["2"], 
                    ToBus = dbus["1"] 
                },
                new ELine 
                { 
                    ID = "4", 
                    ZSeries = new Complex(0, 0.3), 
                    FromBus = dbus["2"], 
                    ToBus = dbus["3"] 
                },
                new ELine 
                { 
                    ID = "5", 
                    ZSeries = new Complex(0, 0.15), 
                    FromBus = dbus["3"], 
                    ToBus = dbus["4"] 
                },
                new ELine 
                { 
                    ID = "6", 
                    ZSeries = new Complex(0, 0.25),
                    FromBus = dbus["1"],
                    ToBus = dbus["4"]
                },
                new ELine 
                { 
                    ID = "7", 
                    ZSeries = new Complex(0, 0.4),
                    FromBus = dbus["2"],
                    ToBus = dbus["4"]
                },
            };
        }
    }
}
