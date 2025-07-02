using EEMathLib;
using System.Numerics;

namespace TestUnit
{
    public class PhasorTest
    {
        [Fact]
        public void Should_convert_degree_to_radian()
        {
            // arrange
            var deg = 30 + 360;

            // act
            var rad = Phasor.ConvertDegreeToRadian(deg);

            // assert
            var res = 0.523599;
            Assert.True(Checker.EQ(rad, res, 0.00001));
        }

        [Fact]
        public void Should_convert_radian_to_degree()
        {
            // arrange
            var rad = 0.523599 + 2 * Math.PI;

            // act
            var deg = Phasor.ConvertRadianToDegree(rad);

            // assert
            var res = 30.0;
            Assert.True(Checker.EQ(deg, res, 0.1));
        }

        [Fact]
        public void Should_convert_radian_to_degree_2()
        {
            // arrange
            var rad = 3.5 * Math.PI;

            // act
            var deg = Phasor.ConvertRadianToDegree(rad);

            // assert
            var res = -90.0;
            Assert.True(Checker.EQ(deg, res, 0.1));
        }

        [Fact]
        public void Convert_Phasor_to_Complex()
        {
            // arrange
            var p = new Phasor(100, 30);

            // act
            var c = p.ToComplex();

            // assert
            var res = new Complex(86.6, 50);
            Assert.True(Checker.EQ(c, res, 0.1, 0.1));
        }

        [Fact]
        public void Phasor_Division()
        {
            // arrange
            var e = new Phasor(100, 0);
            var z = Phasor.Convert(new Complex(3, 4));

            // act
            var i = e / z;

            // assert
            var res = new Phasor(20, -53.1);
            Assert.True(Checker.EQ(i, res, 0.1, 0.1));
        }

        [Fact]
        public void Phasor_Reciprocal()
        {
            // arrange
            var z = Phasor.Convert(new Complex(3, 4));

            // act
            var y = 1 / z;

            // assert
            var res = Phasor.Convert(new Complex(3, -4) / 25);
            Assert.True(Checker.EQ(y, res, 0.1, 0.1));
        }

        [Fact]
        public void Phasor_Addition()
        {
            var i1 = new Phasor(40, 20);
            var i2 = new Phasor(30, -65);

            var i3 = i1 + i2;

            var res = new Phasor(52.05, -15);
            Assert.True(Checker.EQ(i3, res, 0.1, 0.1));
        }

        [Fact]
        public void Phasor_Subtraction()
        {
            var i1 = new Phasor(2, 20);
            var i2 = new Phasor(6, 30);

            var i3 = i2 - i1;

            var res = new Phasor(4.045, 34.9);
            Assert.True(Checker.EQ(i3, res, 0.1, 0.1));
        }

        [Fact]
        public void Phasor_Multiplication()
        {
            var p1 = new Phasor(5, 45);
            var p2 = new Phasor(4, -20);

            var p3 = p1 * p2;

            var res = new Phasor(20, 25);
            Assert.True(Checker.EQ(p3, res, 0.1, 0.1));
        }

        [Fact]
        public void Tuple_Implicit_Conversion()
        {
            (double Magnitude, double Phase) value = (1.0, 0.0);
            Phasor p = value;
            Assert.True(Checker.EQ(value.Magnitude, p.Magnitude, 0.01));
            Assert.True(Checker.EQ(value.Phase, p.Phase, 0.01));

            var value2 = (1.0, 0.0);
            p = value2;
            Assert.True(Checker.EQ(value2.Item1, p.Magnitude, 0.01));
            Assert.True(Checker.EQ(value2.Item2, p.Phase, 0.01));
        }

        [Fact]
        public void Complex_Implicit_Conversion()
        {
            var v = new Complex(1, 0);
            Phasor p = v;

            Assert.True(Checker.EQ(v.Magnitude, p.Magnitude, 0.01));
            Assert.True(Checker.EQ(v.Phase, p.Phase, 0.01));
        }
    }
}