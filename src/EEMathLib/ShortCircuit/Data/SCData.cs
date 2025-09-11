using EEMathLib.DTO;
using EEMathLib.LoadFlow.Data;
using EEMathLib.MatrixMath;
using EEMathLib.ShortCircuit.ZMX;
using System.Collections.Generic;
using System.Numerics;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit.Data
{
    public class ZNetwork1 : ZNetwork
    {
        public ZNetwork1()
        {
            Buses = new Dictionary<string, IZBus>
            {
                { "1", new ZBus { ID = "1" } },
                { "2", new ZBus { ID = "2" } },
                { "3", new ZBus { ID = "3" } },
                { "4", new ZBus { ID = "4" } },
            };

            Elements = new Dictionary<string, IEZElement>
            {
                { "1", new EZElement { ID = "1", ToBus = Buses["1"], Z = C(0, 0.1) } },
                { "2", new EZElement { ID = "2", ToBus = Buses["2"], Z = C(0, 0.1) } },
                { "3", new EZElement { ID = "3", FromBus = Buses["1"], ToBus = Buses["2"], Z = C(0, 0.2) } },
                { "4", new EZElement { ID = "4", FromBus = Buses["2"], ToBus = Buses["3"], Z = C(0, 0.3) } },
                { "5", new EZElement { ID = "5", FromBus = Buses["3"], ToBus = Buses["4"], Z = C(0, 0.15) } },
                { "6", new EZElement { ID = "6", FromBus = Buses["1"], ToBus = Buses["4"], Z = C(0, 0.25) } },
                { "7", new EZElement { ID = "7", FromBus = Buses["2"], ToBus = Buses["4"], Z = C(0, 0.4) } },
            };

            RefZNetwork = new ZNetworkRef();
        }

        /// <summary>
        /// Build Z matrix by adding individual element
        /// in a specific sequence so it can be checked
        /// against the reference Z matrix data
        /// </summary>
        internal ZNetwork1 BuildZTest()
        {
            var N = Buses.Count;
            Z = MC.Build.Dense(N, N);

            var dEl = Elements;

            #region Add elements

            var znw = this;

            // build up Z matrix by adding elements
            // in a particular order to get a result
            // that can be checked against reference
            // Z matrix data
            var el1 = dEl["1"];
            el1.Sequence = 0;
            if (el1.ValidateAddElementRefToNewBus())
                znw.AddElementRefToNewBus(el1);

            var el2 = dEl["2"];
            el2.Sequence = 1;
            if (el2.ValidateAddElementRefToNewBus())
                znw.AddElementRefToNewBus(el2);

            var el3 = dEl["3"];
            el3.Sequence = 2;
            if (el3.ValidateAddElementExistToExistBus())
                znw.AddElementExistToExistBus(el3);

            var el4 = dEl["4"];
            el4.Sequence = 3;
            if (el4.ValidateAddElementNewToExistBus())
                znw.AddElementNewToExistBus(el4);

            var el5 = dEl["5"];
            el5.Sequence = 4;
            if (el5.ValidateAddElementNewToExistBus())
                znw.AddElementNewToExistBus(el5);

            var el6 = dEl["6"];
            el6.Sequence = 5;
            if (el6.ValidateAddElementExistToExistBus())
                znw.AddElementExistToExistBus(el6);

            var el7 = dEl["7"];
            el7.Sequence = 6;
            if (el7.ValidateAddElementExistToExistBus())
                znw.AddElementExistToExistBus(el7);

            #endregion

            return this;
        }

        class ZNetworkRef : ZNetwork
        {
            public ZNetworkRef()
            {
                // result Z matrix based
                // on elements being added
                // in a specific sequence
                var mx = new MxDTO<Complex>
                {
                    ID = "A",
                    RowSize = 4,
                    ColumnSize = 4,
                    EntriesType = MxDTO<double>.ROW_ENTRIES,
                    Entries = new Complex[]
                        {
                        C(0, 0.0705), C(0, 0.0295), C(0, 0.0420), C(0, 0.0483),
                        C(0, 0.0295), C(0, 0.0705), C(0, 0.0580), C(0, 0.0517),
                        C(0, 0.0420), C(0, 0.0580), C(0, 0.2041), C(0, 0.1271),
                        C(0, 0.0483), C(0, 0.0517), C(0, 0.1271), C(0, 0.1648),
                        }
                };

                Z = MX.ParseMatrix(mx);
            }
        }
    }

    public class ZNetwork2 : ZNetwork
    {
        public ZNetwork2()
        {
            Buses = new Dictionary<string, IZBus>
            {
                { "1", new ZBus { ID = "1", Data = new EEBus { Voltage = 1.05 } } },
                { "2", new ZBus { ID = "2", Data = new EEBus { Voltage = 1.05 } } },
                { "3", new ZBus { ID = "3", Data = new EEBus { Voltage = 1.05 } } },
                { "4", new ZBus { ID = "4", Data = new EEBus { Voltage = 1.05 } } },
                { "5", new ZBus { ID = "5", Data = new EEBus { Voltage = 1.05 } } },
            };

            Elements = new Dictionary<string, IEZElement>
            {
                { "1", new EZElement { ID = "1", ToBus = Buses["1"], Z = C(0, 0.045) } },
                { "2", new EZElement { ID = "2", ToBus = Buses["3"], Z = C(0, 0.0225) } },
                { "3", new EZElement { ID = "3", FromBus = Buses["2"], ToBus = Buses["4"], Z = C(0, 0.1) } },
                { "4", new EZElement { ID = "4", FromBus = Buses["2"], ToBus = Buses["5"], Z = C(0, 0.05) } },
                { "5", new EZElement { ID = "5", FromBus = Buses["4"], ToBus = Buses["5"], Z = C(0, 0.025) } },
                { "6", new EZElement { ID = "6", FromBus = Buses["1"], ToBus = Buses["5"], Z = C(0, 0.02) } },
                { "7", new EZElement { ID = "7", FromBus = Buses["3"], ToBus = Buses["4"], Z = C(0, 0.01) } },
            };

            RefZNetwork = new ZNetworkRef();
        }

        class ZNetworkRef : ZNetwork
        {
            public ZNetworkRef()
            {
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
                var mx = new MxDTO<Complex>
                {
                    ID = "A",
                    RowSize = 5,
                    ColumnSize = 5,
                    EntriesType = MxDTO<double>.ROW_ENTRIES,
                    Entries = new Complex[]
                        {
                        C(0, 0.0279725), C(0, 0.0177025), C(0, 0.0085125), C(0, 0.0122975), C(0, 0.020405),
                        C(0, 0.0177025), C(0, 0.0569525), C(0, 0.0136475), C(0, 0.019715), C(0, 0.02557),
                        C(0, 0.0085125), C(0, 0.0136475), C(0, 0.0182425), C(0, 0.016353), C(0, 0.012298),
                        C(0, 0.0122975), C(0, 0.019715), C(0, 0.016353), C(0, 0.0236), C(0, 0.017763),
                        C(0, 0.020405), C(0, 0.02557), C(0, 0.012298), C(0, 0.017763), C(0, 0.029475),
                        }
                };

                Z = MX.ParseMatrix(mx);
            }
        }

    }

}
