using System.Collections.Generic;

namespace EEMathLib.LoadFlow
{
    /// <summary>
    /// Power flow result
    /// </summary>
    public class LFResult
    {
        /// <summary>
        /// Bus result
        /// </summary>
        public IEnumerable<BusResult> Buses { get; set; }
        
        /// <summary>
        /// Line result
        /// </summary>
        public IEnumerable<LineResult> Lines { get; set; }
    }
}
