using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.NewtonRaphson;

namespace TestUnit
{
    public class NRLoadFlowDebug : IClassFixture<Context>
    {
        Context _ctx;
        LFData2 _data2;

        public NRLoadFlowDebug(Context ctx)
        {
            this._ctx = ctx;
            _data2 = ctx.LoadFlowData2;
        }


        #region PQ

        [Fact]
        public void Calc_PQ_Iteration_0()
        {
            var c = NRExample.Calc_Iteration(_data2, 0);
            Assert.True(c);
        }

        [Fact]
        public void Calc_PQ_Iteration_1()
        {
            var c = NRExample.Calc_Iteration(_data2, 1);
            Assert.True(c);
        }

        #endregion

        #region Iteration

        [Fact]
        public void Calc_Iteration_1()
        {
            var c = NRExample.Calc_Iteration(_data2, 1);
            Assert.True(c);
        }

        [Fact]
        public void Calc_Iteration_2()
        {
            var c = NRExample.Calc_Iteration(_data2, 2);
            Assert.True(c);
        }

        [Fact]
        public void Calc_Iteration_3()
        {
            var c = NRExample.Calc_Iteration(_data2, 3);
            Assert.True(c);
        }

        #endregion

    }
}
