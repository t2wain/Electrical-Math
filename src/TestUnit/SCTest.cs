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
            new SCExample().Calc3PhaseFaultBusFlowFromAllBus(_ctx.ZNetwork1, "1");
        }

        [Fact]
        public void Calc_3Phase_Fault_Element_Flow () 
        {
            new SCExample().Calc3PhaseFaultElementFlow(_ctx.ZNetwork1, "1");
        }

        [Fact]
        public void Calc_Sym_Components()
        {
            var v = new SCExample().CalcSymVoltage();
            Assert.True(v);
        }

        [Fact]
        public void Calc_Asym_Power()
        {
            var v = new SCExample().CalcAsymPower();
            Assert.True(v);
        }

        [Fact]
        public void Calc_Z_Sequence_Matrices()
        {
            var v = new SCExample().CalcZSeqMatrices();
            Assert.True(v);

        }
    }
}
