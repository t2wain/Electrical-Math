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
        /// <summary>
        /// The calculated Z impdedance matrix
        /// </summary>
        public MC Z { get; set; }
        
        /// <summary>
        /// Provide the bus index associated with the Z matrix.
        /// The bus index is dependent on the order of elements
        /// being added to the Z matrix.
        /// </summary>
        public IDictionary<string, IZBus> Buses { get; set; }
        
        /// <summary>
        /// Elements associated with the building of the Z1 matrix
        /// </summary>
        public IDictionary<string, IEZElement> Elements { get; set; }

        /// <summary>
        /// The last the bus index assigned to the new bus
        /// added to the Z matrix.
        /// </summary>
        internal int LastBusIndex { get; set; } = -1;
        
        /// <summary>
        /// Assign the next bus index to the new bus
        /// added to the Z matrix.
        /// </summary>
        internal int GetNextBusIndex() => ++LastBusIndex;

        internal ZNetwork RefZNetwork { get; set; }
    }
}
