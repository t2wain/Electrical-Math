using System.Numerics;

namespace EEMathLib.LoadFlow
{
    public enum BusTypeEnum
    {
        Slack = 0, // Reference bus
        PQ = 1, // Load bus
        PV = 2, // Generator bus
        Isolated = 3 // Isolated bus
    }

    public interface IEntity
    {
        int BusIndex { get; set; }
        string ID { get; set; }
        int EntityType { get; }
    }

    public class EEBus : IEntity
    {
        public int BusIndex { get; set; }
        public string ID { get; set; }
        public int EntityType { get; set; }
        public BusTypeEnum BusType { get; set; }

        public double Voltage { get; set; }
        public double Angle { get; set; } // degree
        public Complex BusVoltage => new Phasor(Voltage, Angle).ToComplex();

        public double Pbus { get; set; }
        public double Qbus { get; set; }
        public Complex Sbus => new Complex(Pbus, Qbus);

        public double Pload { get; set; }
        public double Qload { get; set; }
        public Complex Sload => new Complex(Pload, (BusType == BusTypeEnum.PQ ? -1 : 1) * 0);


        public double Pgen { get; set; }
        public double Qgen { get; set; }
        public Complex Sgen => new Complex(Pgen, Qgen);

        public double Qmin { get; set; }
        public double Qmax { get; set; }
    }
}
