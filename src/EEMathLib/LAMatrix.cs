using MathNet.Numerics.LinearAlgebra;
using System;

namespace EEMathLib
{
    public abstract class LAMatrix
    {
        public double DotProduct(Vector<double> v1,  Vector<double> v2) =>
            v1.Count == v2.Count ? v1.DotProduct(v2) : throw new ArgumentException("Mismatch dimension");
    }
}
