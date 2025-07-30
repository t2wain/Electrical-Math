using EEMathLib.LoadFlow.Data;

namespace TestUnit
{
    public class Context : IDisposable
    {
        LFData? _data1 = null;
        public LFData LoadFlowData 
        { 
            get
            {
                if (_data1 == null)
                    _data1 = new LFData();
                return _data1;
            } 
        }

        LFData2? _data2 = null;
        public LFData2 LoadFlowData2 
        { 
            get
            {
                if (_data2 == null)
                    _data2 = new LFData2();
                return _data2;
            }
        }

        public void Dispose()
        {
            this._data1?.Dispose();
            this._data1 = null;

            this._data2?.Dispose();
            this._data2 = null;
        }
    }
}
