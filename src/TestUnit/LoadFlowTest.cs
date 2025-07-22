using EEMathLib.LoadFlow;

namespace TestUnit
{
    public class LoadFlowTest
    {
        [Fact]
        public void BuildYMatrix_Partial()
        {
            var c = LFExample.BuildYMatrix_Partial();
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void BuildYMatrix()
        {
            var c = LFExample.BuildYMatrix();
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void LoadFlowGS_CalcVoltage() 
        {
            var c = LFExample.LoadFlowGS_CalcVoltage();
            Assert.True(c);
        }

        [Fact]
        public void LoadFlowGS_Solve()
        {
            var c = LFExample.LoadFlowGS_Solve();
            Assert.True(c, "Load flow calculation failed");
        }

        [Fact]
        public void LoadFlowNR_Power_J1()
        {
            var c = LFExample.LoadFlowNR_Power_J1();
            Assert.True(c, "NR power calculation failed");
        }

        [Fact]
        public void LoadFlowNR_Power_JMatrix()
        {
            var c = LFExample.LoadFlowNR_JMatrix();
            Assert.True(c, "NR JMatrix calculation failed");
        }

        [Fact]
        public void LoadFlowNR_Solve()
        {
            var c = LFExample.LoadFlowNR_Solve();
            Assert.True(c, "Load flow calculation failed");
        }


    }
}
