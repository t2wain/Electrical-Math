using System.Numerics;

namespace EEMathLib.LoadFlow
{
    public enum LineTypeEnum
    {
        Line = 0, // Transmission line
        Transformer = 1, // Transformer
        ShuntCompensator = 2 // Shunt compensator
    }

    /// <summary>
    /// Line input data for load flow analysis.
    /// </summary>
    public class EELine : IEntity
    {
        public int BusIndex { get; set; }
        public string ID { get; set; }
        public int EntityType { get; set; }
        public LineTypeEnum LineType { get; set; }
        public string FromBusID { get; set; }
        public string ToBusID { get; set; }
        public EEBus FromBus { get; set; }
        public EEBus ToBus { get; set; }
        public double R { get; set; }
        public double X { get; set; }
        public double G { get; set; }
        public double B { get; set; }
        public Complex ZImp => new Complex(R, X);
        public Complex YImp => new Complex(G, B);
    }
}
