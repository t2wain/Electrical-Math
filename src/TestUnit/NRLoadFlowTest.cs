using EEMathLib.LoadFlow.NewtonRaphson;

namespace TestUnit
{
    public class NRLoadFlowTest : IClassFixture<Context>
    {
        Context _ctx;

        public NRLoadFlowTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void Calc_LoadFlow()
        {
            var c = NRExample.LFSolve(_ctx.LoadFlowData1Y, 0, true);
            Assert.True(c);
        }

        [Fact]
        public void Calc_LoadFlow_V3()
        {
            var c = NRExample.LFSolve(_ctx.LoadFlowData1Y, 3, false);
            Assert.True(c);
        }

    }
}
