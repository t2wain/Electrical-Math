namespace TestUnit
{
    public class GSLoadFlowTest : IClassFixture<Context>
    {
        private readonly Context _ctx;

        public GSLoadFlowTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void BuildYMatrix_Partial()
        {
            var ex = _ctx.GSLoadFlow;
            var c = ex.BuildYMatrix_Partial();
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void BuildYMatrix()
        {
            var ex = _ctx.GSLoadFlow;
            var c = ex.BuildYMatrix();
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void LoadFlowGS_CalcVoltage() 
        {
            var ex = _ctx.GSLoadFlow;
            var c = ex.CalcVoltage();
            Assert.True(c);
        }

        [Fact]
        public void LoadFlowGS_Solve()
        {
            var ex = _ctx.GSLoadFlow;
            var c = ex.Solve();
            Assert.True(c, "Load flow calculation failed");
        }
    }
}
