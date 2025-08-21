using EEDataLib.Excel.ClosedXml;
using System.Data;

namespace TestEEData
{
    public class ExcelTest
    {
        string _fileName = "C:\\devgit\\Data\\NetworkData.xlsx";

        [Fact]
        public void Open_excel_file()
        {
            var fact = new ClosedXmlFactory();
            using var wb = fact.OpenWorkbook(_fileName);
            using var sht = wb.GetWorksheet("Bus");
            using var tbl = sht.GetTable("TBus");

            using var dt = tbl.ToDataTable();

            var dv = new DataView(dt);
            var nw = "N1";
            dv.RowFilter = $"NetworkID = '{nw}'";
            var rcnt = dv.Count;
        }
    }
}