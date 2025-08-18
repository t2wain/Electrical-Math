using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EEMathLib.LoadFlow.Data
{
    class LFData_NRData : NewtonRaphsonData
    {
        class Jacobian : JacobianData
        {
            public Jacobian()
            {
                J1Result = new MxDTO<double>
                {
                    RowSize = 4,
                    ColumnSize = 4,
                    EntriesType = MxDTO<double>.ROW_ENTRIES,
                    Entries = new double[]
                    {
                        29.76,  0, -9.91, -19.84,
                        0, 104.41, -104.41, 0,
                        -9.92, -104.41, 154.01, -39.68,
                        -19.84, 0, -39.68, 109.24,
                    }
                };
            }

            public override double GetJ1kk(BusResult b1, Matrix<double> res = null)
            {
                if (b1.ID == "2")
                    return 29.76;
                else if (b1.ID == "3")
                    return 104.41;
                else if (b1.ID == "4")
                    return 154.01;
                else if (b1.ID == "5")
                    return 109.24;
                else throw new Exception();
            }

            public override double GetJ1kn(BusResult b1,
                BusResult b2, Matrix<double> res = null)
            {
                if (b1.ID == "2")
                {
                    switch (b2.ID)
                    {
                        case "3":
                            return 0;
                        case "4":
                            return -9.92;
                        case "5":
                            return -19.84;
                        default:
                            throw new Exception();
                    }
                }
                else if (b1.ID == "3")
                {
                    switch (b2.ID)
                    {
                        case "2":
                        case "5":
                            return 0;
                        case "4":
                            return -104.41;
                        default:
                            throw new Exception();
                    }
                }
                else if (b1.ID == "4")
                {
                    switch (b2.ID)
                    {
                        case "2":
                            return -9.92;
                        case "3":
                            return -104.41;
                        case "5":
                            return -39.68;
                        default:
                            throw new Exception();
                    }
                }
                else if (b1.ID == "5")
                {
                    switch (b2.ID)
                    {
                        case "2":
                            return -19.84;
                        case "3":
                            return 0;
                        case "4":
                            return -39.68;
                        default:
                            throw new Exception();
                    }
                }
                else throw new Exception();
            }

        }

        public LFData_NRData()
        {
            Iteration = 1;
            JacobianData = new Jacobian();
        }
    }

    /// <summary>
    /// Load flow dataset #1 for testing
    /// </summary>
    public class LFData : LFDataAbstract
    {
        INewtonRaphsonData _nrdata;

        public LFData()
        {
            _Busses = new List<EEBus>
            {
                new EEBus
                {
                    BusIndex = 0,
                    ID = "1",
                    BusType = BusTypeEnum.Slack,

                    PTransmitResult = 3.948,
                    QTransmitResult = 1.144,
                },
                new EEBus
                {
                    BusIndex = 1,
                    ID = "2",
                    BusType = BusTypeEnum.PQ,
                    Pload = 8.0,
                    Qload = 2.8,

                    VoltageResult = 0.834,
                    AngleResult = -22.407,
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

                    VoltageResult = 1.05,
                    AngleResult = -0.597,
                    QTransmitResult = 3.376 - 0.4,
                    QgenResult = 3.376,
                },
                new EEBus
                {
                    BusIndex = 3,
                    ID = "4",
                    BusType = BusTypeEnum.PQ,

                    VoltageResult = 1.019,
                    AngleResult = -2.834,
                },
                new EEBus
                {
                    BusIndex = 4,
                    ID = "5",
                    BusType = BusTypeEnum.PQ,

                    VoltageResult = 0.974,
                    AngleResult = -4.548,
                },
            };


            _Lines = new List<EELine>
            {
                new EELine
                {
                    FromBusID = "2",
                    ToBusID = "4",
                    RSeries = 0.009,
                    XSeries = 0.1,
                    BShunt = 1.72,
                },
                new EELine
                {
                    FromBusID = "2",
                    ToBusID = "5",
                    RSeries = 0.0045,
                    XSeries = 0.05,
                    BShunt = 0.88,
                },
                new EELine
                {
                    FromBusID = "4",
                    ToBusID = "5",
                    RSeries = 0.00225,
                    XSeries = 0.025,
                    BShunt = 0.44,
                },
                new EELine
                {
                    FromBusID = "1",
                    ToBusID = "5",
                    RSeries = 0.0015,
                    XSeries = 0.02,
                },
                new EELine
                {
                    FromBusID = "3",
                    ToBusID = "4",
                    RSeries = 0.00075,
                    XSeries = 0.01,
                },
            };

            _YResult = new MxDTO<Complex>
            {
                RowSize = 5,
                ColumnSize = 5,
                EntriesType = MxDTO<Complex>.ROW_ENTRIES,
                Entries = new Complex[]
                {
                    C(3.73, -49.72), Zero, Zero, Zero, C(-3.73, 49.72),
                    Zero, C(2.68, -28.46), Zero, C(-0.89, 9.92), C(-1.79, 19.84),
                    Zero, Zero, C(7.46, -99.44), C(-7.46, 99.44), Zero,
                    Zero, C(-0.89, 9.92), C(-7.46, 99.44), C(11.92, -147.96), C(-3.57, 39.68),
                    C(-3.73, 49.72), C(-1.79, 19.84), Zero, C(-3.57, 39.68), C(9.09, -108.58)
                }
            };

            _nrdata = new LFData_NRData();
        }

        public override INewtonRaphsonData GetNewtonRaphsonData(int iteration = 0) => _nrdata;
    }
}
