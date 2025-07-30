using EEMathLib.LoadFlow;

namespace TestUnit
{
    public class LFNetworkTest : IClassFixture<Context>
    {
        Context _ctx;

        public LFNetworkTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void BuildYMatrix_Partial()
        {
            var c = NetworkExample.BuildYMatrix_Partial_LFData(_ctx.LoadFlowData);
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void BuildYMatrix()
        {
            var c = NetworkExample.BuildYMatrix(_ctx.LoadFlowData);
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void BuildYMatrix2()
        {
            var c = NetworkExample.BuildYMatrix(_ctx.LoadFlowData2);
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

    }
}
