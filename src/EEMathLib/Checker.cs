using System;
using System.Numerics;

namespace EEMathLib
{
    public static class Checker
    {
        public static bool EQ(double x, double y, double err) =>
            Math.Abs(x - y) <= err;

        public static bool EQ(Phasor x, Phasor y, double magerr, double pherr) =>
            EQ(x.Magnitude, y.Magnitude, magerr) && EQ(x.Phase, y.Phase, pherr);

        public static bool EQ(Complex x, Complex y, double reerr, double imerr) =>
            EQ(x.Real, y.Real, reerr) && EQ(x.Imaginary, y.Imaginary, imerr);
    }
}
