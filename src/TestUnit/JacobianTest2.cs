using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.NewtonRaphson;

namespace TestUnit
{
    /// <summary>
    /// Tests based on sample data LFData2
    /// </summary>
    public class JacobianTest2 : IClassFixture<Context>
    {
        private LFData2 _data;

        public JacobianTest2(Context ctx)
        {
            _data = ctx.LoadFlowData2;
        }

        #region J1, J2, J3, J4

        [Fact]
        public void Calc_J1()
        {
            var c = NRExample.Calc_JMatrix(_data, true, false, false, false, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_J2()
        {
            var c = NRExample.Calc_JMatrix(_data, false, true, false, false, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_J3()
        {
            var c = NRExample.Calc_JMatrix(_data, false, false, true, false, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_J4()
        {
            var c = NRExample.Calc_JMatrix(_data, false, false, false, true, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        #endregion

        
        [Fact]
        public void Calc_Jkk()
        {
            var c = NRExample.Calc_Jkk(_data, true, true, true, true, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_Jkn()
        {
            var c = NRExample.Calc_Jkn(_data, true, true, true, true, 1);
            Assert.True(c, "NR JMatrix calculation failed");
        }


    }
}
