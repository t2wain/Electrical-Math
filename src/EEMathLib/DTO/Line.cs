using System.Numerics;

namespace EEMathLib.DTO
{
    public struct Line
    {
        public Line(Point fromCoord, Point toCoord)
        {
            FromPoint = fromCoord;
            ToPoint = toCoord;
        }

        public Point FromPoint { get; private set; }
        public Point ToPoint { get; private set; }

        public Vector3 ToVector() =>
            new Vector3(
                ToPoint.X - FromPoint.X,
                ToPoint.Y - FromPoint.Y,
                ToPoint.Z - FromPoint.Z
            );
    }
}
