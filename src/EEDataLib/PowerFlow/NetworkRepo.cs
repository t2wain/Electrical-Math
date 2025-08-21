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
                BusIndex = (int)row.GetDouble("Index"),
                BusType = row.GetBusType("Type"),
                Voltage = row.GetDouble("V"),
                Pload = row.GetDouble("Pl"),
                Qload = row.GetDouble("Ql"),
                Pgen = row.GetDouble("Pg"),
                Qgen = row.GetDouble("Qg"),
                Qmin = row.GetDouble("Qmin"),
                Qmax = row.GetDouble("Qmax"),
                Gshunt = row.GetDouble("Gshunt"),
                Bshunt = row.GetDouble("Bshunt"),
                VoltageResult = row.GetDouble("Vk"),
                AngleResult = row.GetDouble("Ak"),
                PResult = row.GetDouble("Pk"),
                QResult = row.GetDouble("Qk"),
                QgenResult = row.GetDouble("Qkgen"),
            };

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
                LineType = row.GetLineType("Type"),
                FromBusID = row["FromBusID"].ToString(),
                ToBusID = row["ToBusID"].ToString(),
                RSeries = row.GetDouble("Rseries"),
                XSeries = row.GetDouble("Xseries"),
                TxTap = (int)row.GetDouble("TxTap"),
                BShunt = row.GetDouble("Bshunt"),
                PResult = row.GetNullDouble("Pf"),
                QResult = row.GetNullDouble("Qf"),
                PResultReverse = row.GetNullDouble("Pr"),
                QResultReverse = row.GetNullDouble("Qr"),
            };

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

    static class DataRowViewExtensions
    {
        public static T Get<T>(this DataRowView row, string fieldName)
        {
            var v = row[fieldName];
            if (v != DBNull.Value)
                return (T)v;
            else return default;
        }

        public static double GetDouble(this DataRowView row, string fieldName)
        {
            if (row[fieldName] is double v)
                return v;
            else return 0;
        }

        public static double? GetNullDouble(this DataRowView row, string fieldName)
        {
            if (row[fieldName] is double v && Math.Abs(v) > 1e-100)
                return v;
            else return null;
        }

        public static BusTypeEnum GetBusType(this DataRowView row, string fieldName)
        {
            var btype = row[fieldName].ToString();
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

        public static LineTypeEnum GetLineType(this DataRowView row, string fieldName)
        {
            var ltype = row[fieldName].ToString();
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
    }
}
