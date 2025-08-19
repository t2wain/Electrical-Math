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
        public void Calc_PQDelta_Partial_LFData()
        {
            var c = NRExample.Calc_PQDelta_Partial(_ctx.LoadFlowData1);
            Assert.True(c);
        }

        [Fact]
        public void Calc_LoadFlow()
        {
            var c = NRExample.LFSolve(_ctx.LoadFlowData1Y, true);
            Assert.True(c);
        }

    }
}
