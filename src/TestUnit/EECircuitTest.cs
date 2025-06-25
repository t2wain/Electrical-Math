using EEMathLib;
using System.Numerics;

namespace TestUnit
{
    public class EECircuitTest
    {
        [Fact]
        public void Calc_power_from_voltage_and_impedance()
        {
            // arrange
            IVoltageLN e = new Phasor(100, 0);
            IZImp z = Phasor.Convert(new Complex(3, 4));

            // act
            ICurrent i = EECircuit.Current(e, z);
            IPowerS1 s = EECircuit.Power(e, i);

            // assert
            IPowerS1 res = new Phasor(2000, 53.1);
            Assert.True(Checker.EQ(s.Base, res.Base, 0.1, 0.1));
        }

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
        public void Calc_example_circuit_1()
        {
            var ex = new CircuitExample();
            ex.Example1();
        }
    }
}
