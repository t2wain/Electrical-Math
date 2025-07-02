using System;
using System.Numerics;

namespace EEMathLib
{
    #region Types

    public interface IPhasor
    {
        double Magnitude { get; }
        double Phase { get; }
        Phasor Base { get; }
        Complex ToComplex();
        Phasor Conjugate();
        Phasor ShiftPhaseBy(double angle);
    }

    public interface IVoltage : IPhasor { }

    public interface IVoltageLN : IVoltage 
    {
        IVoltageLL ToVLL();
    }

    public interface IVoltageLL : IVoltage 
    {
        IVoltageLN ToVLN();
    }

    public interface ICurrent : IPhasor 
    {
        /// <summary>
        /// The definition is always reference to the voltage phasor.
        /// The default reference is voltage phasor with 0 degree.
        /// </summary>
        bool IsLag(IVoltage voltage = null);
    }

    public interface IYImp : IPhasor 
    { 
        IZImp ToZImp();
    }
    public interface IZImp : IPhasor 
    {
        IYImp ToYImp();
    }
    public interface IPower : IPhasor 
    {
        double PowerFactor { get; }
        bool IsInductiveVars { get; }
    }

    public interface IPowerS1 : IPower { }

    public interface IPowerS3 : IPower { }

    #endregion


    /// <summary>
    /// Phasor represents a sinusoidal value of many
    /// electrical quantities. The interface marks
    /// the intended electrical value of the phasor.
    /// </summary>
    public struct Phasor : IPhasor, ICurrent, IZImp, IYImp, IPower, 
        IPowerS1, IPowerS3, IVoltage, IVoltageLL, IVoltageLN
    {
        #region Constructor

        /// <summary>
        /// Create a new phasor with 0 degree
        /// </summary>
        /// <param name="magnitude">RMS value</param>
        public Phasor(double magnitude) : this(magnitude, 0.0) { }

        /// <summary>
        /// Create a new phasor
        /// </summary>
        /// <param name="magnitude">RMS value</param>
        /// <param name="phase">Phase in degree</param>
        public Phasor(double magnitude, double phase)
        {
            Magnitude = magnitude;
            Phase = ConvertDegreeToStandard(phase);
        }

        #endregion

        #region IPhasor

        /// <summary>
        /// RMS value
        /// </summary>
        public double Magnitude { get; private set; }

        /// <summary>
        /// Phase in degree
        /// </summary>
        public double Phase { get; private set; }

        public Phasor Base => this;

        /// <summary>
        /// Convert phasor to rectangular form
        /// </summary>
        public Complex ToComplex() =>
            Complex.FromPolarCoordinates(Magnitude, ConvertDegreeToRadian(Phase));

        public Phasor Conjugate() => new Phasor(Magnitude, -Phase);

        public Phasor ShiftPhaseBy(double angle) =>
            new Phasor(Magnitude, Phase + angle);

        #endregion

        #region IVoltage

        IVoltageLL IVoltageLN.ToVLL() =>
            this * Math.Sqrt(3);

        IVoltageLN IVoltageLL.ToVLN() =>
            this / Math.Sqrt(3);

        #endregion

        #region ICurrent

        bool ICurrent.IsLag(IVoltage voltage) =>
            voltage == null ? Math.Sign(Phase) == -1 : voltage.Phase > Phase;

        #endregion

        #region IPower

        double IPower.PowerFactor => Math.Cos(Phase);
        bool IPower.IsInductiveVars => Math.Sign(Phase) == 1;

        #endregion

        #region Impedance

        IYImp IZImp.ToYImp() => 1 / this;

        IZImp IYImp.ToZImp() => 1 / this;

        #endregion

        #region Convert

        /// <summary>
        /// Convert rectangular form to phasor
        /// </summary>
        public static Phasor Convert(Complex value) =>
            new Phasor(value.Magnitude, ConvertRadianToDegree(value.Phase));

        /// <summary>
        /// Convert tuple to phasor
        /// </summary>
        public static Phasor Convert((double Magnitude, double Phase) value) =>
            new Phasor(value.Magnitude, value.Phase);

        /// <summary>
        /// Power factor is always a positive value. Lead/Lag must also be
        /// specified to create a phasor for power.
        /// </summary>
        public static IPower CreatePowerPhasorFromApparentPower(double apparentPower, double powerfactor, bool isLag) =>
            new Phasor(apparentPower, ConvertPowerFactorToDegree(powerfactor) * (isLag ? 1 : -1));

        /// <summary>
        /// Power factor is always a positive value. Lead/Lag must also be
        /// specified to create a phasor for power.
        /// </summary>
        public static IPower CreatePowerPhasorFromActivePower(IPowerS3 activePower, double powerfactor, bool isLag) =>
            new Phasor(activePower.ToComplex().Real / powerfactor, ConvertPowerFactorToDegree(powerfactor) * (isLag ? 1 : -1));

        /// <summary>
        /// Power factor is always a positive value. Lead/Lag must also be
        /// specified to create a phasor for current. Lead/Lag is relative
        /// to the voltage phasor of a circuit with a default of 0 degree.
        /// </summary>
        public static ICurrent CreateCurrentPhasor(double magnitude, double powerfactor, bool isLag) =>
            new Phasor(magnitude, ConvertPowerFactorToDegree(powerfactor) * (isLag ? -1 : 1));

        /// <summary>
        /// Convert degree to radian
        /// </summary>
        public static double ConvertDegreeToRadian(double degree) =>
            degree % 360 / 180 * Math.PI;

        public static double ConvertPowerFactorToDegree(double powerFactor) =>
            ConvertRadianToDegree(Math.Acos(powerFactor));

        /// <summary>
        /// Convert radian to degree
        /// </summary>
        public static double ConvertRadianToDegree(double radian)
        {
            var degree = radian % (2 * Math.PI) / Math.PI * 180;
            return ConvertDegreeToStandard(degree);
        }

        public static double ConvertDegreeToStandard(double degree)
        {
            var d = degree % 360;
            if (Math.Abs(d) > 180)
                return (360 - Math.Abs(d)) * Math.Sign(d) * -1;
            else return d;
        }

        #endregion

        #region Operators

        public static Phasor operator -(Phasor left, Phasor right) =>
            Convert(left.ToComplex() - right.ToComplex());

        public static Phasor operator +(Phasor left, Phasor right) =>
            Convert(left.ToComplex() + right.ToComplex());

        public static Phasor operator *(Phasor left, Phasor right) =>
            new Phasor(left.Magnitude * right.Magnitude, left.Phase + right.Phase);

        public static Phasor operator *(Phasor left, double scalar) =>
            new Phasor(left.Magnitude * scalar, left.Phase);

        public static Phasor operator /(Phasor left, Phasor right) =>
            new Phasor(left.Magnitude / right.Magnitude, left.Phase - right.Phase);

        public static Phasor operator /(Phasor left, double scalar) =>
            new Phasor(left.Magnitude / scalar, left.Phase);

        public static Phasor operator /(double scalar, Phasor right) =>
            new Phasor(scalar / right.Magnitude, -right.Phase);

        public static implicit operator Phasor((double Magnitude, double Phase) value) =>
            new Phasor(value.Magnitude, value.Phase);

        public static implicit operator Phasor(Complex value) =>
            Convert(value);

        #endregion
    }
}
