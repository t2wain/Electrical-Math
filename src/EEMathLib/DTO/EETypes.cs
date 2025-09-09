using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.DTO
{
    public interface IEntity
    {
        string ID { get; set; }
        int EntityType { get; }

    }

    public interface IEZComp
    {
        Complex Zero { get; }
        Complex One { get; }
        Complex Two { get; }
    }

    public interface IEBus : IEntity
    {
        int BusIndex { get; set; }
        double Voltage { get; set; }
    }

    public interface IELine : IEntity
    {
        IEBus FromBus { get; }
        IEBus ToBus { get; }
        Complex ZSeries { get; }
        Complex YShunt { get; }
    }

    public interface IETransformer : IEntity
    {
        IEBus FromBus { get; }
        IEBus ToBus { get; }
        double X { get; }
    }

    public interface IEGen : IEntity
    {
        IEBus Bus { get; set; }
        double Pgen { get; set; }
        double Qgen { get; set; }
        double Qmin { get; set; }
        double Qmax { get; set; }

        double Xpp { get; set; }
        double Xp { get; set; }
        double X {  get; set; }
    }

    public interface IELoad : IEntity
    {
        IEBus Bus { get; set; }
        double Pload { get; set; }
        double Qload { get; set; }
        double Xpp { get; set; }
    }

    public interface IENetwork
    {
        IEnumerable<IEBus> Buses { get; }
        IEnumerable<IELine> Lines { get; }
        IEnumerable<IETransformer> Transformers { get; }
        IEnumerable<IEGen> Generators { get; }
        IEnumerable<IELoad> Loads { get; }
        Matrix<Complex> YMatrix { get; set; }

    }
}
