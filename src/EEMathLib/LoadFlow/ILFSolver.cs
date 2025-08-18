using EEMathLib.DTO;

namespace EEMathLib.LoadFlow
{
    public interface ILFSolver
    {
        Result<LFResult> Solve(EENetwork network, double threshold = 0.001, int maxIteration = 20);
    }
}
