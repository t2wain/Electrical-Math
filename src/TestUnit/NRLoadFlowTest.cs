using EEMathLib.LoadFlow.NR;

namespace TestUnit
{
    public class NRLoadFlowTest : IClassFixture<Context>
    {
        private readonly Context _ctx;

        public NRLoadFlowTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void Calc_PQDelta()
        {
            var c = NRExample.Calc_PQDelta_LFData(_ctx.LoadFlowData);
            Assert.True(c, "NR power calculation failed");
        }

        [Fact]
        public void Calc_LoadFlow()
        {
            var c = NRExample.LFSolve(_ctx.LoadFlowData);
            Assert.True(c, "Load flow calculation failed");
        }

        [Fact]
        public void Calc_FastDecoupled_LoadFlow()
        {
            var c = NRExample.LFSolve_FastDecoupled(_ctx.LoadFlowData);
            Assert.True(c, "Load flow calculation failed");
        }

    }
}
