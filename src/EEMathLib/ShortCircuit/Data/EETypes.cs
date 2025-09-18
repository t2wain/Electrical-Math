using EEMathLib.DTO;
using System.Numerics;

namespace EEMathLib.ShortCircuit.Data
{
    public enum ZTypeEnum
    {
        Subtransient,
        Transient,
        Synchronous
    }

    /// <summary>
    /// Bus data and result for building Z matrix.
    /// </summary>
    public interface IZBus
    {
        string ID { get; }
        IEBus Data { get; }
        int BusIndex { get; set; }
        bool Visited { get; set; }
    }

    /// <summary>
    /// Bus data and result for building Z matrix.
    /// </summary>
    public class ZBus : IZBus
    {
        public string ID { get; set; }
        public IEBus Data { get; set; }

        /// <summary>
        /// As new node is added to the Z matrix,
        /// an incremental index is assigned to the
        /// bus to correspond to the entry of the
        /// Z matrix
        /// </summary>
        public int BusIndex { get; set; } = -1;

        /// <summary>
        /// Related to walking-the-graph
        /// algorithm when building the 
        /// Z matrix.
        /// </summary>
        public bool Visited { get; set; }
    }

    /// <summary>
    /// Impedance data for building Z matrix
    /// </summary>
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

    /// <summary>
    /// Impedance data for building Z matrix
    /// </summary>
    public class EZElement : IEZElement
    {
        public string ID { get; set; }  
        public IEntity Data { get; set; }
        public IZBus FromBus { get; set; }
        public IZBus ToBus { get; set; }
        public Complex Z { get; set; }

        /// <summary>
        /// The sequence index that the
        /// element is added to the Z matrix
        /// </summary>
        public int Sequence { get; set; } = -1;

        /// <summary>
        /// Identify which algorithm used
        /// to add the element to the Z
        /// matrix
        /// </summary>
        public int AddCase { get; set; }

    }

}
