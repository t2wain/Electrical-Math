using EEDataLib.Excel.ClosedXml;
using EEDataLib.Excel.Common;
using EEMathLib.LoadFlow.Data;
using System;
using System.Data;
using System.Linq;

namespace EEDataLib.PowerFlow
{
    public class NetworkData : LFDataAbstract
    {
        public NetworkData(DataView dvbus, DataView dvline)
        {
            PopulateBuses(dvbus);
            PopulateLines(dvline);
        }

        T Parse<T>(object v)
        {
            if (v != DBNull.Value)
                return (T)v;
            else return default;
        }

        double? ParseNullable(double v) =>
            Math.Abs(v) <= 0 ? null : (double?)v;

        #region Buses

        void PopulateBuses(DataView dvbus)
        {
            _Busses = dvbus
                .Cast<DataRowView>()
                .Select(row => CreateBus(row))
                .ToArray();
        }

        EEBus CreateBus(DataRowView row) =>
            new EEBus
            {
                ID = row["ID"].ToString(),
                BusIndex = (int)Parse<double>(row["Index"]),
                BusType = ParseBusType(row["Type"].ToString()),
                Voltage = Parse<double>(row["V"]),
                Pload = Parse<double>(row["Pl"]),
                Qload = Parse<double>(row["Ql"]),
                Pgen = Parse<double>(row["Pg"]),
                Qmin = Parse<double>(row["Qmin"]),
                Qmax = Parse<double>(row["Qmax"]),
                VoltageResult = Parse<double>(row["Vk"]),
                AngleResult = Parse<double>(row["Ak"]),
                QResult = Parse<double>(row["Qk"]),
                QgenResult = Parse<double>(row["Qkgen"]),
            };

        BusTypeEnum ParseBusType(string btype) 
        {
            switch (btype)
            {
                case "S":
                    return BusTypeEnum.Slack;
                case "PQ":
                    return BusTypeEnum.PQ;
                case "PV":
                    return BusTypeEnum.PV;
                default:
                    throw new Exception();
            }
        }

        #endregion

        #region Line

        void PopulateLines(DataView dvline)
        {
            _Lines = dvline
                .Cast<DataRowView>()
                .Select(row => CreateLine(row))
                .ToList();
        }

        EELine CreateLine(DataRowView row) =>
            new EELine
            {
                ID = row["ID"].ToString(),
                LineType = ParseLineType(row["Type"].ToString()),
                FromBusID = row["FromBusID"].ToString(),
                ToBusID = row["ToBusID"].ToString(),
                RSeries = Parse<double>(row["Rseries"]),
                XSeries = Parse<double>(row["Xseries"]),
                BShunt = Parse<double>(row["Bshunt"]),
                PResult = ParseNullable(Parse<double>(row["Pf"])),
                QResult = ParseNullable(Parse<double>(row["Qf"])),
                PResultReverse = ParseNullable(Parse<double>(row["Pr"])),
                QResultReverse = ParseNullable(Parse<double>(row["Qr"])),
            };

        LineTypeEnum ParseLineType(string ltype)
        {
            switch (ltype)
            {
                case "L":
                    return LineTypeEnum.Line;
                case "T":
                    return LineTypeEnum.Transformer;
                default:
                    throw new Exception();
            }
        }

        #endregion
    }

    public class NetworkRepo : IDisposable
    {
        DataTable _tbus;
        DataTable _tline;

        public void InitRepo(string fileName)
        {
            CleanUp();
            (_tbus, _tline) = GetDataTables(fileName);
        }

        static IExWorkbook OpenWorkbook(string fileName) =>
            new ClosedXmlFactory().OpenWorkbook(fileName);

        static (DataTable TBus, DataTable TLine) GetDataTables(string fileName)
        {
            using (var wb = OpenWorkbook(fileName))
            using (var wsBus = wb.GetWorksheet("Bus"))
            using (var wsLine = wb.GetWorksheet("Line"))
            {
                var tbus = wsBus.GetTable("TBus").ToDataTable();
                var tline = wsLine.GetTable("TLine").ToDataTable();
                return (tbus, tline);
            }
        }

        public ILFData GetNetworkData(string id)
        {
            var filter = $"NetworkID = '{id}'";
            var dvBus = new DataView(_tbus);
            dvBus.RowFilter = filter;
            var dvLine = new DataView(_tline);
            dvLine.RowFilter = filter;
            return new NetworkData(dvBus, dvLine);
        }

        public void CleanUp() 
        {
            if (_tbus != null)
                _tbus.Dispose();
            if (_tline != null)
                _tline.Dispose();
            _tline = null;
            _tline = null;
        }

        public void Dispose()
        {
            CleanUp();
        }
    }
}
