using System.Numerics;

namespace EEMathLib.LoadFlow
{
    public enum LineTypeEnum
    {
        Line = 0, // Transmission line
        Transformer = 1, // Transformer
        ShuntCompensator = 2 // Shunt compensator
    }

    public class EELine : IEntity
    {
        public int BusIndex { get; set; }
        public string ID { get; set; }
        public int EntityType { get; set; }
        public LineTypeEnum LineType { get; set; }
        public IEntity FromBus { get; set; }
        public IEntity ToBus { get; set; }
        public Complex ZImp { get; set; }
        public Complex YImp { get; set; }
    }
}
