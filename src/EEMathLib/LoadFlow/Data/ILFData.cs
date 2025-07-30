using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.LoadFlow.Data
{
    public interface ILFData : IDisposable
    {
        EENetwork CreateNetwork();
        IEnumerable<EEBus> Busses { get; }
        IEnumerable<EELine> Lines { get; }

        IEnumerable<EEBus> LFResult { get; }
        MxDTO<Complex> YResult { get; }
        MxDTO<double> J1Result { get; }
        MxDTO<double> J2Result { get; }
        MxDTO<double> J3Result { get; }
        MxDTO<double> J4Result { get; }

        double GetJ1kk(BusResult b1, Matrix<double> res = null);
        double GetJ1kn(BusResult b1, BusResult b2, Matrix<double> res = null);
    }

}
