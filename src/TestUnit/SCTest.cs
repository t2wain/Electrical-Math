using EEMathLib.ShortCircuit;

namespace TestUnit
{
    public class SCTest : IClassFixture<Context>
    {
        Context _ctx;

        public SCTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void Calc_ZMatrix()
        {
            var ex = new SCExample();
            var res = ex.BuildZ1();
            Assert.True(res);
        }

        [Fact]
        public void Calc_3Phase_Fault()
        {
            new SCExample().Calc3PhaseFault(_ctx.ZNetwork1); ;
        }

        [Fact]
        public void Calc_3Phase_Fault_Buses_Voltage() 
        {
            new SCExample().Calc3PhaseFaultBusesVoltage(_ctx.ZNetwork1, "1");
        }

        [Fact]
        public void Calc_3Phase_Fault_Current_Flow_From_All_Bus()
        {
            new SCExample().Calc3PhaseFaultCurrentFlowFromAllBus(_ctx.ZNetwork1, "1");
        }
    }
}
