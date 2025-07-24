using EEMathLib.DTO;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;

namespace EEMathLib.LoadFlow
{
    public interface ILFData
    {
        EENetwork CreateNetwork();
        IEnumerable<EEBus> LFResult { get; }
        MxDTO<Complex> YResult { get; }
        MxDTO<double> J1Result { get; }
    }

    public class LFData : ILFData
    {
        #region Data

        readonly IEnumerable<EEBus> Busses = new List<EEBus>
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

        readonly IEnumerable<EELine> Lines = new List<EELine>
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

        readonly IEnumerable<EEBus> _LFResult = new List<EEBus>
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

        readonly MxDTO<Complex> _YResult = new MxDTO<Complex>
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

        readonly MxDTO<double> _J1Result = new MxDTO<double>
        {
            RowSize = 4,
            ColumnSize = 4,
            EntriesType = MxDTO<double>.ROW_ENTRIES,
            Entries = new double[]
            {
                29.76,  0, -9.91, -19.84,
                0, 104.41, -104.41, 0,
                -9.92, -104.41, 154.01, -39.68,
                -19.84, 0, -39.68, 59.52,
            }
        };

        #endregion

        EENetwork _network = null;
        public EENetwork CreateNetwork()
        {
            if (_network == null)
            {
                _network = new EENetwork(Busses, Lines);
                _network.AssignBusToLine();
                _network.BuildYImp();
            }
            return _network;
        }

        public IEnumerable<EEBus> LFResult => _LFResult;

        public MxDTO<Complex> YResult => _YResult;

        public MxDTO<double> J1Result => _J1Result;
    }
}
