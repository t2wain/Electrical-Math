using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
namespace EEMathLib.LoadFlow
{
    /// <summary>
    /// Test the EENetwork class
    /// </summary>
    public static class NetworkExample
    {

        /// <summary>
        /// Build Y matrix based on dataset LFData
        /// </summary>
        public static bool BuildYMatrix_Partial_LFData(LFData data)
        {
            var nw = data.CreateNetwork();
            var Y = nw.YMatrix;
            var res = MX.ParseMatrix(data.YResult);

            var y25 = new Phasor(19.9195, 95.143);
            var e25 = Phasor.Convert(Y[1, 4]);
            var c = Checker.EQ(y25, e25, 0.0001, 0.001);

            var y24 = new Phasor(9.95972, 95.143);
            var e24 = Phasor.Convert(Y[1, 3]);
            c = c && Checker.EQ(y24, e24, 0.0001, 0.001);

            var y22 = new Phasor(28.5847, -84.624);
            var e22 = Phasor.Convert(Y[1, 1]);
            c = c && Checker.EQ(y22, e22, 0.0001, 0.001);

            return c;
        }

        /// <summary>
        /// Build Y matrix
        /// </summary>
        public static bool BuildYMatrix(ILFData data)
        {
            var nw = data.CreateNetwork();
            var Y = nw.YMatrix;
            var res = MX.ParseMatrix(data.YResult);
            var v = Checker.EQ(Y, res, 0.1, 0.1);
            return v.Valid;
        }

    }
}
