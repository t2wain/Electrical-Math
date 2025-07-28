using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.GaussSiedel;
using EEMathLib.LoadFlow.NR;

namespace TestUnit
{
    public class Context : IDisposable
    {
        public Context()
        {
            LoadFlowData = new LFData();
        }

        public ILFData LoadFlowData { get; private set; }

        public GSExample GSLoadFlow => new GSExample(this.LoadFlowData);

        public NRExample NRLoadFlow => new NRExample(this.LoadFlowData);

        public void Dispose()
        {
            this.LoadFlowData?.Dispose();
            this.LoadFlowData = null;
        }
    }
}
