using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using EEMathLib.ShortCircuit.ZMX;
using System.Collections.Generic;

namespace EEMathLib.ShortCircuit.Data
{
    /// <summary>
    /// Sample data for testing
    /// </summary>
    public class ZNetwork1 : ZNetwork
    {
        public ZNetwork1()
        {
            // input data
            Buses = new Dictionary<string, IZBus>
            {
                { "1", new ZBus { ID = "1" } },
                { "2", new ZBus { ID = "2" } },
                { "3", new ZBus { ID = "3" } },
                { "4", new ZBus { ID = "4" } },
            };

            // input data
            Elements = new Dictionary<string, IEZElement>
            {
                { "1", new EZElement { ID = "1", ToBus = Buses["1"], Z = MX.C(0, 0.1) } },
                { "2", new EZElement { ID = "2", ToBus = Buses["2"], Z = MX.C(0, 0.1) } },
                { "3", new EZElement { ID = "3", FromBus = Buses["1"], ToBus = Buses["2"], Z = MX.C(0, 0.2) } },
                { "4", new EZElement { ID = "4", FromBus = Buses["2"], ToBus = Buses["3"], Z = MX.C(0, 0.3) } },
                { "5", new EZElement { ID = "5", FromBus = Buses["3"], ToBus = Buses["4"], Z = MX.C(0, 0.15) } },
                { "6", new EZElement { ID = "6", FromBus = Buses["1"], ToBus = Buses["4"], Z = MX.C(0, 0.25) } },
                { "7", new EZElement { ID = "7", FromBus = Buses["2"], ToBus = Buses["4"], Z = MX.C(0, 0.4) } },
            };

            // result data
            RefZNetwork = new ZNetworkRef();
        }

        /// <summary>
        /// Z matrix result data
        /// </summary>
        class ZNetworkRef : ZNetwork
        {
            public ZNetworkRef()
            {
                // result Z matrix based
                // on elements being added
                // in a specific sequence
                Z = MX.BuildMX(4, 4,
                        MX.C(0, 0.0705), MX.C(0, 0.0295), MX.C(0, 0.0420), MX.C(0, 0.0483),
                        MX.C(0, 0.0295), MX.C(0, 0.0705), MX.C(0, 0.0580), MX.C(0, 0.0517),
                        MX.C(0, 0.0420), MX.C(0, 0.0580), MX.C(0, 0.2041), MX.C(0, 0.1271),
                        MX.C(0, 0.0483), MX.C(0, 0.0517), MX.C(0, 0.1271), MX.C(0, 0.1648)
                    );
            }
        }
    }

    /// <summary>
    /// Sample data for testing
    /// </summary>
    public class ZNetwork2 : ZNetwork
    {
        public ZNetwork2()
        {
            // input data
            Buses = new Dictionary<string, IZBus>
            {
                { "1", new ZBus { ID = "1", Data = new EEBus { Voltage = 1.05 } } },
                { "2", new ZBus { ID = "2", Data = new EEBus { Voltage = 1.05 } } },
                { "3", new ZBus { ID = "3", Data = new EEBus { Voltage = 1.05 } } },
                { "4", new ZBus { ID = "4", Data = new EEBus { Voltage = 1.05 } } },
                { "5", new ZBus { ID = "5", Data = new EEBus { Voltage = 1.05 } } },
            };

            // input data
            Elements = new Dictionary<string, IEZElement>
            {
                { "1", new EZElement { ID = "1", ToBus = Buses["1"], Z = MX.C(0, 0.045) } },
                { "2", new EZElement { ID = "2", ToBus = Buses["3"], Z = MX.C(0, 0.0225) } },
                { "3", new EZElement { ID = "3", FromBus = Buses["2"], ToBus = Buses["4"], Z = MX.C(0, 0.1) } },
                { "4", new EZElement { ID = "4", FromBus = Buses["2"], ToBus = Buses["5"], Z = MX.C(0, 0.05) } },
                { "5", new EZElement { ID = "5", FromBus = Buses["4"], ToBus = Buses["5"], Z = MX.C(0, 0.025) } },
                { "6", new EZElement { ID = "6", FromBus = Buses["1"], ToBus = Buses["5"], Z = MX.C(0, 0.02) } },
                { "7", new EZElement { ID = "7", FromBus = Buses["3"], ToBus = Buses["4"], Z = MX.C(0, 0.01) } },
            };

            // result data
            RefZNetwork = new ZNetworkRef();
        }

        /// <summary>
        /// Z matrix result data
        /// </summary>
        class ZNetworkRef : ZNetwork
        {
            public ZNetworkRef()
            {
                // Bus data with index corresponding
                // to entries of Z matrix
                Buses = new Dictionary<string, IZBus>
                {
                    { "1", new ZBus { ID = "1", BusIndex = 0 } },
                    { "2", new ZBus { ID = "2", BusIndex = 1 } },
                    { "3", new ZBus { ID = "3", BusIndex = 2 } },
                    { "4", new ZBus { ID = "4", BusIndex = 3 } },
                    { "5", new ZBus { ID = "5", BusIndex = 4 } },
                };

                // result Z matrix based
                // on elements being added
                // in a specific sequence
                Z = MX.BuildMX(5, 5,
                        MX.C(0, 0.0279725), MX.C(0, 0.0177025), MX.C(0, 0.0085125), MX.C(0, 0.0122975), MX.C(0, 0.020405),
                        MX.C(0, 0.0177025), MX.C(0, 0.0569525), MX.C(0, 0.0136475), MX.C(0, 0.019715), MX.C(0, 0.02557),
                        MX.C(0, 0.0085125), MX.C(0, 0.0136475), MX.C(0, 0.0182425), MX.C(0, 0.016353), MX.C(0, 0.012298),
                        MX.C(0, 0.0122975), MX.C(0, 0.019715), MX.C(0, 0.016353), MX.C(0, 0.0236), MX.C(0, 0.017763),
                        MX.C(0, 0.020405), MX.C(0, 0.02557), MX.C(0, 0.012298), MX.C(0, 0.017763), MX.C(0, 0.029475)
                    );
            }
        }

    }

