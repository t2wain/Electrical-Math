using EEMathLib.DTO;
using EEMathLib.MatrixMath;
using MathNet.Numerics.LinearAlgebra.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using GS = EEMathLib.LoadFlow.LFGaussSiedel;
using NR = EEMathLib.LoadFlow.LFNewtonRaphson;

namespace EEMathLib.LoadFlow
{
    public static class LFExample
    {
        #region Data

        static readonly IEnumerable<EEBus> Busses = new List<EEBus>
        {
            new EEBus
            {
                BusIndex = 0,
                ID = "1",
                BusType = BusTypeEnum.Slack,
            },
            new EEBus
            {
                BusIndex = 1,
                ID = "2",
                BusType = BusTypeEnum.PQ,
                Pload = 8.0,
                Qload = 2.8,
            },
            new EEBus
            {
                BusIndex = 2,
                ID = "3",
                BusType = BusTypeEnum.PV,
                Voltage = 1.05,
                Pgen = 5.2,
                Pload = 0.8,
                Qload = 0.4,
                Qmin = -2.8,
                Qmax = 4.0,
            },
            new EEBus
            {
                BusIndex = 3,
                ID = "4",
                BusType = BusTypeEnum.PQ,
            },
            new EEBus
            {
                BusIndex = 4,
                ID = "5",
                BusType = BusTypeEnum.PQ,
            },
        };

        static readonly IEnumerable<EELine> Lines = new List<EELine>
        {
            new EELine
            {
                FromBusID = "2",
                ToBusID = "4",
                R = 0.009,
                X = 0.1,
                B = 1.72,
            },
            new EELine
            {
                FromBusID = "2",
                ToBusID = "5",
                R = 0.0045,
                X = 0.05,
                B = 0.88,
            },
            new EELine
            {
                FromBusID = "4",
                ToBusID = "5",
                R = 0.00225,
                X = 0.025,
                B = 0.44,
            },
            new EELine
            {
                FromBusID = "1",
                ToBusID = "5",
                R = 0.0015,
                X = 0.02,
            },
            new EELine
            {
                FromBusID = "3",
                ToBusID = "4",
                R = 0.00075,
                X = 0.01,
            },
        };

        static readonly IEnumerable<EEBus> LFResult = new List<EEBus>
        {
            new EEBus
            {
                BusIndex = 0,
                ID = "1",
                BusType = BusTypeEnum.Slack,
                Voltage = 1.0,
                Angle = 0.0,
                Pgen = 3.948,
                Qgen = 1.144,
            },
            new EEBus
            {
                BusIndex = 1,
                ID = "2",
                BusType = BusTypeEnum.PQ,
                Voltage = 0.834,
                Angle = -22.407,
                Pload = 8.0,
                Qload = 2.8,
            },
            new EEBus
            {
                BusIndex = 2,
                ID = "3",
                BusType = BusTypeEnum.PV,
                Voltage = 1.05,
                Angle = -0.597,
                Pgen = 5.2,
                Qgen = 3.376,
                Pload = 0.8,
                Qload = 0.4,
                Qmin = -2.8,
                Qmax = 4.0,
            },
            new EEBus
            {
                BusIndex = 3,
                ID = "4",
                BusType = BusTypeEnum.PQ,
                Voltage = 1.019,
                Angle = -2.834,
            },
            new EEBus
            {
                BusIndex = 4,
                ID = "5",
                BusType = BusTypeEnum.PQ,
                Voltage = 0.974,
                Angle = -4.548,
            },
        };

        static readonly MxDTO<Complex> YResult = new MxDTO<Complex>
        {
            RowSize = 5,
            ColumnSize = 5,
            EntriesType = MxDTO<Complex>.ROW_ENTRIES,
            Entries = new Complex[] 
            {
                new Complex(3.73, -49.72), Complex.Zero, Complex.Zero, Complex.Zero, new Complex(-3.73, 49.72),
                Complex.Zero, new Complex(2.68, -28.46), Complex.Zero, new Complex(-0.89, 9.92), new Complex(-1.79, 19.84),
                Complex.Zero, Complex.Zero, new Complex(7.46, -99.44), new Complex(-7.46, 99.44), Complex.Zero,
                Complex.Zero, new Complex(-0.89, 9.92), new Complex(-7.46, 99.44), new Complex(11.92, -147.96), new Complex(-3.57, 39.68),
                new Complex(-3.73, 49.72), new Complex(-1.79, 19.84), Complex.Zero, new Complex(-3.57, 39.68), new Complex(9.09, -108.58)
            }
        };  

        static EENetwork CreateNetwork()
        {
            var nw = new EENetwork(Busses, Lines);
            nw.AssignBusToLine();
            nw.BuildYImp();
            return nw;
        }

        #endregion

        #region Y Matrix

        /// <summary>
        /// Build Y matrix
        /// </summary>
        public static bool BuildYMatrix_Partial()
        {
            var nw = CreateNetwork();
            var Y = nw.YMatrix;
            var res = MX.ParseMatrix(YResult);

            var y25 = new Phasor(19.9195, 95.143);
            var e25 = Phasor.Convert(Y[1, 4]);
            var c = Checker.EQ(y25, e25, 0.0001, 0.001);

            var y24 = new Phasor(9.95972, 95.143);
            var e24 = Phasor.Convert(Y[1, 3]);
            c = c && Checker.EQ(y24, e24, 0.0001, 0.001);

            var y22 = new Phasor(28.5847, -84.624);
            var e22 = Phasor.Convert(Y[1, 1]);
            c = c && Checker.EQ(y22, e22, 0.0001, 0.001);

            return c;
        }

