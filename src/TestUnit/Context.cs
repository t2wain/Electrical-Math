using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;

namespace TestUnit
{
    public class Context : IDisposable
    {
        LFData? _data1 = null;
        public LFData LoadFlowData1
        { 
            get
            {
                if (_data1 == null)
                    _data1 = new LFData();
                return _data1;
            } 
        }

        LFData? _data1Y = null;
        public LFData LoadFlowData1Y
        {
            get
            {
                if (_data1Y == null)
                {
                    _data1Y = new LFData();
                    var nw = _data1Y.CreateNetwork();
                    nw.YMatrix = MX.ParseMatrix(_data1Y.YResult);
                }
                return _data1Y;
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

        LFData2? _data2y = null;
        public LFData2 LoadFlowData2Y
        {
            get
            {
                if (_data2y == null)
                {
                    _data2y = new LFData2();
                    var nw = _data2y.CreateNetwork();
                    nw.YMatrix = MX.ParseMatrix(_data2y.YResult);
                }
                return _data2y;
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
