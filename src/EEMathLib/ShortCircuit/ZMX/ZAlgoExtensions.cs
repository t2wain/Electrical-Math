using EEMathLib.DTO;
using EEMathLib.ShortCircuit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit.ZMX
{
    public static class ZAlgoExtensions
    {
        /// <summary>
        /// Create elements from a given network 
        /// to build impedance matrix
        /// </summary>
        public static ZNetwork BuildZElements(this IENetwork nw, ZTypeEnum ztype)
        {

            var dBus = nw.Buses
                .Select(b => new ZBus { ID = b.ID, Data = b })
                .Cast<IZBus>()
                .ToDictionary(b => b.ID);

            var lstEl = new List<EZElement>();

            var lines = nw.Lines
                .Select(l => new EZElement
                {
                    ID = l.ID,
                    Data = l,
                    FromBus = dBus[l.FromBus.ID],
                    ToBus = dBus[l.ToBus.ID],
                    Z = l.ZSeries
                });
            lstEl.AddRange(lines);

            var xfs = nw.Transformers
                .Select(t => new EZElement
                {
                    ID = t.ID,
                    Data = t,
                    FromBus = dBus[t.FromBus.ID],
                    ToBus = dBus[t.ToBus.ID],
                    Z = new Complex(0, t.X)
                });
            lstEl.AddRange(xfs);

            if (nw.Generators.Count() > 0 && ztype != ZTypeEnum.SteadyState)
            {
                var gens = nw.Generators
                    .Select(g => new EZElement
                    {
                        ID = g.ID,
                        Data = g,
                        ToBus = dBus[g.Bus.ID],
                        Z = new Complex(0, ztype == ZTypeEnum.Transient ? g.Xp : g.Xpp)
                    });
                lstEl.AddRange(gens);
            }

            if (nw.Loads.Count() > 0 && ztype == ZTypeEnum.Subtransient) 
            {
                var loads = nw.Loads
                    .Select(l => new EZElement
                    {
                        ID = l.ID,
                        Data = l,
                        ToBus = dBus[l.Bus.ID],
                        Z = new Complex(0, l.Xpp)
                    });
                lstEl.AddRange(loads);
            }

            var N = nw.Buses.Count();
            var zres = new ZNetwork
            {
                Buses = dBus,
                Elements = lstEl.Cast<IEZElement>().ToDictionary(e => e.Data.ID),
            };

            return zres;
        }

        public static void BuildZMatrix(this ZNetwork nw)
        {
            var N = nw.Buses.Count();
            nw.Z = MC.Build.Dense(N, N, Complex.Zero);

            var dBus = new Dictionary<string, IZBus>();
            var dElement = nw.Elements.Values.ToDictionary(e => e.ID);
            nw.RemainElements = dElement;
            nw.ExistBuses = dBus;

            // case 1 : between ref bus to new bus
            nw.AddElementsRefToNewBus();

            // case 2 : between new bus and existing bus
            nw.AddElementsNewToExistBus();

            // case 3 : between ref bus to exist bus
            nw.AddElementsRefToExistBus();

            // case 4 : between new bus and existing bus
            nw.AddElementsExistToExistBus();

            if (dElement.Count > 0)
                throw new Exception();
        }

        #region AddElementsRefToNewBus

        public static void AddElementsRefToNewBus(this ZNetwork nw)
        {
            var q = nw.RemainElements.Values
                .Where(e => nw.ValidateAddElementRefToNewBus(e))
                .ToList();
            foreach (var el in q)
            {
                var valid = nw.ValidateAddElementRefToNewBus(el);
                if (valid)
                {
                    nw.AddElementRefToNewBus(el);
                    nw.TrackAddElementRefToNewBus(el);
                }
            }
        }

        public static void AddElementRefToNewBus(this ZNetwork nw, IEZElement element)
        {
            var bus = element.ToBus;
            bus.BusIndex = nw.NextBusIndex;
            var p = bus.BusIndex;
            nw.Z[p, p] = element.Z;
        }

        #endregion

        #region AddElementsNewToExistBus

        public static void AddElementsNewToExistBus(this ZNetwork nw)
        {
            var q = nw.RemainElements.Values
                .Where(e => nw.ValidateAddElementNewToExistBus(e))
                .ToList();
            foreach (var el in q)
            {
                var valid = nw.ValidateAddElementNewToExistBus(el);
                if (valid)
                {
                    var fb = el.FromBus;
                    var tb = el.ToBus;
                    var existBus = nw.ExistBuses.ContainsKey(fb.ID) ? fb : tb;
                    var newBus = !nw.ExistBuses.ContainsKey(fb.ID) ? fb : tb;
                    nw.AddElementNewToExistBus(el, newBus, existBus);
                    nw.TrackAddElementNewToExistBus(el, newBus);
                }
            }
        }

        public static void AddElementNewToExistBus(this ZNetwork nw, IEZElement element, IZBus newBus, IZBus existBus)
        {
            var p = existBus.BusIndex;
            newBus.BusIndex = nw.NextBusIndex;
            var q = newBus.BusIndex;

            var zpq = nw.Z[p, q];
            nw.Z[q, q] = zpq + element.Z;
            foreach(var i in Enumerable.Range(0, nw.LastBusIndex))
            {
                nw.Z[q, i] = nw.Z[p, i];
                nw.Z[i, q] = nw.Z[i, p];    
            }
        }

        #endregion

        #region AddElementsRefToExistBus

        public static void AddElementsRefToExistBus(this ZNetwork nw)
        {
            var q = nw.RemainElements.Values
                .Where(e => nw.ValidateAddElementRefToExistBus(e))
                .ToList();
            foreach (var el in q)
            {
                var valid = nw.ValidateAddElementRefToExistBus(el);
                if (valid)
                {
                    nw.AddElementRefToExistBus(el);
                    nw.TrackAddElementRefToExistBus(el);
                }
            }
        }

        public static bool AddElementRefToExistBus(this ZNetwork nw, IEZElement element)
        {
            var bus = element.ToBus;
            if (element.FromBus != null || !nw.ExistBuses.ContainsKey(bus.ID))
                return false;

            var q = element.ToBus.BusIndex;
            var dz = MC.Build.Dense(nw.LastBusIndex, 1, Complex.Zero);
            foreach (var i in Enumerable.Range(0, nw.LastBusIndex))
            {
                dz[i, 0] = -nw.Z[i, q];
            }
            var zll = nw.Z[q, q] + element.Z;
            var md = dz * dz.Transpose() / zll;

            foreach(var i in Enumerable.Range(0, md.RowCount))
                foreach(var j in Enumerable.Range(0, md.ColumnCount))
                    nw.Z[i, j] -= md[i, j];

            nw.RemainElements.Remove(element.ID);
            return true;
        }

        #endregion

        #region AddElementsExistToExistBus

        public static void AddElementsExistToExistBus(this ZNetwork nw)
        {
            var q = nw.RemainElements.Values
                .Where(e => nw.ValidateAddElementExistToExistBus(e))
                .ToList();
            foreach (var el in q)
            {
                var valid = nw.ValidateAddElementExistToExistBus(el);
                if (valid)
                {
                    nw.AddElementExistToExistBus(el);
                    nw.TrackAddElementExistToExistBus(el);
                }
            }
        }

        public static void AddElementExistToExistBus(this ZNetwork nw, IEZElement element)
        {
            var p = element.FromBus.BusIndex;
            var q = element.ToBus.BusIndex;
            var dz = MC.Build.Dense(nw.LastBusIndex, 1, Complex.Zero);
            foreach (var i in Enumerable.Range(0, nw.LastBusIndex))
            {
                dz[i, 0] = nw.Z[i, q] - nw.Z[i, p];
            }
            var zll = element.Z + nw.Z[p, p] + nw.Z[q, q] - 2 * nw.Z[p, q];
            var md = dz * dz.Transpose() / zll;

            foreach (var i in Enumerable.Range(0, md.RowCount))
                foreach (var j in Enumerable.Range(0, md.ColumnCount))
                    nw.Z[i, j] -= md[i, j];
        }

        #endregion

        #region Y Matrix

        public static void BuildZFromYMatrix(this ZNetwork nw)
        {
            var y = nw.BuildYImp();
            var z = y.Inverse();
            nw.Z = z;
        }

        /// <summary>
        /// Build Y matrix.
        /// </summary>
        public static MC BuildYImp(this ZNetwork nw)
        {
            var N = nw.Buses.Count();
            var Y = MC.Build.Dense(N, N, Complex.Zero);

            foreach (var b in nw.Buses.Values)
                b.BusIndex = b.Data.BusIndex;

            foreach (var l in nw.Elements.Values)
            {
                var y = 1 / l.Z;
                var j = l.ToBus.BusIndex;
                Y[j, j] += y;
                if (l.FromBus is IZBus fb)
                {
                    var i = fb.BusIndex;
                    Y[i, i] += y;
                    Y[i, j] -= y;
                    Y[j, i] -= y;
                }
            }
            return Y;
        }

        #endregion
    }
}
