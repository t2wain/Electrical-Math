using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.NewtonRaphson;

namespace TestUnit
{
    /// <summary>
    /// Tests based on sample data LFData
    /// </summary>
    public class JacobianTest : IClassFixture<Context>
    {
        private LFData _data;

        public JacobianTest(Context ctx)
        {
            _data = ctx.LoadFlowData;
        }

        [Fact]
        public void Calc_J1_Partial()
        {
            var c = NRExample.Calc_J1_Partial(_data);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_J1_J2_J3_J4()
        {
            var c = NRExample.Calc_J1_J2_J3_J4(_data, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_Jkk()
        {
            var c = NRExample.Calc_Jkk(_data, true, false, false, false, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_Jkn()
        {
            var c = NRExample.Calc_Jkn(_data, true, false, false, false, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

    }
}