        /// <summary>
        /// Build Y matrix
        /// </summary>
        public static bool BuildYMatrix()
        {
            var nw = CreateNetwork();
            var Y = nw.YMatrix;
            var res = MX.ParseMatrix(YResult);

            var d = Y.Transpose().AsColumnMajorArray();
            var q = d.Zip(YResult.Entries, (y, r) => new { y, r });
            var c = true;
            foreach(var i in q)
            {
                c = c && Checker.EQ(i.y, i.r, 0.01, 0.01);
                if (!c) break;
            }
            return c;
        }

        #endregion

        #region Gauss-Siedel examples

        /// <summary>
        /// Calculate bus "2" voltage at iteration 1.
        /// </summary>
        public static bool LoadFlowGS_CalcVoltage()
        {
            var nw = CreateNetwork();
            var buses = GS.Initialize(nw.Buses);
            var bus = buses.FirstOrDefault(b => b.ID == "2");
            var v = GS.CalcVoltage(bus, nw.YMatrix, buses);

            var v2 = new Phasor(0.8746, -15.675);
            var e2 = (Phasor)v;

            var c = Checker.EQ(v2, e2, 0.0001, 0.001);

            return c;
        }

        /// <summary>
        /// Solve load flow using Gauss-Siedel method.
        /// </summary>
        public static bool LoadFlowGS_Solve()
        {
            var nw = CreateNetwork();
            var threshold = 0.0125;
            var res = GS.Solve(nw, threshold, 100, 20);

            if (res.Error == ErrorEnum.Divergence)
                return false;

            var rbuses = GS.CalcResult(res.Data).ToDictionary(bus => bus.ID);
            var dbuses = LFResult.ToDictionary(bus => bus.ID);

            var c = true;
            foreach (var dbus in dbuses.Values)
            {
                var rb = rbuses[dbus.ID];
                c = c && Checker.EQPct(rb.Voltage, dbus.Voltage, threshold);
                c = c && Checker.EQPct(rb.Angle, dbus.Angle, threshold);

                if (rb.BusType == BusTypeEnum.PQ
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    c = c && Checker.EQPct(rb.Pgen, dbus.Pgen, threshold);
                    c = c && Checker.EQPct(rb.Qgen, dbus.Qgen, threshold);
                }
            }

            return c;
        }

        #endregion

        #region Newton-Raphson examples

        public static bool LoadFlowNR_Power_J1()
        {
            var nw = CreateNetwork();
            var buses = NR.Initialize(nw.Buses);
            var busesPQ = NR.ReIndexBusPQ(buses)
                .Where(b => b.BusIndex > -1)
                .ToList();

            var bus2 = busesPQ.FirstOrDefault(b => b.ID == "2");

            var mxPQdelta = NR.CalcDeltaPower(nw.YMatrix, busesPQ);
            var rowCount = mxPQdelta.RowCount;
            var c = rowCount == 8;

            var pdk = mxPQdelta[bus2.BusIndex, 0];
            var res = -7.99972;
            c = c && Checker.EQ(pdk, res, 0.001);

            var J1 = NR.CreateJ1(nw.YMatrix, busesPQ);
            var bus4 = busesPQ.FirstOrDefault(b => b.ID == "4");
            var j24 = J1[bus2.BusIndex, bus4.BusIndex];
            res = -9.91964;
            c = c && Checker.EQ(j24, res, 0.001);

            return c;
        }

        public static bool LoadFlowNR_JMatrix()
        {
            var nw = CreateNetwork();
            var buses = NR.Initialize(nw.Buses);
            var busesPQ = NR.ReIndexBusPQ(buses)
                .Where(b => b.BusIndex > -1)
                .ToList();

            var J1 = NR.CreateJ1(nw.YMatrix, busesPQ);
            var J2 = NR.CreateJ2(nw.YMatrix, busesPQ);
            var J3 = NR.CreateJ3(nw.YMatrix, busesPQ);
            var J4 = NR.CreateJ4(nw.YMatrix, busesPQ);

            var J = NR.CreateJacobianMatrix(nw.YMatrix, busesPQ);

            return true;
        }

        public static bool LoadFlowNR_Solve()
        {

            var nw = CreateNetwork();
            var threshold = 0.001;
            var res = NR.Solve(nw, threshold, 100, 20);

            if (res.Error == ErrorEnum.Divergence)
                return false;


            var rbuses = NR.CalcResult(res.Data).ToDictionary(bus => bus.ID);
            var dbuses = LFResult.ToDictionary(bus => bus.ID);

            var c = true;
            foreach (var dbus in dbuses.Values)
            {
                var rb = rbuses[dbus.ID];
                c = c && Checker.EQPct(rb.Voltage, dbus.Voltage, threshold);
                c = c && Checker.EQPct(rb.Angle, dbus.Angle, threshold);

                if (rb.BusType == BusTypeEnum.PQ
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    c = c && Checker.EQPct(rb.Pgen, dbus.Pgen, threshold);
                    c = c && Checker.EQPct(rb.Qgen, dbus.Qgen, threshold);
                }
            }

            return c;
        }

        #endregion
    }
}
