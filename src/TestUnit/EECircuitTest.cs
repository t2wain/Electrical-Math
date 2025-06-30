using EEMathLib;
using System.Numerics;

namespace TestUnit
{
    public class EECircuitTest
    {
        #region Power

        [Fact]
        public void Calc_S1_from_vln_and_current()
        {
            // arrange
            IVoltageLN e = new Phasor(100, 0);
            ICurrent i = new Phasor(20, -53.1);

            // act
            IPowerS1 s = EECircuit.Power(e, i);

            // assert
            IPowerS1 res = new Phasor(2000, 53.1);
            Assert.True(Checker.EQ(s.Base, res.Base, 0.1, 0.1));
        }

        [Fact]
        public void Calc_S3_from_vll_and_current()
        {
            // arrange
            IVoltageLN vln = new Phasor(100, 0);
            ICurrent i = new Phasor(20, -53.1);

            // act
            IVoltageLL vll = vln.ToVLL();
            IPowerS3 s = EECircuit.Power(vll, i);

            // assert
            IPowerS3 res = new Phasor(2000, 53.1) * 3;
            Assert.True(Checker.EQ(s.Base, res.Base, 0.1, 0.1));
        }

        [Fact]
        public void Calc_S3_from_vln_and_current()
        {
            // arrange
            IVoltageLN vln = new Phasor(100, 0);
            ICurrent i = new Phasor(20, -53.1);

            // act
            IPowerS3 s = EECircuit.PowerS3(vln, i);

            // assert
            IPowerS3 res = new Phasor(2000, 53.1) * 3;
            Assert.True(Checker.EQ(s.Base, res.Base, 0.1, 0.1));
        }


        #endregion

        #region Current

        [Fact]
        public void Calc_current_from_voltge_and_impedance() 
        {
            // arrange
            IVoltage e = new Phasor(100, 0);
            IZImp z = Phasor.Convert(new Complex(3, 4));

            // act
            ICurrent i = EECircuit.Current(e, z);

            // assert
            ICurrent res = new Phasor(20, -53.1);
            Assert.True(Checker.EQ(i.Base, res.Base, 0.1, 0.1));
        }

        [Fact]
        public void Calc_current_from_S1_and_vln()
        {
            IVoltageLN e = new Phasor(100, 0);
            IPowerS1 p = new Phasor(2000, 53.1);

            ICurrent i = EECircuit.Current(p, e);

            ICurrent res = new Phasor(20, -53.1);
            Assert.True(Checker.EQ(i.Base, res.Base, 0.1, 0.1));
        }

        [Fact]
        public void Calc_current_from_S3_and_vll()
        {
            // arrange
            IVoltageLN vln = new Phasor(100, 0);
            IPowerS1 s1 = new Phasor(2000, 53.1);

            // convert to 3 phase
            IVoltageLL vll = vln.ToVLL();
            IPowerS3 s3 = new Phasor(2000, 53.1) * 3;

            // act
            ICurrent i = EECircuit.Current(s3, vll);

            // assert
            ICurrent res = new Phasor(20, -53.1);
            Assert.True(Checker.EQ(i.Base, res.Base, 0.1, 0.1));
        }

        #endregion

        #region Voltage

        [Fact]
        public void Calc_voltage_from_impedance_and_current()
        {
            IZImp z = Phasor.Convert(new Complex(3, 4));
            ICurrent i = new Phasor(20, -53.1);

            IVoltage v = EECircuit.Voltage(i, z);

            IVoltage res = new Phasor(100, 0);
            Assert.True(Checker.EQ(v.Base, res.Base, 0.1, 0.1));
        }

        [Fact]
        public void Calc_voltage_drop_IEEE()
        {
            IVoltage v = new Phasor(120, 0);
            ICurrent i = new Phasor(20, -30);
            IZImp z = Phasor.Convert(new Complex(0.2, 0.1));

            double vdropPct = EECircuit.CalcPctVDropIEEE(v, i, z);

            double res = 3.72;
            Assert.True(Checker.EQ(vdropPct, res, 0.1, 0.1));
        }

        [Fact]
        public void Calc_voltage_drop()
        {
            IVoltage v = new Phasor(120, 0);
            ICurrent i = new Phasor(20, -30);
            IZImp z = Phasor.Convert(new Complex(0.2, 0.1));

            double vdropPct = EECircuit.CalcPctVDrop(v, i, z);

            double res = 3.72;
            Assert.True(Checker.EQ(vdropPct, res, 0.1, 0.1));
        }

        #endregion

        #region Impedance

        [Fact]
        public void Calc_impedance_from_voltage_and_current()
        {
            IVoltage e = new Phasor(100, 0);
            ICurrent i = new Phasor(20, -53.1);

            IZImp z = EECircuit.ZImp(e, i);

            IZImp res = Phasor.Convert(new Complex(3, 4));
            Assert.True(Checker.EQ(z.Base, res.Base, 0.1, 0.1));
        }

        [Fact]
        public void Calc_impedance_from_s1_and_vln()
        {
            IVoltageLN e = new Phasor(100, 0);
            IPowerS1 p = new Phasor(2000, 53.1);

            IZImp z = EECircuit.ZImp(p, e);

            IZImp res = Phasor.Convert(new Complex(3, 4));
            Assert.True(Checker.EQ(z.Base, res.Base, 0.1, 0.1));
        }

        [Fact]
        public void Calc_impedance_from_s3_and_vll()
        {
            IVoltageLL e = new Phasor(100, 0);
            IPowerS3 p = new Phasor(2000, 53.1);

            IZImp z = EECircuit.ZImp(p, e);

            IZImp res = Phasor.Convert(new Complex(3, 4));
            Assert.True(Checker.EQ(z.Base, res.Base, 0.1, 0.1));
        }

        #endregion
    }
}
