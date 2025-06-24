using System;

namespace EEMathLib
{
    public abstract class PUBase
    {
        #region Base values

        public double Power { get; protected set; }
        public double Voltage { get; protected set; }
        public double Current { get; protected set; }
        public double Impedance { get; protected set; }

        #endregion

        #region Conversion

        public static Phasor ToPU(IPhasor value, double baseValue) =>
            value.Base / baseValue;

        public static Phasor ToValue(IPhasor value, double baseValue) =>
            value.Base * baseValue;

        /// <summary>
        /// Calclate the conversion factor to a new per-unit base.
        /// </summary>
        public static double ConvertFactor(PUBase frombase, PUBase tobase) =>
            Math.Pow(frombase.Voltage / tobase.Voltage, 2)
                * (tobase.Power / frombase.Power);

        /// <summary>
        /// Covert the pu to a new base
        /// </summary>
        public Phasor ConvertPU(IPhasor pu, PUBase newbase) =>
            pu.Base * ConvertFactor(this, newbase);

        /// <summary>
        /// Convert impedance to pu
        /// </summary>
        public IZImp ToPU(IZImp value) => ToPU(value.Base, Impedance);

        /// <summary>
        /// Convert current to pu
        /// </summary>
        public ICurrent ToPU(ICurrent value) => ToPU(value.Base, Current);

        /// <summary>
        /// Convert voltage to pu
        /// </summary>
        public IVoltage ToPU(IVoltage value) => ToPU(value.Base, Voltage);

        /// <summary>
        /// Convert impedance pu to actual value
        /// </summary>
        public IZImp ToValue(IZImp pu) => ToValue(pu.Base, Impedance);

        /// <summary>
        /// Convert current pu to actual value
        /// </summary>
        public ICurrent ToValue(ICurrent pu) => ToValue(pu.Base, Current);

        /// <summary>
        /// Convert voltage pu to actual value
        /// </summary>
        public IVoltage ToValue(IVoltage pu) => ToValue(pu.Base, Voltage);

        #endregion
    }

    /// <summary>
    /// Single phase system
    /// </summary>
    public class PUBase1P : PUBase
    {
        /// <summary>
        /// Create single-phase per-unit base
        /// </summary>
        /// <param name="s1Power">Base power of single-phase system</param>
        /// <param name="voltageLN">Base line-to-neutral voltage</param>
        public PUBase1P(double s1Power, double voltageLN)
        {
            Power = s1Power;
            Voltage = voltageLN;
            Current = s1Power / Voltage;
            Impedance = Math.Pow(voltageLN, 2) / s1Power;
        }
    }

    /// <summary>
    /// Three phase system
    /// </summary>
    public class PUBase3P : PUBase
    {
        /// <summary>
        /// Create a three-phase per-unit base
        /// </summary>
        /// <param name="s3Power">Base power of three-phase system</param>
        /// <param name="voltageLL">Base line-to-line voltage</param>
        public PUBase3P(double s3Power, double voltageLL)
        {
            Power = s3Power;
            Voltage = voltageLL;

            Current = s3Power / (Math.Sqrt(3) * voltageLL);
            Impedance = Math.Pow(voltageLL, 2) / s3Power;
        }
    }

}
