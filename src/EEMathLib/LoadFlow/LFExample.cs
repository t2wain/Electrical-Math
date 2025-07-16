using System.Collections.Generic;
using System.Linq;

namespace EEMathLib.LoadFlow
{
    public static class LFExample
    {
        static IEnumerable<EEBus> Busses => new List<EEBus>
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

        static IEnumerable<EELine> Lines => new List<EELine>
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
                X = 0.44,
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

        static EENetwork CreateNetwork()
        {
            var nw = new EENetwork(Busses, Lines);
            nw.AssignBusToLine();
            nw.BuildYImp();
            return nw;
        }

        /// <summary>
        /// Build Y matrix
        /// </summary>
        public static bool Ex1()
        {
            var nw = CreateNetwork();
            var Y = nw.YMatrix;

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
        /// Calculate load flow using Gauss-Siedel method
        /// </summary>
        public static bool Ex2()
        {
            var nw = CreateNetwork();
            var buses = LFGaussSiedel.Solve(nw, 1);
            var dbus = buses.ToDictionary(b => b.BusData.ID);
            var bus = dbus["2"];

            var v2 = new Phasor(0.8746, -15.675);
            var e2 = Phasor.Convert(bus.BusVoltage);
            var c = Checker.EQ(v2, e2, 0.0001, 0.001);

            return true;
        }
    }
}
