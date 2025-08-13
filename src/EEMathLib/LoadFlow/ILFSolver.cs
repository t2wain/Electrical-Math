using EEMathLib.DTO;
using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;

namespace EEMathLib.LoadFlow
{

    public interface ILFSolver
    {
        Result<BU> Solve(EENetwork network, double threshold = 0.001, int maxIteration = 20);
    }
}
