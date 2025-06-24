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
    }
}