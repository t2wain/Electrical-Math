using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
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
        protected MxDTO<double> _J2Result;
        protected MxDTO<double> _J3Result;
        protected MxDTO<double> _J4Result;

        public double BasePower => _BasePower;

        public IEnumerable<EEBus> Busses => _Busses;
        public IEnumerable<EELine> Lines => _Lines;

        public IEnumerable<EEBus> LFResult => _LFResult;
        public MxDTO<Complex> YResult => _YResult;
        public MxDTO<double> J1Result => _J1Result;
        public MxDTO<double> J2Result => _J2Result;
        public MxDTO<double> J3Result => _J3Result;
        public MxDTO<double> J4Result => _J4Result;
        public abstract double GetJ1kk(BusResult b1, Matrix<double> res = null);
        public abstract double GetJ1kn(BusResult b1, BusResult b2, Matrix<double> res = null);

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


        protected Complex Zero => Complex.Zero;
        protected Complex C(double g, double b) => new Complex(g, b);

        public void Dispose()
        {
            _network?.Dispose();
            _network = null;
            _Busses = null;
            _Lines = null;
            _LFResult = null;
            _YResult = null;
            _J1Result = null;
            _J2Result = null;
            _J2Result = null;
            _J4Result = null;
        }
    }
}
