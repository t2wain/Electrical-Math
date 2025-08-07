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
        public void Calc_PQ_Iteration_0()
        {
            var c = NRExample.Calc_PQ(_data, 0);
            Assert.True(c);
        }

        [Fact]
        public void Calc_DeltaPQ_Iteration_0()
        {
            var c = NRExample.Calc_DeltaPQ(_data, 0);
            Assert.True(c);
        }

        [Fact]
        public void Calc_LoadFlow()
        {
            var c = NRExample.LFSolve(_data);
            Assert.True(c);
        }

        [Fact]
        public void Iterate_LoadFlow()
        {
            var c = NRExample.LFIterate3times(_data);
            Assert.True(c);
        }

    }
}