    #region ZNetwork3

    /// <summary>
    /// Sample data for testing
    /// </summary>
    public class ZNetwork3Z1 : ZNetwork
    {
        public ZNetwork3Z1()
        {
            // input data
            Buses = new Dictionary<string, IZBus>
            {
                { "1", new ZBus { ID = "1", Data = new EEBus { Voltage = 1.0 } } },
                { "2", new ZBus { ID = "2", Data = new EEBus { Voltage = 1.0 } } },
                { "3", new ZBus { ID = "3", Data = new EEBus { Voltage = 1.0 } } },
                { "4", new ZBus { ID = "4", Data = new EEBus { Voltage = 1.0 } } },
                { "5", new ZBus { ID = "5", Data = new EEBus { Voltage = 1.0 } } },
                { "6", new ZBus { ID = "6", Data = new EEBus { Voltage = 1.0 } } },
            };

            // input data
            Elements = new Dictionary<string, IEZElement>
            {
                { "1", new EZElement { ID = "1", ToBus = Buses["1"], Z = MX.C(0, 0.2) } },
                { "2", new EZElement { ID = "2", ToBus = Buses["4"], Z = MX.C(0, 0.2) } },
                { "3", new EZElement { ID = "3", FromBus = Buses["1"], ToBus = Buses["2"], Z = MX.C(0, 0.2) } },
                { "4", new EZElement { ID = "4", FromBus = Buses["1"], ToBus = Buses["5"], Z = MX.C(0, 0.25) } },
                { "5", new EZElement { ID = "5", FromBus = Buses["2"], ToBus = Buses["3"], Z = MX.C(0, 0.15) } },
                { "6", new EZElement { ID = "6", FromBus = Buses["3"], ToBus = Buses["4"], Z = MX.C(0, 0.3) } },
                { "7", new EZElement { ID = "7", FromBus = Buses["5"], ToBus = Buses["6"], Z = MX.C(0, 0.22) } },
                { "8", new EZElement { ID = "8", FromBus = Buses["6"], ToBus = Buses["4"], Z = MX.C(0, 0.35) } },
            };
        }
    }

    /// <summary>
    /// Sample data for testing
    /// </summary>
    public class ZNetwork3Z2 : ZNetwork
    {
        public void Init()
        {
            // input data
            Elements = new Dictionary<string, IEZElement>
            {
                { "1", new EZElement { ID = "1", ToBus = Buses["1"], Z = MX.C(0, 0.14) } },
                { "2", new EZElement { ID = "2", ToBus = Buses["4"], Z = MX.C(0, 0.14) } },
                { "3", new EZElement { ID = "3", FromBus = Buses["1"], ToBus = Buses["2"], Z = MX.C(0, 0.2) } },
                { "4", new EZElement { ID = "4", FromBus = Buses["1"], ToBus = Buses["5"], Z = MX.C(0, 0.25) } },
                { "5", new EZElement { ID = "5", FromBus = Buses["2"], ToBus = Buses["3"], Z = MX.C(0, 0.15) } },
                { "6", new EZElement { ID = "6", FromBus = Buses["3"], ToBus = Buses["4"], Z = MX.C(0, 0.3) } },
                { "7", new EZElement { ID = "7", FromBus = Buses["5"], ToBus = Buses["6"], Z = MX.C(0, 0.22) } },
                { "8", new EZElement { ID = "8", FromBus = Buses["6"], ToBus = Buses["4"], Z = MX.C(0, 0.35) } },
            };
        }
    }

    /// <summary>
    /// Sample data for testing
    /// </summary>
    public class ZNetwork3Z0 : ZNetwork
    {
        public void Init()
        {
            // input data
            Elements = new Dictionary<string, IEZElement>
            {
                { "1", new EZElement { ID = "1", ToBus = Buses["1"], Z = MX.C(0, 0.06) } },
                { "2", new EZElement { ID = "2", ToBus = Buses["4"], Z = MX.C(0, 0.15) } },
                { "3", new EZElement { ID = "3", FromBus = Buses["1"], ToBus = Buses["2"], Z = MX.C(0, 0.2) } },
                { "4", new EZElement { ID = "4", FromBus = Buses["1"], ToBus = Buses["5"], Z = MX.C(0, 0.25) } },
                { "5", new EZElement { ID = "5", FromBus = Buses["2"], ToBus = Buses["3"], Z = MX.C(0, 0.3) } },
                { "7", new EZElement { ID = "7", FromBus = Buses["5"], ToBus = Buses["6"], Z = MX.C(0, 0.5) } },
            };

        }
    }


    #endregion
}
