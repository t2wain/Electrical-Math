using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.LoadFlow.Data
{
    /// <summary>
    /// Load flow dataset
    /// </summary>
    public interface ILFData : IDisposable
    {
        EENetwork CreateNetwork();

        /// <summary>
        /// Bus input data
        /// </summary>
        IEnumerable<EEBus> Busses { get; }

        /// <summary>
        /// Line input data
        /// </summary>
        IEnumerable<EELine> Lines { get; }

        /// <summary>
        /// Load flow result for testing
        /// </summary>
        IEnumerable<EEBus> LFResult { get; }

        /// <summary>
        /// Y matrix result for testing
        /// </summary>
        MxDTO<Complex> YResult { get; }

        /// <summary>
        /// J1 matrix result for testing
        /// </summary>
        MxDTO<double> J1Result { get; }

        /// <summary>
        /// J2 matrix result for testing
        /// </summary>
        MxDTO<double> J2Result { get; }

        /// <summary>
        /// J3 matrix result for testing
        /// </summary>
        MxDTO<double> J3Result { get; }

        /// <summary>
        /// J4 matrix result for testing
        /// </summary>
        MxDTO<double> J4Result { get; }

        /// <summary>
        /// Convenient method only
        /// </summary>
        double GetJ1kk(BusResult b1, Matrix<double> res = null);

        /// <summary>
        /// Convenient method only
        /// </summary>
        double GetJ1kn(BusResult b1, BusResult b2, Matrix<double> res = null);
    }

}
