using EEMathLib.LoadFlow;

namespace TestUnit
{
    public class Context
    {
        public Context()
        {
            LoadFlowData = new LFData();
        }

        public ILFData LoadFlowData { get; private set; }

        public GSExample GSLoadFlow => new GSExample(this.LoadFlowData);

        public NRExample NRLoadFlow => new NRExample(this.LoadFlowData);
    }
}
