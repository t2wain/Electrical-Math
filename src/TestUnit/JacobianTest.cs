namespace TestUnit
{
    public class JacobianTest : IClassFixture<Context>
    {
        private readonly Context _ctx;

        public JacobianTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void Calc_J1()
        {
            var ex = _ctx.NRLoadFlow;
            var c = ex.Calc_J1();
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_JMatrix()
        {
            var ex = _ctx.NRLoadFlow;
            var c = ex.Calc_JMatrix();
            Assert.True(c, "NR JMatrix calculation failed");
        }

    }
}
