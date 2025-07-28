using EEMathLib.DTO;
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
    }

}
