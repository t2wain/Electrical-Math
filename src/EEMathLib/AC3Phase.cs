namespace EEMathLib
{
    public interface IAC3PhaseMixin
    {
        #region Voltage

        /// <summary>
        /// Phase voltage (string)
        /// </summary>
        IVoltage Us1 { get; }
        /// <summary>
        /// Phase voltage (string)
        /// </summary>
        IVoltage Us2 { get; }
        /// <summary>
        /// Phase voltage (string)
        /// </summary>
        IVoltage Us3 { get; }

        /// <summary>
        /// Line voltage between L1 and L2
        /// </summary>
        IVoltageLL U12 { get; }
        /// <summary>
        /// Line voltage between L2 and L3
        /// </summary>
        IVoltageLL U23 { get; }
        /// <summary>
        /// Line voltage between L3 and L1
        /// </summary>
        IVoltageLL U31 { get; }

        #endregion

        #region Impedance

        /// <summary>
        /// Phase impedance (string)
        /// </summary>
        IZImp Zs { get; }

        #endregion

        #region Current

        /// <summary>
        /// Phase current (string)
        /// </summary>
        ICurrent Is1 { get; }
        /// <summary>
        /// Phase current (string)
        /// </summary>
        ICurrent Is2 { get; }
        /// <summary>
        /// Phase current (string)
        /// </summary>
        ICurrent Is3 { get; }

        /// <summary>
        /// Line current on L1
        /// </summary>
        ICurrent I1 { get; }
        /// <summary>
        /// Line current on L2
        /// </summary>
        ICurrent I2 { get; }
        /// <summary>
        /// Line current on L3
        /// </summary>
        ICurrent I3 { get; }

        #endregion
    }

    /// <summary>
    /// Three-phase system consists of three conductors, L1, L2, and L3.
    /// The voltages between conductors are U12, U23, and U31.
    /// </summary>
    public interface IDelta : IAC3PhaseMixin 
    {
        /// <summary>
        /// Phase current, same as Is1
        /// </summary>
        ICurrent I12 { get; }
        /// <summary>
        /// Phase current, same as Is2
        /// </summary>
        ICurrent I23 { get; }
        /// <summary>
        /// Phase current, same as Is3
        /// </summary>
        ICurrent I31 { get; }
    }

    /// <summary>
    /// Three-phase system consists of four conductors, L1, L2, L3, and Ln.
    /// The voltages between conductors are U12, U23, and U31.
    /// </summary>
    public interface IWye : IAC3PhaseMixin 
    {
        /// <summary>
        /// Same as Us1
        /// </summary>
        IVoltageLN U1n { get; }
        /// <summary>
        /// Same as Us2
        /// </summary>
        IVoltageLN U2n { get; }
        /// <summary>
        /// Same as Us3
        /// </summary>
        IVoltageLN U3n { get; }

    }

    public abstract class AC3Phase : IAC3PhaseMixin, IDelta, IWye
    {
        /// <summary>
        /// U12, U23, U31 line voltage phase shift in degree.
        /// </summary>
        public static double WyeDeltaXfmrPhaseShift => -30;
        /// <summary>
        /// U12, U23, U31 line voltage phase shift in degree.
        /// </summary>
        public static double DeltaWyeXfmrPhaseShift => 30;

        #region Voltage

        /// <summary>
        /// Phase voltage
        /// </summary>
        public IVoltage Us1 { get; protected set; }
        /// <summary>
        /// Phase voltage
        /// </summary>
        public IVoltage Us2 { get; protected set; }
        /// <summary>
        /// Phase voltage
        /// </summary>
        public IVoltage Us3 { get; protected set; }

        /// <summary>
        /// Line voltage
        /// </summary>
        public IVoltageLL U12 { get; protected set; }
        /// <summary>
        /// Line voltage
        /// </summary>
        public IVoltageLL U23 { get; protected set; }
        /// <summary>
        /// Line voltage
        /// </summary>
        public IVoltageLL U31 { get; protected set; }

        #endregion

        #region Impedance

        /// <summary>
        /// Phase impedance
        /// </summary>
        public IZImp Zs { get; protected set; }

        #endregion

        #region Current

        /// <summary>
        /// Phase current
        /// </summary>
        public ICurrent Is1 { get; protected set; }
        /// <summary>
        /// Phase current
        /// </summary>
        public ICurrent Is2 { get; protected set; }
        /// <summary>
        /// Phase current
        /// </summary>
        public ICurrent Is3 { get; protected set; }

        /// <summary>
        /// Line current
        /// </summary>
        public ICurrent I1 { get; protected set; }
        /// <summary>
        /// Line current
        /// </summary>
        public ICurrent I2 { get; protected set; }
        /// <summary>
        /// Line current
        /// </summary>
        public ICurrent I3 { get; protected set; }

        #endregion

        #region Delta

        /// <summary>
        /// Phase current
        /// </summary>
        protected ICurrent I12 { get; set; }
        /// <summary>
        /// Phase current
        /// </summary>
        protected ICurrent I23 { get; set; }
        /// <summary>
        /// Phase current
        /// </summary>
        protected ICurrent I31 { get; set; }

        /// <summary>
        /// Phase current
        /// </summary>
        ICurrent IDelta.I12 => I12;
        /// <summary>
        /// Phase current
        /// </summary>
        ICurrent IDelta.I23 => I23;
        /// <summary>
        /// Phase current
        /// </summary>
        ICurrent IDelta.I31 => I31;

        #endregion

        #region Wye

        protected IVoltageLN U1n { get; set; }
        protected IVoltageLN U2n { get; set; }
        protected IVoltageLN U3n { get; set; }

        IVoltageLN IWye.U1n => U1n;
        IVoltageLN IWye.U2n => U2n;
        IVoltageLN IWye.U3n => U3n;

        #endregion
    }

    /// <summary>
    /// Three-phase system consists of four conductors, L1, L2, L3, and Ln.
    /// The line voltages between conductors are U12, U23, and U31.
    /// </summary>
    public class Wye : AC3Phase, IWye
    {
        public Wye() : this(new Phasor(1, 0), new Phasor(1, 0)) { }
        public Wye(IVoltage u1, IZImp zs)
        {
            // Phase voltages, clock-wise rotation
            // Line-to-neutral voltages
            Us1 = u1;
            Us2 = u1.ShiftPhaseBy(-120);
            Us3 = u1.ShiftPhaseBy(120);

            U1n = Us1.Base;
            U2n = Us2.Base;
            U3n = Us3.Base;

            Zs = zs;

            // Line-to-line voltages
            U12 = Us1.Base - Us2.Base;
            U23 = Us2.Base - Us3.Base;
            U31 = Us3.Base - Us1.Base;

            Is1 = Us1.Base / Zs.Base;
            Is2 = Us2.Base / Zs.Base;
            Is3 = Us3.Base / Zs.Base;

            I1 = Is1;
            I2 = Is2;
            I3 = Is3;
        }
    }

    /// <summary>
    /// Three-phase system consists of three conductors, L1, L2, and L3.
    /// The line voltages between conductors are U12, U23, and U31.
    /// </summary>
    public class Delta : AC3Phase, IDelta
    {
        public Delta() : this(new Phasor(1, 0), new Phasor(1, 0)) { }
        public Delta(IVoltage u1, IZImp zs)
        {
            // Phase voltages, clock-wise rotation
            Us1 = u1;
            Us2 = u1.ShiftPhaseBy(-120);
            Us3 = u1.ShiftPhaseBy(120);

            Zs = zs;

            // Line-to-line voltages
            U12 = Us1.Base;
            U23 = Us2.Base;
            U31 = Us3.Base;

            // Phase currents
            I12 = Is1 = Us1.Base / Zs.Base;
            I23 = Is2 = Us2.Base / Zs.Base;
            I31 = Is3 = Us3.Base / Zs.Base;

            // Line currents
            I1 = I12.Base - I31.Base;
            I2 = I23.Base - I12.Base;
            I3 = I31.Base - I23.Base;
        }
    }
}
