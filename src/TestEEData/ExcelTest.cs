using EEDataLib.Excel.ClosedXml;
using EEDataLib.PowerFlow;
using System.Data;

namespace TestEEData
{
    public class ExcelTest
    {
        [Fact]
        public void Open_excel_file()
        {
            var fact = new ClosedXmlFactory();
            using var wb = fact.OpenWorkbook("C:\\devgit\\Data\\NetworkData.xlsx");
            using var sht = wb.GetWorksheet("Bus");
            using var tbl = sht.GetTable("TBus");

            using var dt = tbl.ToDataTable();

            var dv = new DataView(dt);
            var nw = "N1";
            dv.RowFilter = $"NetworkID = '{nw}'";
            var rcnt = dv.Count;
        }

        [Fact]
        public void Open_DataRepo()
        {
            var repo = new NetworkRepo();
            repo.InitRepo("C:\\devgit\\Data\\NetworkData.xlsx");
            var data1 = repo.GetNetworkData("N1");
            var data2 = repo.GetNetworkData("N2");
        }
    }
}