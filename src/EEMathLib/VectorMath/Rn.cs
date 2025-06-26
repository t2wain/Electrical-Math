using System;
using System.Numerics;

namespace EEMathLib.VectorMath
{
    /// <summary>
    /// Operations for vector of n dimensions 
    /// </summary>
    public abstract class Rn
    {
        public static double DotProduct(Vector<double> v1, Vector<double> v2) =>
            Vector.Dot(v1, v2);

        /// <summary>
        /// Length or norm of a vector
        /// </summary>
        public static double Length(Vector<double> v) =>
            Math.Sqrt(Vector.Dot(v, v));

        /// <summary>
        /// Unit length vector
        /// </summary>
        public static Vector<double> Normalize(Vector<double> v) =>
            v * (1 / Length(v));

        /// <summary>
        /// Angles between two vectors
        /// </summary>
        public static double Angle(Vector<double> v1, Vector<double> v2) =>
            Phasor.ConvertRadianToDegree(
                Math.Acos(
                    Vector.Dot(v1, v2) / (Length(v1) * Length(v2))
                ));

    }
}
