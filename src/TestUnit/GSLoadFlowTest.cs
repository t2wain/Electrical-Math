using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.GaussSeidel;

namespace TestUnit
{
    public class GSLoadFlowTest : IClassFixture<Context>
    {
        LFData _data;

        public GSLoadFlowTest(Context ctx)
        {
            _data = ctx.LoadFlowData;
        }

        [Fact]
        public void LoadFlowGS_CalcVoltage() 
        {
            var c = GSExample.CalcVoltage(_data);
            Assert.True(c);
        }

        [Fact]
        public void LoadFlowGS_Solve()
        {
            var c = GSExample.Solve(_data);
            Assert.True(c, "Load flow calculation failed");
        }
    }
}
