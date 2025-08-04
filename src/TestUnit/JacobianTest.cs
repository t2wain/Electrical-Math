using EEMathLib.LoadFlow.NewtonRaphson;

namespace TestUnit
{
    public class JacobianTest : IClassFixture<Context>
    {
        private Context _ctx;

        public JacobianTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void Calc_J1_Partial()
        {
            var c = NRExample.Calc_J1_Partial(_ctx.LoadFlowData);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_JMatrix()
        {
            var c = NRExample.Calc_JMatrix(_ctx.LoadFlowData);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_Jkk()
        {
            var c = NRExample.Calc_Jkk(_ctx.LoadFlowData, true, false, false, false);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_Jkn()
        {
            var c = NRExample.Calc_Jkn(_ctx.LoadFlowData, true, false, false, false);
            Assert.True(c, "NR JMatrix calculation failed");
        }

    }
}
