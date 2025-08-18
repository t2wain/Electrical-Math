using EEMathLib.LoadFlow.Data;
using System;
using System.Numerics;

namespace EEMathLib.LoadFlow
{
    /// <summary>
    /// Track result of load flow calculation
    /// at each iteration
    /// </summary>
    public class BusResult
    {
        /// <summary>
        /// Original data of each bus
        /// </summary>
        public EEBus BusData { get; set; }

        /// <summary>
        /// For use as zero-based index 
        /// entry of various matrices
        /// </summary>
        public int BusIndex { get; set; }

        public string ID { get; set; }

        /// <summary>
        /// PV buses can be switched to PQ
        /// or back to PV at each load flow iteration
        /// </summary>
        public BusTypeEnum BusType { get; set; }

        /// <summary>
        /// Voltage result of each bus
        /// </summary>
        public Complex BusVoltage { get; set; }

        /// <summary>
        /// Transmitted (injected) power of each bus
        /// </summary>
        public Complex Sbus { get; set; }

        #region Newton-Raphson bus indices

        /// <summary>
        /// For use as zero-based index entry of
        /// Jacobian J1 (col), J3 (row) matrices
        /// at each iteration
        /// </summary>
        public int Aidx { get; set; }

        /// <summary>
        /// For use as zero-based index entry of
        /// Jacobian J2 (col), J4 (col) matrices
        /// at each iteration
        /// </summary>
        public int Vidx { get; set; }

        /// <summary>
        /// For use as zero-based index entry of
        /// Jacobian J1 (row), J2 (row) matrices
        /// at each iteration
        /// </summary>
        public int Pidx { get; set; }

        /// <summary>
        /// For use as zero-based index entry of
        /// Jacobian J3 (row), J4 (row) matrices
        /// at each iteration
        /// </summary>
        public int Qidx { get; set; }

        #endregion
    }
}
