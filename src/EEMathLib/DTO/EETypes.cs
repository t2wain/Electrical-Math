using EEMathLib.ShortCircuit.Data;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.DTO
{
    public enum WindingEnum
    {
        D,
        Y,
        Yn,
        Dy,
        Dyn,
        Yd,
        YNd,
        YNyn
    }

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
        WindingEnum Winding { get; }
        double X { get; }
    }

    public interface IEZGen : IEntity
    {
        IEBus Bus { get; set; }
        WindingEnum Winding { get; }
        double Rs { get; }
        double Xm { get; }
        Complex Zn { get; }
        double Xs { get; }
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

        WindingEnum Winding { get; }
        double Rs { get; }
        double Xm { get; }
        Complex Zn { get; }
        double Xs { get; }

        IEZGen GetZGen(ZTypeEnum ztype);
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
