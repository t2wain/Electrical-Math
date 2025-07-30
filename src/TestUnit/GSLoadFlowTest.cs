using EEMathLib.LoadFlow.GaussSiedel;

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
        public void LoadFlowGS_CalcVoltage() 
        {
            var c = GSExample.CalcVoltage(_ctx.LoadFlowData);
            Assert.True(c);
        }

        [Fact]
        public void LoadFlowGS_Solve()
        {
            var c = GSExample.Solve(_ctx.LoadFlowData);
            Assert.True(c, "Load flow calculation failed");
        }
    }
}
