using EEMathLib.DTO;
using System.Numerics;

namespace EEMathLib.ShortCircuit.Data
{
    public enum ZTypeEnum
    {
        Subtransient,
        Transient,
        SteadyState
    }

    public interface IZBus
    {
        string ID { get; }
        IEBus Data { get; }
        int BusIndex { get; set; }
    }

    public class ZBus : IZBus
    {
        public string ID { get; set; }
        public IEBus Data { get; set; }
        public int BusIndex { get; set; } = -1;
    }

    public interface IEZElement
    {
        string ID { get; }
        IEntity Data { get; }
        IZBus FromBus { get; }
        IZBus ToBus { get; }
        Complex Z { get; }
    }

    public class EZElement : IEZElement
    {
        public string ID { get; set; }  
        public IEntity Data { get; set; }
        public IZBus FromBus { get; set; }
        public IZBus ToBus { get; set; }
        public Complex Z { get; set; }
    }

}
