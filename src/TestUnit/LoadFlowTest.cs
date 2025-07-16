using EEMathLib.LoadFlow;

namespace TestUnit
{
    public class LoadFlowTest
    {
        [Fact]
        public void Ex1()
        {
            var c = LFExample.Ex1();
            Assert.True(c, "Build Y matrix ex1 failed.");
        }

        [Fact]
        public void Ex2() 
        {
            var c = LFExample.Ex2();
            Assert.True(c);
        }

        [Fact]
        public void Ex3()
        {
            var c = LFExample.Ex3();
            Assert.True(c, "Load flow calculation failed");
        }


    }
}
