using EEDataLib.Excel.ClosedXml;
using EEDataLib.PowerFlow;
using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.GaussSeidel;
using EEMathLib.MatrixMath;
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

        [Fact]
        public void Open_DataRepo()
        {
            var repo = new NetworkRepo();
            repo.InitRepo(_fileName);
            var data1 = repo.GetNetworkData("N1");
            var data2 = repo.GetNetworkData("N2");
        }

        [Fact]
        public void LoadFlowGS_Solve_Data1()
        {
            var repo = new NetworkRepo();
            repo.InitRepo(_fileName);
            var data1 = repo.GetNetworkData("N1");
            var c = GSExample.Solve(data1, true);
            Assert.True(c, "Load flow calculation failed");
        }

        [Fact]
        public void LoadFlowGS_Solve_Data2()
        {
            var repo = new NetworkRepo();
            repo.InitRepo(_fileName);
            var data2 = repo.GetNetworkData("N2");
            var nw = data2.CreateNetwork();

            var refData = new LFData2();
            nw.YMatrix = MX.ParseMatrix(refData.YResult);

            var c = GSExample.Solve(data2, true);
            Assert.True(c, "Load flow calculation failed");
        }

    }
}