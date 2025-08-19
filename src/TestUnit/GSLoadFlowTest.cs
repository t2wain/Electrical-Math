using EEMathLib.LoadFlow.GaussSeidel;

namespace TestUnit
{
    public class GSLoadFlowTest : IClassFixture<Context>
    {
        Context _ctx;

        public GSLoadFlowTest(Context ctx)
        {
            _ctx = ctx;
        }

        [Fact]
        public void LoadFlowGS_CalcVoltage() 
        {
            var c = GSExample.CalcVoltage(_ctx.LoadFlowData1);
            Assert.True(c);
        }

        [Fact]
        public void LoadFlowGS_Solve_Data1()
        {
            var c = GSExample.Solve(_ctx.LoadFlowData1, true);
            Assert.True(c, "Load flow calculation failed");
        }

        [Fact]
        public void LoadFlowGS_Solve_Data2_YData()
        {
            var c = GSExample.Solve(_ctx.LoadFlowData2Y, true);
            Assert.True(c, "Load flow calculation failed");
        }
    }
}
