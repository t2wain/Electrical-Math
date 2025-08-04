using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EEMathLib
{
    /// <summary>
    /// Convenience methods for checking results in unit testing
    /// </summary>
    public static class Checker
    {
        public static bool EQ(double x, double y, double err) =>
            Math.Abs(x - y) <= err;
        
        public static bool EQPct(double x, double y, double err) =>
            Math.Abs((x - y) / (y == 0 ? 1 : y)) <= err;

        public static bool EQ(Phasor x, Phasor y, double magerr, double pherr) =>
            EQ(x.Magnitude, y.Magnitude, magerr) && EQ(x.Phase, y.Phase, pherr);

        public static bool EQ(Complex x, Complex y, double reerr, double imerr) =>
            EQ(x.Real, y.Real, reerr) && EQ(x.Imaginary, y.Imaginary, imerr);

        public static (bool Valid, List<(int Row, int Col)> MisMatch) EQ(
            Matrix<double> mx, Matrix<double> res, double err)
        {
            var c = true;
            var lst = new List<(int Row, int Col)>();
            foreach(var row in Enumerable.Range(0, mx.RowCount))
                foreach(var col in Enumerable.Range(0,mx.ColumnCount))
                {
                    var j = mx[row, col];
                    var r = res[row, col];
                    var v = EQ(j, r, err);
                    if (!v)
                        lst.Add((row, col));
                    c = c && v;
                }

            foreach (var i in lst)
            {
                var v = mx[i.Row, i.Col];
                var r = res[i.Row, i.Col];
            }

            return (c, lst);
        }

        public static (bool Valid, List<(int Row, int Col)> MisMatch) EQ(
            Matrix<Complex> mx, Matrix<Complex> res, double reerr, double imerr)
        {
            var c = true;
            var lst = new List<(int Row, int Col)>();
            foreach(var row in Enumerable.Range(0, mx.RowCount))
                foreach(var col in Enumerable.Range(0,mx.ColumnCount))
                {
                    var j = mx[row, col];
                    var r = res[row, col];
                    var v = EQ(j, r, reerr, imerr);
                    if (!v)
                        lst.Add((row, col));
                    c = c && v;
                }


            foreach (var i in lst)
            {
                var v = mx[i.Row, i.Col];
                var r = res[i.Row, i.Col];
            }

            return (c, lst);
        }   

    }
}
