using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.LoadFlow.Data
{
    public class NewtonRaphsonData : INewtonRaphsonData
    {
        public int Iteration { get; set; }
        public IJacobianData JacobianData { get; set; }

        public double[] PCal { get; set; }
        public double[] QCal { get; set; }
        public double[] PDelta { get; set; }
        public double[] QDelta { get; set; }
        public double[] MDelta { get; set; }

        public double[] ADelta { get; set; }
        public double[] VDelta { get; set; }

        public double[] VBus { get; set; }
        public double[] ABus { get; set; }
    }

    public class JacobianData : IJacobianData
    {
        public MxDTO<double> J1Result { get; set; }
        public MxDTO<double> J2Result { get; set; }
        public MxDTO<double> J3Result { get; set; }
        public MxDTO<double> J4Result { get; set; }
        public virtual double GetJ1kk(BusResult b1, Matrix<double> res = null) =>
            throw new NotImplementedException();
        public virtual double GetJ1kn(BusResult b1, BusResult b2, Matrix<double> res = null) =>
            throw new NotImplementedException();
    }

    /// <summary>
    /// Load flow dataset base implementation
    /// </summary>
    public abstract class LFDataAbstract : ILFData
    {
        LFNetwork _network = null;

        protected double _BasePower;
        protected IEnumerable<EEBus> _Busses; 
        protected IEnumerable<EELine> _Lines; 
        protected MxDTO<Complex> _YResult;

        public double BasePower => _BasePower;

        public IEnumerable<EEBus> Busses => _Busses;
        public IEnumerable<EELine> Lines => _Lines;

        public MxDTO<Complex> YResult => _YResult;

        public virtual LFNetwork CreateNetwork()
        {
            if (_network == null)
            {
                _network = new LFNetwork(this);
                _network.AssignBusToLine();
                _network.BuildYImp();
            }
            return _network;
        }

        virtual public INewtonRaphsonData GetNewtonRaphsonData(int iteration = 0) =>
            throw new NotImplementedException();

        protected Complex Zero => Complex.Zero;
        protected Complex C(double g, double b) => new Complex(g, b);

        virtual public void Dispose()
        {
            _network?.Dispose();
            _network = null;
            _Busses = null;
            _Lines = null;
            _YResult = null;
        }
    }
}
