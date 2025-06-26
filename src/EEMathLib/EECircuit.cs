using System;

namespace EEMathLib
{
    /// <summary>
    /// Provide various electrical circuit calculations
    /// of voltage, current, impedance, and power in
    /// single-phase or three-phase system.
    /// </summary>
    public static class EECircuit
    {

        #region Power

        /// <summary>
        /// Calculate three phase power with V and I phasors
        /// </summary>
        /// <param name="vln">Line-to-neutral voltage</param>
        /// <param name="amp">Line current</param>
        public static IPowerS3 PowerS3(IVoltageLN vln, ICurrent amp) =>
            vln.Base * amp.Conjugate() * 3;

        /// <summary>
        /// Calculate single phase power with V and I phasors
        /// </summary>
        /// <param name="vln">Line-to-neutral voltage</param>
        /// <param name="amp">Line current</param>
        public static IPowerS1 Power(IVoltageLN vln, ICurrent amp) =>
            vln.Base * amp.Conjugate();

        /// <summary>
        /// Calculate three phase power with V and I phasors
        /// </summary>
        /// <param name="voltage">Line-to-line voltage</param>
        /// <param name="amp">Line current</param>
        public static IPowerS3 Power(IVoltageLL voltage, ICurrent amp) =>
            voltage.Base * amp.Conjugate() * Math.Sqrt(3);

        #endregion

        #region Voltage

        /// <summary>
        /// Calculate voltage phasor
        /// </summary>
        /// <param name="amp">Line current</param>
        /// <param name="zimp">Impedance</param>
        /// <returns>Voltage phasor</returns>
        public static IVoltage Voltage(ICurrent amp, IZImp zimp) =>
            amp.Base * zimp.Base;

        /// <summary>
        /// Calculate voltage at load terminal
        /// </summary>
        public static IVoltage CalcVLoad(IVoltage voltSrc, ICurrent loadAmp, IZImp zimp) =>
            voltSrc.Base - Voltage(loadAmp, zimp).Base;

        /// <summary>
        /// Calculate percent voltage drop at load terminal
        /// </summary>
        public static double CalcVDrop(IVoltage voltSrc, ICurrent loadAmp, IZImp zimp) =>
            CalcVLoad(voltSrc, loadAmp, zimp).Magnitude / voltSrc.Magnitude * 100;

        #endregion

        #region Impedance

        /// <summary>
        /// Calculate impedance
        /// </summary>
        /// <param name="voltage">Voltage</param>
        /// <param name="amp">Current</param>
        public static IZImp ZImp(IVoltage voltage, ICurrent amp) =>
            voltage.Base / amp.Base;

        /// <summary>
        /// Calculate impedance
        /// </summary>
        /// <param name="power">Single phase power</param>
        /// <param name="voltage">Line-to-neutral voltage</param>
        public static IZImp ZImp(IPowerS1 power, IVoltageLN voltage) =>
            voltage.Base * voltage.Base / power.Base;

        /// <summary>
        /// Calculate impedance
        /// </summary>
        /// <param name="power">Three phase power</param>
        /// <param name="voltage">Line-to-line voltage</param>
        public static IZImp ZImp(IPowerS3 power, IVoltageLL voltage) =>
            voltage.Base * voltage.Base / power.Base;

        #endregion

        #region Current

        /// <summary>
        /// Calculate current
        /// </summary>
        /// <param name="voltage">Voltage</param>
        /// <param name="zimp">Impedance</param>
        public static ICurrent Current(IVoltage voltage, IZImp zimp) =>
            voltage.Base / zimp.Base;

        public static ICurrent Current(IPowerS1 power, IVoltageLN voltage) =>
            (power.Base / voltage.Base).Conjugate();

        public static ICurrent Current(IPowerS3 power, IVoltageLL voltage) =>
            (power.Base / (voltage.Base * Math.Sqrt(3))).Conjugate();

        /// <summary>
        /// Calculate load current
        /// </summary>
        /// <param name="activePower">Reactive power must be zero</param>
        public static ICurrent Current(IPowerS3 activePower, IVoltageLL voltage, double powerFactor, bool isLag)
        {
            IPowerS3 apparentPower = Phasor.CreatePowerPhasorFromActivePower(activePower, powerFactor, isLag).Base;
            ICurrent current = Current(apparentPower, voltage);
            return current;
        }

        #endregion
    }
}
