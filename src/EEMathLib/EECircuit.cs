namespace EEMathLib
{
    public static class EECircuit
    {
        /// <summary>
        /// Calculate three phase power with V and I phasors
        /// </summary>
        /// <param name="vln">Line-to-neutral voltage</param>
        /// <param name="amp">Line current</param>
        public static IPower PowerS3(IVoltage vln, ICurrent amp) =>
            vln.Base * amp.Conjugate() * 3;

        /// <summary>
        /// Calculate single phase power with V and I phasors
        /// </summary>
        /// <param name="vln">Line-to-neutral voltage</param>
        /// <param name="amp">Line current</param>
        public static IPower Power(IVoltage vln, ICurrent amp) =>
            vln.Base * amp.Conjugate();

        /// <summary>
        /// Calculate voltage phasor
        /// </summary>
        /// <param name="amp">Line current</param>
        /// <param name="zimp">Impedance</param>
        /// <returns>Voltage phasor</returns>
        public static IVoltage Voltage(ICurrent amp, IZImp zimp) =>
            amp.Base * zimp.Base;

        /// <summary>
        /// Calculate impedance
        /// </summary>
        /// <param name="voltage">Voltage</param>
        /// <param name="amp">Current</param>
        public static IZImp ZImp(IVoltage voltage, ICurrent amp) =>
            voltage.Base / amp.Base;

        /// <summary>
        /// Calculate current
        /// </summary>
        /// <param name="voltage">Voltage</param>
        /// <param name="zimp">Impedance</param>
        public static ICurrent Current(IVoltage voltage, IZImp zimp) =>
            voltage.Base / zimp.Base;
    }
}
