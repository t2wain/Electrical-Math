using EEMathLib.ShortCircuit;

namespace TestUnit
{
    public class SCTest
    {
        [Fact]
        public void Calc_ZMatrix()
        {
            var ex = new SCExample();
            var res = ex.BuildZ1();
            Assert.True(res);
        }
    }
}
