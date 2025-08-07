using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.NewtonRaphson;

namespace TestUnit
{
    public class NRLoadFlowTest : IClassFixture<Context>
    {
        Context _ctx;

        public NRLoadFlowTest(Context ctx)
        {
            this._ctx = ctx;
        }


        [Fact]
        public void Calc_PQDelta_Partial_LFData()
        {
            var c = NRExample.Calc_PQDelta_Partial(_ctx.LoadFlowData);
            Assert.True(c);
        }

        [Fact]
        public void Calc_LoadFlow()
        {
            var c = NRExample.LFSolve(_ctx.LoadFlowData);
            Assert.True(c);
        }

        //[Fact]
        //public void Calc_FastDecoupled_LoadFlow()
        //{
        //    var c = NRExample.LFSolve_FastDecoupled(_ctx.LoadFlowData);
        //    Assert.True(c);
        //}

    }
}
