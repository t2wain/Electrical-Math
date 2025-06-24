using EEMathLib;
using System.Numerics;

namespace TestUnit
{
    public class PUTest
    {
        [Fact]
        public void Check_example_1()
        {
            #region Zone 1

            ///////////// ZONE 1
            var base1 = new PUBase3P(45e6, 13.8e3);

            // T1 nameplate rating
            var t1sbase = 45e6;
            var t1vbase = 13.8e3;
            var t1base = new PUBase3P(t1sbase, t1vbase);
            IZImp t1zpu_rate = Phasor.Convert(new Complex(0, 0.05));

            // assert base impedance
            Assert.True(Checker.EQ(t1base.Impedance, 4.232, 0.001));

            // convert to base 1
            IZImp t1zpu = t1base.ConvertPU(t1zpu_rate, base1);

            Assert.True(Checker.EQ(t1zpu.Base, t1zpu_rate.Base, 0.1, 0.1));

            #endregion

            #region Zone 2

            ///////////// ZONE 2
            var base2 = new PUBase3P(45e6, 69e3);

            Assert.True(Checker.EQ(base2.Impedance, 105.8, 0.1));

            // line impedance in zone 2
            IZImp l2z = Phasor.Convert(new Complex(5, 15));
            IZImp l2pu = base2.ToPU(l2z);

            IZImp l2res = Phasor.Convert(new Complex(0.047, 0.142));
            Assert.True(Checker.EQ(l2pu.Base, l2res.Base, 0.2, 0.2));

            // T2 nameplate rating
            var t2sbase = 25e6;
            var t2vbase = 69e3;
            var t2base = new PUBase3P(t2sbase, t2vbase);
            IZImp t2zpu_rate = Phasor.Convert(new Complex(0, 0.046));

            // convert to base 2
            IZImp t2zpu = t2base.ConvertPU(t2zpu_rate, base2);

            IZImp t2res = Phasor.Convert(new Complex(0, 0.0828));
            Assert.True(Checker.EQ(t2zpu.Base, t2res.Base, 0.001, 0.001));

            #endregion

            #region Zone 3

            ///////////// ZONE 3
            var base3 = new PUBase3P(45e6, 13.8e3);

            Assert.True(Checker.EQ(base3.Impedance, 4.232, 0.2));

            IZImp ld3z = Phasor.Convert(new Complex(200, 0));
            IZImp ld3zpu = base3.ToPU(ld3z);

            IZImp ld3res = Phasor.Convert(new Complex(47.259, 0));
            Assert.True(Checker.EQ(ld3zpu.Base, ld3res.Base, 0.01, 0.01));

            #endregion

            #region Overall circuit

            //// CIRCUIT

            // calc total impedance
            IZImp zckt_pu = t1zpu.Base + l2pu.Base + t2zpu_rate.Base + ld3zpu.Base;

            // calc circuit current
            IVoltage vckt_pu = new Phasor(1, 0);
            ICurrent ickt_pu = vckt_pu.Base / zckt_pu.Base;

            ICurrent ickt_res = new Phasor(0.02114, -0.3);
            Assert.True(Checker.EQ(ickt_pu.Base, ickt_res.Base, 0.01, 0.1));

            // calc actual current in zone 1
            ICurrent izone1 = base1.ToValue(ickt_pu);
            Assert.True(Checker.EQ(base1.Current, 1883, 0.5));
            Assert.True(Checker.EQ(izone1.Base.Magnitude, 39.8, 0.5));

            // calc actual current in zone 2
            ICurrent izone2 = base2.ToValue(ickt_pu);
            Assert.True(Checker.EQ(base2.Current, 376.5, 0.5));
            Assert.True(Checker.EQ(izone2.Base.Magnitude, 7.96, 0.5));

            // calc actual current in zone 3
            ICurrent izone3 = base3.ToValue(ickt_pu);
            Assert.True(Checker.EQ(base3.Current, 1883, 0.5));
            Assert.True(Checker.EQ(izone3.Base.Magnitude, 39.8, 0.5));

            #endregion
        }
    }
}
