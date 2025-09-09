using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EEMathLib.LoadFlow
{
    public class LFNetwork : ENetwork, IDisposable
    {
        ILFData _data;

        public LFNetwork(ILFData data)
        {
            this._data = data;
        }

        public IEnumerable<EEBus> EBuses => _data.Busses;
        public IEnumerable<EELine> ELines => _data.Lines;

        /// <summary>
        /// Assign consecutive indices to buses, 
        /// starting from 0 for slack bus.
        /// </summary>
        public void BuildIndex() 
        {
            var qbus = EBuses
                .OrderBy(b => b.BusType == BusTypeEnum.Slack ? 0 : 1)
                .Zip(Enumerable.Range(0, this.EBuses.Count()),
                    (bus, index) => { bus.BusIndex = index; return bus; })
                .ToList();
        }

        public void AssignBusToLine()
        {
            var d = EBuses.ToDictionary(b => b.ID);
            foreach (var l in ELines)
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
                var yl = (1 / l.ZSeries) + (l.YShunt / 2);

                var yii = Y[i, i];
                var yjj = Y[j, j];
                var yij = Y[i, j];
                var yji = Y[i, j];

                var y = 1 / l.ZSeries;
                Y[i, i] += yl;
                Y[j, j] += yl;
                Y[i, j] -= y;
                Y[j, i] -= y;
            }
            YMatrix = Y;
        }


        public override IEnumerable<IEBus> Buses => EBuses;
        public override IEnumerable<IELine> Lines => ELines;

        public void Dispose()
        {
            this._data = null;
        }
    }
}
