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

        INewtonRaphsonData GetNewtonRaphsonData(int iteration = 0);
    }

    public interface IJacobianData
    {
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

    /// <summary>
    /// PDelta = Pspecify - PCal;
    /// QDelta = Qspecify - QCal;
    /// Mismach vector MDelta = [ PDelta, QDelta ];
    /// Correction vector [XDelta] = [ADelta, VDelata];
    /// [J][XDelta] = [MDelta]
    /// </summary>
    public interface INewtonRaphsonData
    {
        int Iteration { get; }

        /// <summary>
        /// Jacobian matrix [J]
        /// </summary>
        IJacobianData JacobianData { get; }

        #region Mismatch vector MDelta

        /// <summary>
        /// PDelta = Pspecify - PCal;
        /// </summary>
        double[] PCal { get; }

        /// <summary>
        /// QDelta = Qspecify - QCal;
        /// </summary>
        double[] QCal { get; }

        /// <summary>
        /// PDelta = Pspecify - PCal;
        /// </summary>
        double[] PDelta { get; }

        /// <summary>
        /// QDelta = Qspecify - QCal;
        /// </summary>
        double[] QDelta { get; }

        /// <summary>
        /// Mismach vector MDelta = [ PDelta, QDelta ];
        /// </summary>
        double[] MDelta { get; }

        #endregion

        #region Correction vector

        /// <summary>
        /// Correction vector [XDelta] = [ADelta, VDelata]
        /// </summary>
        double[] ADelta { get; }

        /// <summary>
        /// Correction vector [XDelta] = [ADelta, VDelata]
        /// </summary>
        double[] VDelta { get; }

        #endregion

        /// <summary>
        /// VBus result
        /// </summary>
        double[] VBus { get; }

        /// <summary>
        /// ABus result
        /// </summary>
        double[] ABus { get; }
    }
}
