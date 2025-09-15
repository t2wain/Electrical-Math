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
        #region Initialize

        /// <summary>
        /// Prepare the Z impedance data structure contains all buses and
        /// all impedance elements based on a given network.
        /// </summary>
        /// <param name="nw">Network data</param>
        /// <param name="ztype">Impedance based on fault duration  
        /// either Sub-transient, Transient, or Steady-state</param>
        /// <returns>Data structure to build-up Z impdance matrix</returns>
        public static ZNetwork BuildZNetwork(this IENetwork nw, ZTypeEnum ztype)
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

            var znw = new ZNetwork
            {
                Buses = dBus,
                Elements = lstEl.Cast<IEZElement>().ToDictionary(e => e.Data.ID),
            };

            return znw;
        }

        internal static IDictionary<string, List<Branch>> BuildGraph(this ZNetwork znw)
        {
            var dBus = new Dictionary<string, List<Branch>> 
            { 
                { "ref", new List<Branch>() } 
            };
            foreach (var bus in znw.Buses.Values)
                dBus.Add(bus.ID, new List<Branch>());
            foreach(var el in znw.Elements.Values)
            {
                if (el.FromBus == null)
                    dBus["ref"].Add(new Branch(el, el.ToBus));
                else dBus[el.FromBus.ID].Add(new Branch(el, el.ToBus));
                dBus[el.ToBus.ID].Add(new Branch(el, el.FromBus));
            }
            return dBus;
        }

        #endregion

        #region Z Matrix

        /// <summary>
        /// Calculate a new Z matrix and save it to Z property.
        /// Once completed, each bus has a new bus index which
        /// are indexes to access the entries of the Z matrix.
        /// </summary>
        /// <param name="znw">ZNetwork data structure</param>
        /// <returns>The same ZNetwork with a new Z matrix stored in the Z property</returns>
        public static ZNetwork BuildZMatrix(this ZNetwork znw)
        {
            var N = znw.Buses.Count;
            znw.Z = MC.Build.Dense(N, N);

            // add z element to the matrix
            // using breath-first algo
            var g = znw.BuildGraph();
            var qbus = new Queue<string>();
            qbus.Enqueue("ref");
            var elSeq = -1;
            while (qbus.Count > 0)
            {
                // get the next bus
                // to process the Z elements
                var bid = qbus.Dequeue();
                var branches = g[bid]
                    .Where(i => i.Element.Sequence < 0)
                    .ToList();
                foreach (var br in branches)
                {
                    if (br.Element.Sequence >= 0)
                        continue; // the element was added earlier

                    if (br.ToBus is IZBus bus && !bus.Visited)
                    {
                        // track the new bus encounter
                        // to come back later and process
                        // it impedance element.
                        bus.Visited = true;
                        qbus.Enqueue(bus.ID);
                    }

                    // add the element to the Z matrix
                    znw.AddElement(br.Element);
                    br.Element.Sequence = ++elSeq;
                }
            }

            return znw;
        }

        /// <summary>
        /// Add the next impedance element to the Z matrix
        /// </summary>
        /// <param name="znw">Z matrix data structure</param>
        /// <param name="element">Impedance element</param>
        /// <returns>The same Z matrix data structure</returns>
        /// <exception cref="Exception">Unable to add the impedance element</exception>
        public static ZNetwork AddElement(this ZNetwork znw, IEZElement element)
        {
            if (element.ValidateAddElementRefToNewBus())
                znw.AddElementRefToNewBus(element);
            else if (element.ValidateAddElementRefToExistBus())
                znw.AddElementRefToExistBus(element);
            else if (element.ValidateAddElementNewToExistBus())
                znw.AddElementNewToExistBus(element);
            else if (element.ValidateAddElementExistToExistBus())
                znw.AddElementExistToExistBus(element);
            else throw new Exception();
            return znw;
        }

        /// <summary>
        /// Case 1 : Add the next impedance element to the Z matrix
        /// </summary>
        internal static ZNetwork AddElementRefToNewBus(this ZNetwork znw, IEZElement element)
        {
            var bus = element.ToBus;
            bus.BusIndex = znw.GetNextBusIndex();
            var p = bus.BusIndex;
            znw.Z[p, p] = element.Z;

            element.AddCase = 1;
            return znw;
        }

        /// <summary>
        /// Case 2 : Add the next impedance element to the Z matrix
        /// </summary>
        internal static ZNetwork AddElementNewToExistBus(this ZNetwork znw, IEZElement element)
        {
            var fb = element.FromBus;
            var tb = element.ToBus;
            var existBus = fb.BusIndex >= 0 ? fb : tb;
            var newBus = fb.BusIndex < 0 ? fb : tb;

            var p = existBus.BusIndex;
            newBus.BusIndex = znw.GetNextBusIndex();
            var q = newBus.BusIndex;

            var zpp = znw.Z[p, p];
            znw.Z[q, q] = zpp + element.Z;
            foreach (var i in Enumerable.Range(0, znw.LastBusIndex))
            {
                znw.Z[q, i] = znw.Z[p, i];
                znw.Z[i, q] = znw.Z[i, p];    
            }

            element.AddCase = 2;
            return znw;
        }

        /// <summary>
        /// Case 3 : Add the next impedance element to the Z matrix
        /// </summary>
        internal static ZNetwork AddElementRefToExistBus(this ZNetwork znw, IEZElement element)
        {
            var q = element.ToBus.BusIndex;
            var dz = MC.Build.Dense(znw.LastBusIndex + 1, 1, Complex.Zero);
            var N = znw.LastBusIndex + 1;
            foreach (var i in Enumerable.Range(0, N))
            {
                dz[i, 0] = -znw.Z[i, q];
            }
            var zll = znw.Z[q, q] + element.Z;
            var md = dz * dz.Transpose() / zll;

            foreach(var i in Enumerable.Range(0, N))
                foreach(var j in Enumerable.Range(0, N))
                    znw.Z[i, j] -= md[i, j];
            
            element.AddCase = 3;
            return znw;
        }

        /// <summary>
        /// Case 4 : Add the next impedance element to the Z matrix
        /// </summary>
        internal static ZNetwork AddElementExistToExistBus(this ZNetwork znw, IEZElement element)
        {
            var p = element.FromBus.BusIndex;
            var q = element.ToBus.BusIndex;
            var N = znw.LastBusIndex + 1;
            var dz = MC.Build.Dense(N, 1, Complex.Zero);
            foreach (var i in Enumerable.Range(0, N))
            {
                dz[i, 0] = znw.Z[i, q] - znw.Z[i, p];
            }
            var zll = element.Z + znw.Z[p, p] + znw.Z[q, q] - 2 * znw.Z[p, q];
            var md = dz * dz.Transpose() / zll;

            foreach (var i in Enumerable.Range(0, N))
                foreach (var j in Enumerable.Range(0, N))
                    znw.Z[i, j] -= md[i, j];

            element.AddCase = 4;
            return znw;
        }


        /// <summary>
        /// Validate adding impedance element as Case 1
        /// </summary>
        internal static bool ValidateAddElementRefToNewBus(this IEZElement element) =>
            element.FromBus == null && element.ToBus.BusIndex < 0;

        /// <summary>
        /// Validate adding impedance element as Case 2
        /// </summary>
        internal static bool ValidateAddElementNewToExistBus(this IEZElement element)
        {
            var fbIdx = element.FromBus.BusIndex;
            var tbIdx = element.ToBus.BusIndex;
            return (fbIdx >= 0 && tbIdx < 0)
                || (fbIdx < 0 && tbIdx >= 0);
        }

        /// <summary>
        /// Validate adding impedance element as Case 3
        /// </summary>
        internal static bool ValidateAddElementRefToExistBus(this IEZElement element) =>
            element.FromBus == null && element.ToBus.BusIndex >= 0;

        /// <summary>
        /// Validate adding impedance element as Case 4
        /// </summary>
        internal static bool ValidateAddElementExistToExistBus(this IEZElement element) =>
            element.FromBus.BusIndex >= 0 && element.ToBus.BusIndex >= 0;

        /// <summary>
        /// Validate that is Z matrix is equal the reference Z matrix
        /// </summary>
        /// <param name="znw">This Z matrix</param>
        /// <param name="refData">Reference Z matrix</param>
        /// <returns>True if both Z matrices are equal</returns>
        internal static bool EQ(this ZNetwork znw, ZNetwork refData)
        {
            var res = true;
            var lstBusId = znw.Buses.Keys;
            foreach (var bi in lstBusId)
                foreach (var bj in lstBusId)
                {
                    var za = znw.Z[znw.Buses[bi].BusIndex, znw.Buses[bj].BusIndex];
                    var zb = refData.Z[refData.Buses[bi].BusIndex, refData.Buses[bj].BusIndex];
                    var v = Checker.EQ(za, zb, 0.0001, 0.0001);
                    res &= v;
                }
            return res;
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
