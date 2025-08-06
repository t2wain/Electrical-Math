using EEMathLib.DTO;
using System.Collections.Generic;
using System.Numerics;
using System;
using MathNet.Numerics.LinearAlgebra;

namespace EEMathLib.LoadFlow.Data
{
    #region NRData for several iterations

    class LFData2_NRData0 : NewtonRaphsonData
    {
        public LFData2_NRData0()
        {
            Iteration = 0;
            PCal = new double[] { -0.0444e-14, -0.1776e-14, -0.0333e-14, -0.0444e-14 };
            QCal = new double[] { -0.143, -0.143 };
            MDelta = new double[] { 0.5, 1.0, -1.15, -0.85, -0.457, -0.257 };
        }
    }

    class LFData2_NRData1 : NewtonRaphsonData
    {
        #region Jacobian

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
                    13.0858,  -7.4835, 0, 0,
                    -7.4835, 19.0911, -7.1309, -4.4768,
                    0, -7.1309, 10.8657, -3.7348,
                    0, -4.4768, -3.7348, 15.6951,
                    }
                };

                J2Result = new MxDTO<double>
                {
                    RowSize = 4,
                    ColumnSize = 2,
                    EntriesType = MxDTO<double>.ROW_ENTRIES,
                    Entries = new double[]
                    {
                    0, 0,
                    -1.2584, -1.12982,
                    2.1921, -0.9337,
                    -0.9337, 3.9047
                    }
                };

                J3Result = new MxDTO<double>
                {
                    RowSize = 2,
                    ColumnSize = 4,
                    EntriesType = MxDTO<double>.ROW_ENTRIES,
                    Entries = new double[]
                    {
                    0,  1.2584, -2.1921, 0.9337,
                    0, 1.1298, 0.9337, -3.9047,
                    }
                };

                J4Result = new MxDTO<double>
                {
                    RowSize = 2,
                    ColumnSize = 2,
                    EntriesType = MxDTO<double>.ROW_ENTRIES,
                    Entries = new double[]
                    {
                    10.5797, -3.7348,
                    -3.7348, 15.4091,
                    }
                };
            }

            public override double GetJ1kk(BusResult bus, Matrix<double> res = null)
            {
                if (res != null)
                {
                    var v = res[bus.Pidx, bus.Pidx];
                    return v;
                }

                if (bus.ID == "2")
                    return 13.0858;
                else if (bus.ID == "3")
                    return 19.0911;
                else if (bus.ID == "4")
                    return 10.8657;
                else if (bus.ID == "5")
                    return 15.6951;
                else throw new Exception();
            }

            public override double GetJ1kn(BusResult b1,
                BusResult b2, Matrix<double> res = null)
            {
                if (res != null)
                {
                    var v = res[b1.Pidx, b2.Aidx];
                    return v;
                }

                if (b1.ID == "2")
                {
                    switch (b2.ID)
                    {
                        case "3":
                            return -7.4835;
                        case "4":
                            return 0;
                        case "5":
                            return 0;
                        default:
                            throw new Exception();
                    }
                }
                else if (b1.ID == "3")
                {
                    switch (b2.ID)
                    {
                        case "2":
                            return -7.4835;
                        case "4":
                            return -7.1309;
                        case "5":
                            return -4.4768;
                        default:
                            throw new Exception();
                    }
                }
                else if (b1.ID == "4")
                {
                    switch (b2.ID)
                    {
                        case "2":
                            return 0;
                        case "3":
                            return -7.1309;
                        case "5":
                            return -3.7348;
                        default:
                            throw new Exception();
                    }
                }
                else if (b1.ID == "5")
                {
                    switch (b2.ID)
                    {
                        case "2":
                            return 0;
                        case "3":
                            return -4.4768;
                        case "4":
                            return -3.7348;
                        default:
                            throw new Exception();
                    }
                }
                else throw new Exception();
            }

        }

        #endregion

        public LFData2_NRData1()
        {
            Iteration = 1;
            JacobianData = new Jacobian();
            PCal = new double[] { 0.52023, 0.9293, -1.0413, -0.8128 };
            QCal = new double[] { -0.5158, 0.3472 };
            ADelta = new double[] { 0.0331, -0.0090, -0.1275, -0.0799 };
            VDelta = new double[] { -0.0783, -0.0475 };
            VBus = new double[] { 1.0000, 1.0000, 1.0000, 0.9217, 0.9525 };
            ABus = new double[] { 0, 0.0331, -0.0090, -0.1275, -0.0799 };
            MDelta = new double[] { -0.0023, 0.0707, -0.1087, -0.0372, -0.0842, -0.0528 };
        }
    }

    class LFData2_NRData2 : NewtonRaphsonData
    {
        public LFData2_NRData2()
        {
            Iteration = 2;
        }
    }

    class LFData2_NRData3 : NewtonRaphsonData
    {
        public LFData2_NRData3()
        {
            Iteration = 3;
        }
    }

    #endregion

    /// <summary>
    /// Load flow dataset #2 for testing
    /// </summary>
    public class LFData2 : LFDataAbstract
    {
        INewtonRaphsonData _nrdata0;
        INewtonRaphsonData _nrdata1;
        INewtonRaphsonData _nrdata2;
        INewtonRaphsonData _nrdata3;

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
                    C(3.2417, -13.0138), C(-1.4006, 5.6022), Zero, Zero, C(-1.8412, 7.4835),
                    C(-1.4006, 5.6022), C(3.2417, -13.0138), C(-1.8412,7.4835), Zero, Zero,
                    Zero, C(-1.8412,7.4835), C(4.2294, -18.9271), C(-1.2584, 7.1309), C(-1.1298,4.4768),
                    Zero, Zero, C(-1.2584, 7.1309), C(2.1921, -10.7227), C(-0.9337, 3.7348),
                    C(-1.8412, 7.4835), Zero, C(-1.1298,4.4768), C(-0.9337, 3.7348), C(3.9047, -15.5521)
                }
            };

        }

        public override INewtonRaphsonData GetNewtonRaphsonData(int iteration = 0)
        {
            switch (iteration)
            {
                case 0:
                    if (_nrdata0 == null)
                        _nrdata0 = new LFData2_NRData0();
                    return _nrdata0;
                case 1:
                    if (_nrdata1 == null)
                        _nrdata1 = new LFData2_NRData1();
                    return _nrdata1;
                case 2:
                    if (_nrdata2 == null)
                        _nrdata2 = new LFData2_NRData2();
                    return _nrdata2;
                case 3:
                    if (_nrdata3 == null)
                        _nrdata3 = new LFData2_NRData3();
                    return _nrdata3;
                default:
                    throw new Exception();
            }
        }
    }
}
