using EEMathLib.LoadFlow.Data;
using System.Numerics;

namespace EEMathLib.LoadFlow
{
    /// <summary>
    /// Represents the results of power flow, including 
    /// both forward and reverse power flow data.
    /// Forward flow is based on from-bus to to-bus.
    /// Reverse flow is based on to-bus to from-bus.
    /// Use the postive values of power and current as the
    /// actual direction of power flow.
    /// </summary>
    public class LineResult
    {
        public EELine LineData { get; set; }

        #region Forward power flow

        /// <summary>
        /// Complex power
        /// </summary>
        public Complex SLine { get; set; }

        /// <summary>
        /// Real power
        /// </summary>
        public double P { get; set; }

        /// <summary>
        /// Reactive power
        /// </summary>
        public double Q { get; set; }

        /// <summary>
        /// Current
        /// </summary>
        public double I { get; set; }

        #endregion

        #region Reverse power flow

        public Complex SLineReverse { get; set; }
        public double PReverse { get; set; }
        public double QReverse { get; set; }
        public double IReverse { get; set; }

        #endregion

    }
}
