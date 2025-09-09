using EEMathLib.DTO;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EEMathLib
{
    public class ENetwork : IENetwork
    {
        public virtual IEnumerable<IEBus> Buses { get; set;} = Enumerable.Empty<IEBus>();
        public virtual IEnumerable<IELine> Lines { get; set;} = Enumerable.Empty<IELine>();
        public virtual IEnumerable<IETransformer> Transformers { get; set; } = Enumerable.Empty<IETransformer>();
        public virtual IEnumerable<IEGen> Generators { get; set; } = Enumerable.Empty<IEGen>();
        public virtual IEnumerable<IELoad> Loads { get; set; } = Enumerable.Empty<IELoad>();
        public virtual Matrix<Complex> YMatrix { get; set; }
    }
}
