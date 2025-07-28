using EEMathLib.DTO;
using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.LoadFlow.Data
{
    public abstract class LFDataAbstract : ILFData
    {
        EENetwork _network = null;

        protected double _BasePower;
        protected IEnumerable<EEBus> _Busses; 
        protected IEnumerable<EELine> _Lines; 
        protected IEnumerable<EEBus> _LFResult; 
        protected MxDTO<Complex> _YResult;
        protected MxDTO<double> _J1Result;

        public double BasePower => _BasePower;

        public IEnumerable<EEBus> Busses => _Busses;
        public IEnumerable<EELine> Lines => _Lines;

        public IEnumerable<EEBus> LFResult => _LFResult;
        public MxDTO<Complex> YResult => _YResult;
        public MxDTO<double> J1Result => _J1Result;

        public EENetwork CreateNetwork()
        {
            if (_network == null)
            {
                _network = new EENetwork(this);
                _network.AssignBusToLine();
                _network.BuildYImp();
            }
            return _network;
        }

        public void Dispose()
        {
            _network?.Dispose();
            _network = null;
            _Busses = null;
            _Lines = null;
            _LFResult = null;
            _YResult = null;
            _J1Result = null;
        }
    }
}
