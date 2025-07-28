using EEMathLib.LoadFlow.Data;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EEMathLib.LoadFlow
{
    public class EENetwork : IDisposable
    {
        ILFData _data;

        public EENetwork(ILFData data)
        {
            this._data = data;
        }

        public IEnumerable<EEBus> Buses => _data.Busses;
        public IEnumerable<EELine> Lines => _data.Lines;
        public Matrix<Complex> YMatrix { get; protected set; }

        /// <summary>
        /// Assign consecutive indices to buses, 
        /// starting from 0 for slack bus.
        /// </summary>
        public void BuildIndex() 
        {
            var qbus = Buses
                .OrderBy(b => b.BusType == BusTypeEnum.Slack ? 0 : 1)
                .Zip(Enumerable.Range(0, this.Buses.Count()),
                    (bus, index) => { bus.BusIndex = index; return bus; })
                .ToList();
        }

        public void AssignBusToLine()
        {
            var d = Buses.ToDictionary(b => b.ID);
            foreach (var l in Lines)
            {
                if (d.TryGetValue(l.FromBusID, out var fromBus))
                {
                    l.FromBus = fromBus;
                }
                if (d.TryGetValue(l.ToBusID, out var toBus))
                {
                    l.ToBus = toBus;
                }
            }
        }

        /// <summary>
        /// Build Y matrix from lines.
        /// </summary>
        public void BuildYImp()
        {
            var N = Buses.Count();
            var Y = Matrix<Complex>.Build.Dense(N, N, Complex.Zero);
            foreach (var l in Lines)
            {
                var i = l.FromBus.BusIndex;
                var j = l.ToBus.BusIndex;
                var zl = (1 / l.ZImp) + (l.YImp / 2);
                var z = 1 / l.ZImp;
                Y[i, i] += zl;
                Y[j, j] += zl;
                Y[i, j] -= z;
                Y[j, i] -= z;
            }
            YMatrix = Y;
        }

        public void Dispose()
        {
            this._data = null;
        }
    }
}
