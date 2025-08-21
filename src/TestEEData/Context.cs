using EEDataLib.PowerFlow;

namespace TestEEData
{
    public class Context
    {
        public string FileName => "C:\\devgit\\Data\\NetworkData.xlsx";

        NetworkRepo _networkRepo = null!;
        public NetworkRepo Repo
        {
            get
            {
                if (_networkRepo == null)
                {
                    _networkRepo = new NetworkRepo();
                    _networkRepo.InitRepo(FileName);
                }
                return _networkRepo;
            }
        }
    }
}
