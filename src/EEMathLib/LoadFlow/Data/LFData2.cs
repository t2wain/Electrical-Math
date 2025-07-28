using EEMathLib.DTO;
using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.LoadFlow.Data
{
    public class LFData2 : LFDataAbstract
    {
        public LFData2()
        {
            double _BaseMVA = 100.0;

            _Busses = new List<EEBus>
            {
                new EEBus
                {
                    BusIndex = 0,
                    ID = "1",
                    Voltage = 1.0,
                    BusType = BusTypeEnum.Slack,
                },
                new EEBus
                {
                    BusIndex = 1,
                    ID = "2",
                    BusType = BusTypeEnum.PV,
                    Voltage = 1.0,
                    Pgen = 50 / _BaseMVA,
                    Qmin = -500 / _BaseMVA,
                    Qmax = 500 / _BaseMVA,
                },
                new EEBus
                {
                    BusIndex = 2,
                    ID = "3",
                    BusType = BusTypeEnum.PV,
                    Voltage = 1.0,
                    Pgen = 100 / _BaseMVA,
                    Qmin = -500 / _BaseMVA,
                    Qmax = 500 / _BaseMVA,
                },
                new EEBus
                {
                    BusIndex = 3,
                    ID = "4",
                    BusType = BusTypeEnum.PQ,
                    Pload = 115 / _BaseMVA,
                    Qload = 60 / _BaseMVA,
                },
                new EEBus
                {
                    BusIndex = 4,
                    ID = "5",
                    BusType = BusTypeEnum.PQ,
                    Pload = 85 / _BaseMVA,
                    Qload = 40 / _BaseMVA,
                },
            };

            _Lines = new List<EELine>
            {
                new EELine
                {
                    ID = "1",
                    FromBusID = "1",
                    ToBusID = "2",
                    R = 0.042,
                    X = 0.168,
                    B = 0.041 * 2,
                },
                new EELine
                {
                    ID = "2",
                    FromBusID = "1",
                    ToBusID = "5",
                    R = 0.031,
                    X = 0.126,
                    B = 0.031 * 2,
                },
                new EELine
                {
                    ID = "3",
                    FromBusID = "2",
                    ToBusID = "3",
                    R = 0.031,
                    X = 0.126,
                    B = 0.031 * 2,
                },
                new EELine
                {
                    ID = "4",
                    FromBusID = "3",
                    ToBusID = "4",
                    R = 0.031,
                    X = 0.126,
                    B = 0.031 * 2,
                },
                new EELine
                {
                    ID = "5",
                    FromBusID = "3",
                    ToBusID = "5",
                    R = 0.053,
                    X = 0.210,
                    B = 0.051 * 2,
                },
                new EELine
                {
                    ID = "6",
                    FromBusID = "4",
                    ToBusID = "5",
                    R = 0.063,
                    X = 0.252,
                    B = 0.061 * 2,
                },
            };

            _LFResult = new List<EEBus>
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

            _YResult = new MxDTO<Complex>
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

            _J1Result = new MxDTO<double>
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
        
        }
    }
}
