using EEMathLib.LoadFlow.NR;

namespace TestUnit
{
    public class JacobianTest2 : IClassFixture<Context>
    {
        private Context _ctx;

        public JacobianTest2(Context ctx)
        {
            this._ctx = ctx;
        }

        #region J1, J2, J3, J4

        [Fact]
        public void Calc_J1()
        {
            var c = NRExample.Calc_JMatrix(_ctx.LoadFlowData2, true, false, false, false);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_J2()
        {
            var c = NRExample.Calc_JMatrix(_ctx.LoadFlowData2, false, true, false, false);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_J3()
        {
            var c = NRExample.Calc_JMatrix(_ctx.LoadFlowData2, false, false, true, false);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_J4()
        {
            var c = NRExample.Calc_JMatrix(_ctx.LoadFlowData2, false, false, false, true);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        #endregion

        
        [Fact]
        public void Calc_Jkk()
        {
            var c = NRExample.Calc_Jkk(_ctx.LoadFlowData2, true, true, true, true);
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void Calc_Jkn()
        {
            var c = NRExample.Calc_Jkn(_ctx.LoadFlowData2, true, false, false, false);
            Assert.True(c, "NR JMatrix calculation failed");
        }


    }
}
