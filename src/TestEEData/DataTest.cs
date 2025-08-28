using EEDataLib.PowerFlow;
using EEMathLib.LoadFlow.Data;
using EEMathLib.LoadFlow.GaussSeidel;
using EEMathLib.LoadFlow.NewtonRaphson;
using EEMathLib.MatrixMath;

namespace TestEEData
{
    public class DataTest : IClassFixture<Context>
    {
        NetworkRepo _repo;

        public DataTest(Context ctx)
        {
            _repo = ctx.Repo;
        }

        #region N1

        [Fact]
        public void LoadFlowGS_Solve_Validate_Data1()
        {
            var data1 = _repo.GetNetworkData("N1");
            var c = GSExample.Solve(data1, true);
            Assert.True(c, "Load flow calculation failed");
        }

        #endregion

        #region N2

        [Fact]
        public void LoadFlowGS_Solve_Validate_Data2()
        {
            var data2 = _repo.GetNetworkData("N2");
            var nw = data2.CreateNetwork();

            var refData = new LFData2();
            nw.YMatrix = MX.ParseMatrix(refData.YResult);

            var c = GSExample.Solve(data2, true);
            Assert.True(c, "Load flow calculation failed");
        }

        #endregion

        #region N3

        [Fact]
        public void LoadFlowGS_Solve_Data3()
        {
            var data3 = _repo.GetNetworkData("N3");
            var c = GSExample.Solve(data3, false, 150);
            Assert.True(c, "Load flow calculation failed");
        }

        [Fact]
        public void LoadFlowGS_Solve_Validate_Data3()
        {
            var data3 = _repo.GetNetworkData("N3");
            var c = GSExample.Solve(data3, true, 150);
            Assert.True(c, "Load flow calculation failed");
        }

        [Fact]
        public void LoadFlowNR_Solve_Data3()
        {
            var data3 = _repo.GetNetworkData("N3");
            var c = NRExample.LFSolve(data3, 4, false);
            Assert.True(c);
        }

        #endregion

        #region N4

        [Fact]
        public void LoadFlowNR_Solve_Data4()
        {
            ILFData data4 = _repo.GetNetworkData("N4");

            var Y = data4.CreateNetwork().YMatrix;

            var c = NRExample.LFSolve(data4, 4, false);
            Assert.True(c);


        }

        #endregion
    }
}