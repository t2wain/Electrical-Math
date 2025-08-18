namespace EEMathLib.LoadFlow.Data
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

    /// <summary>
    /// Bus input data for load flow analysis.
    /// </summary>
    public class EEBus : IEntity
    {
        public int BusIndex { get; set; }
        public string ID { get; set; }
        public int EntityType { get; set; }
        public BusTypeEnum BusType { get; set; }

        public double Voltage { get; set; }
        public double Angle { get; set; }

        public double Pload { get; set; }
        public double Qload { get; set; }

        public double Pgen { get; set; }
        public double Qgen { get; set; }

        public double Qmin { get; set; }
        public double Qmax { get; set; }

        public double VoltageResult { get; set; }
        public double AngleResult { get; set; }
        public double QgenResult { get; set; }
        public double PResult { get; set; }
        public double QResult { get; set; }
    }
}
