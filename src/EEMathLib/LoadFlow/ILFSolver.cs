using EEMathLib.DTO;
using System.Collections.Generic;

namespace EEMathLib.LoadFlow
{
    public interface ILFSolver
    {
        Result<LFResult> Solve(EENetwork network, double threshold = 0.001, int maxIteration = 20);
        Result<LFResult> Solve(EENetwork network, IEnumerable<BusResult> initBuses, 
            double threshold = 0.015, int maxIteration = 20);
    }
}
