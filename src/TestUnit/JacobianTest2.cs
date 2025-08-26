using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.NewtonRaphson.JacobianMX;

namespace TestUnit
{
    /// <summary>
    /// Tests based on sample data LFData2
    /// </summary>
    public class JacobianTest2 : IClassFixture<Context>
    {
        private LFData2 _data;

        public JacobianTest2(Context ctx)
        {
            _data = ctx.LoadFlowData2;
        }

        [Fact]
        public void Calc_J1_J2_J3_J4()
        {
            var c = JCExample.Calc_J1_J2_J3_J4(_data, true, true, true, true, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }
        
        [Fact]
        public void Calc_Jkk()
        {
            var c = JCExample.Calc_Jkk(_data, true, true, true, true, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_Jkn()
        {
            var c = JCExample.Calc_Jkn(_data, true, true, true, true, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_JMatrix()
        {
            var c = JCExample.Calc_JMatrix(_data, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }


    }
}
