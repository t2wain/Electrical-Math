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

    public class Entity : IEntity
    {
        public string ID { get; set; }
        public int EntityType { get; set;  }
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

    public class EBus : Entity, IEBus
    {
        public int BusIndex { get; set; }
        public double Voltage { get; set; } = 1.0;
    }

    public interface IELine : IEntity
    {
        IEBus FromBus { get; }
        IEBus ToBus { get; }
        Complex ZSeries { get; }
        Complex YShunt { get; }
    }

    public class ELine : Entity, IELine
    {
        public IEBus FromBus { get; set; }
        public IEBus ToBus { get; set; }
        public Complex ZSeries { get; set; }
        public Complex YShunt { get; set; }
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
