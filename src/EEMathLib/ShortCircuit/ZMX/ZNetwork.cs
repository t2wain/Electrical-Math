using EEMathLib.ShortCircuit.Data;
using System.Collections.Generic;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit.ZMX
{
    /// <summary>
    /// Contain the Z impedance matrix and its associate 
    /// buses and branches. The bus index to the Z matrix
    /// are from the associated buses.
    /// </summary>
    public class ZNetwork
    {
        public MC Z { get; set; }
        
        /// <summary>
        /// Remember to use the bus index from this to
        /// get the entry of the Z matrix
        /// </summary>
        public IDictionary<string, IZBus> Buses { get; set; }
        
        public IDictionary<string, IEZElement> Elements { get; set; }
        
        /// <summary>
        /// Track the bus index for each new bus
        /// added to the Z matrix.
        /// </summary>
        internal int LastBusIndex { get; set; } = -1;
        
        /// <summary>
        /// Assign the next bus index to the new bus
        /// added to the Z matrix.
        /// </summary>
        internal int GetNextBusIndex() => ++LastBusIndex;
    }
}
