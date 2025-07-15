using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EEMathLib.LoadFlow
{
    public class EENetwork
    {
        public EENetwork(IEnumerable<EEBus> buses, IEnumerable<EELine> lines)
        {
            this.Buses = new List<EEBus>(buses);
            this.Lines = new List<EELine>(lines);
            this.BuildIndex();
            this.BuildYImp();
        }

        public IEnumerable<EEBus> Buses { get; protected set; }
        public IEnumerable<EELine> Lines { get; protected set; }
        public Matrix<Complex> YMatrix { get; protected set; }

        protected void BuildIndex() 
        {
            var qbus = Buses
                .OrderBy(b => b.BusType == BusTypeEnum.Slack ? 0 : 1)
                .Zip(Enumerable.Range(0, this.Buses.Count()),
                    (bus, index) => { bus.BusIndex = index; return bus; })
                .ToList();
        }

        protected void BuildYImp()
        {
            var N = Buses.Count();
            var Y = Matrix<Complex>.Build.Dense(N, N, Complex.Zero);
            foreach (var l in Lines)
            {
                var i = l.FromBus.BusIndex;
                var j = l.ToBus.BusIndex;
                var zl = 1 / l.ZImp + l.YImp;
                Y[i, i] += zl;
                Y[j, j] += zl;
                Y[i, j] -= zl;
                Y[j, i] -= zl;
            }
            YMatrix = Y;
        }

    }
}
