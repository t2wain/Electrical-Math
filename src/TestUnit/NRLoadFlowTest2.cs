using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.NewtonRaphson;

namespace TestUnit
{
    public class NRLoadFlowTest2 : IClassFixture<Context>
    {
        LFData2 _data2;
        Context _ctx;

        public NRLoadFlowTest2(Context ctx)
        {
            _data2 = ctx.LoadFlowData2;
            _ctx = ctx; 
        }

        [Fact]
        public void Calc_LoadFlow()
        {
            var c = NRExample.LFSolve(_data2, false);
            Assert.True(c);
        }

        [Fact]
        public void Calc_LoadFlow_YData()
        {
            var data2Y = _ctx.LoadFlowData2Y;
            var c = NRExample.LFSolve(data2Y, true);
            Assert.True(c);
        }

        [Fact]
        public void Calc_FastDecoupled_LoadFlow()
        {
            var c = NRExample.LFSolve_FastDecoupled(_data2);
            Assert.True(c);
        }

        [Fact]
        public void Calc_FastDecoupled_LoadFlow_Approximate()
        {
            var c = NRExample.LFSolve_FastDecoupled_Approximation(_data2);
            Assert.True(c);
        }

        [Fact]
        public void Calc_FastDecoupled_DCLike_LoadFlow()
        {
            var c = NRExample.LFSolve_DCLike(_data2);
            Assert.True(c);
        }
    }
}
