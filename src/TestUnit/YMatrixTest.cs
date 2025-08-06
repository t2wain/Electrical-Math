using EEMathLib.LoadFlow;
using EEMathLib.LoadFlow.Data;

namespace TestUnit
{
    public class YMatrixTest : IClassFixture<Context>
    {
        LFData _data;
        LFData2 _data2;

        public YMatrixTest(Context ctx)
        {
            _data = ctx.LoadFlowData;
            _data2 = ctx.LoadFlowData2;
        }

        [Fact]
        public void BuildYMatrix_Partial()
        {
            var c = NetworkExample.BuildYMatrix_Partial_LFData(_data);
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void BuildYMatrix()
        {
            var c = NetworkExample.BuildYMatrix(_data);
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void BuildYMatrix2()
        {
            var c = NetworkExample.BuildYMatrix(_data2);
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

    }
}
