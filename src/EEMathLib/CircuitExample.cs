using System;

namespace EEMathLib
{
    public class CircuitExample
    {
        public void Example1()
        {
            // given voltage and power at load terminal
            IVoltage vload = new Phasor(33_000 / Math.Sqrt(3), 0);
            IPower sload = Phasor.CreatePowerPhasorFromApparentPower(5_000_000, 0.8, true).Base / 3;

            // line condition
            IZImp lzimp = new Phasor(11.5, 22);

            // calc line voltage drop
            ICurrent iload = (sload.Base / vload.Base).Conjugate();
            IVoltage vdrop = iload.Base * lzimp.Base;

            // calc voltage at source terminal
            IVoltage vsrc = vdrop.Base + vload.Base;
        }
    }
}
