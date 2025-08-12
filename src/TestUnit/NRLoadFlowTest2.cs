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

        #region PQ

        [Fact]
        public void Calc_PQ_Iteration_0()
        {
            var c = NRExample.Calc_PQ(_data, 0);
            Assert.True(c);
        }

        [Fact]
        public void Calc_PQ_Iteration_1()
        {
            var c = NRExample.Calc_PQ(_data, 1);
            Assert.True(c);
        }

        #endregion

        #region Iteration

        [Fact]
        public void Calc_Iteration_1()
        {
            var c = NRExample.LFSolve(_data, 1);
            Assert.True(c);
        }

        [Fact]
        public void Calc_Iteration_2()
        {
            var c = NRExample.LFSolve(_data, 2);
            Assert.True(c);
        }

        [Fact]
        public void Calc_Iteration_3()
        {
            var c = NRExample.LFSolve(_data, 3);
            Assert.True(c);
        }

        #endregion

        #region Load flow

        [Fact]
        public void Calc_LoadFlow()
        {
            var c = NRExample.LFSolve(_data);
            Assert.True(c);
        }

        #endregion

    }
}
