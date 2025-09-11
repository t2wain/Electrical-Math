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
        bool Visited { get; set; }
    }

    public class ZBus : IZBus
    {
        public string ID { get; set; }
        public IEBus Data { get; set; }
        public int BusIndex { get; set; } = -1;
        public bool Visited { get; set; }
    }

    public interface IEZElement
    {
        string ID { get; }
        IEntity Data { get; }
        IZBus FromBus { get; }
        IZBus ToBus { get; }
        Complex Z { get; }
        int Sequence { get; set; }
        int AddCase { get; set; }
    }

    public class EZElement : IEZElement
    {
        public string ID { get; set; }  
        public IEntity Data { get; set; }
        public IZBus FromBus { get; set; }
        public IZBus ToBus { get; set; }
        public Complex Z { get; set; }
        public int Sequence { get; set; } = -1;
        public int AddCase { get; set; }

    }

    public class Branch
    {
        public Branch(IEZElement el, IZBus toBus)
        {
            this.Element = el;
            this.ToBus = toBus;
        }
        public IEZElement Element { get; set; }
        public IZBus ToBus { get; set; }
    }

}
