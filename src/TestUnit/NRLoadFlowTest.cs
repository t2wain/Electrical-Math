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
            var ex = _ctx.NRLoadFlow;
            var c = ex.Calc_PQDelta();
            Assert.True(c, "NR power calculation failed");
        }

        [Fact]
        public void Calc_LoadFlow()
        {
            var ex = _ctx.NRLoadFlow;
            var c = ex.LFSolve();
            Assert.True(c, "Load flow calculation failed");
        }

        [Fact]
        public void Calc_FastDecoupled_LoadFlow()
        {
            var ex = _ctx.NRLoadFlow;
            var c = ex.LFSolve_FastDecoupled();
            Assert.True(c, "Load flow calculation failed");
        }

    }
}
