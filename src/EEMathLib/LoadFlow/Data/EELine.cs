using System.Numerics;

namespace EEMathLib.LoadFlow.Data
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
        /// <summary>
        /// Line series resistance
        /// </summary>
        public double RSeries { get; set; }
        /// <summary>
        /// Line series reactance
        /// </summary>
        public double XSeries { get; set; }
        /// <summary>
        /// Line shunt conductance
        /// </summary>
        public double GShunt { get; set; }
        /// <summary>
        /// Line shunt susceptance
        /// </summary>
        public double BShunt { get; set; }
        public int TxTap { get; set; }
        /// <summary>
        /// Line series Susceptance
        /// </summary>
        public Complex ZImpSeries => new Complex(RSeries, XSeries);
        /// <summary>
        /// Line shunt admitance
        /// </summary>
        public Complex YImpShunt => new Complex(GShunt, BShunt);

        public double? PResult { get; set; }
        public double? QResult { get; set; }

        public double? PResultReverse { get; set; }
        public double? QResultReverse { get; set; }

        public double? IResult { get; set; }
        public double? IResultReverse { get; set; }
    }
}
