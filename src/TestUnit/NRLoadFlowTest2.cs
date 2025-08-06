using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.NewtonRaphson;

namespace TestUnit
{
    public class NRLoadFlowTest2 : IClassFixture<Context>
    {
        LFData2 _data;

        public NRLoadFlowTest2(Context ctx)
        {
            _data = ctx.LoadFlowData2;
        }

        [Fact]
        public void Calc_PQ_LFData2_Iteration_0()
        {
            var c = NRExample.Calc_PQ(_data, 0);
            Assert.True(c);
        }

        [Fact]
        public void Calc_LoadFlow()
        {
            var c = NRExample.LFSolve(_data);
            Assert.True(c);
        }

    }
}
