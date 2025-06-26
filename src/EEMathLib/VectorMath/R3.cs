using System;
using System.Numerics;

namespace EEMathLib.VectorMath
{
    /// <summary>
    /// Operations for vectors in 3 dimensions
    /// </summary>
    public abstract class R3
    {
        #region One vector operations

        /// <summary>
        /// Length or norm of a vector
        /// </summary>
        public static double Length(Vector3 v) => v.Length();

        /// <summary>
        /// Unit length vector
        /// </summary>
        public static Vector3 Normalize(Vector3 v) => 
            Vector3.Normalize(v);

        #endregion

        #region Two vectors operations

        public static double DotProduct(Vector3 v1, Vector3 v2) => 
            Vector3.Dot(v1, v2);

        /// <summary>
        /// Cross product is valid only in 3D. Return a vector
        /// orthogonal to both input vectors.
        /// </summary>
        public static Vector3 CrossProduct(Vector3 v1, Vector3 v2) => 
            Vector3.Cross(v1, v2);
        
        /// <summary>
        /// Angles between two vectors
        /// </summary>
        public static double Angle(Vector3 v1, Vector3 v2) =>
            Phasor.ConvertRadianToDegree(
                Math.Acos(
                    Vector3.Dot(v1, v2) / (v1.Length() * v2.Length())
                ));

        /// <summary>
        /// The dot product of two orthogonal vectors is zero.
        /// </summary>
        public static bool IsOrthogonal(Vector3 v1, Vector3 v2) =>
            Vector3.Dot(v1, v2) <= 1e-6;

        /// <summary>
        /// Projection of v onto u
        /// </summary>
        /// <returns>Multiplying value of vector u</returns>
        public static double Projection(Vector3 u, Vector3 v) =>
            Vector3.Dot(u, v) / Vector3.Dot(u, u);

        /// <summary>
        /// Projection of v onto u
        /// </summary>
        /// <returns>Projected vector parallel to vector u</returns>
        public static Vector3 Projection2(Vector3 u, Vector3 v) =>
            u * (float)Projection(u, v);

        #endregion

        #region Standard unit vectors

        /// <summary>
        /// A valid vector with all zero component 
        /// </summary>
        public static Vector3 ZeroVector => Vector3.Zero;

        /// <summary>
        /// Standard unit vector X
        /// </summary>
        public static Vector3 XAxis => Vector3.UnitX;

        /// <summary>
        /// Standard unit vector Y
        /// </summary>
        public static Vector3 YAxis => Vector3.UnitY;

        /// <summary>
        /// Standard unit vector Z
        /// </summary>
        public static Vector3 ZAxis => Vector3.UnitZ;

        #endregion
    }
}
