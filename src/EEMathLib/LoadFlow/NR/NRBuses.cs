using BU = System.Collections.Generic.IEnumerable<EEMathLib.LoadFlow.BusResult>;

namespace EEMathLib.LoadFlow.NR
{
    public class NRBuses
    {
        public BU PQBuses { get; set; }
        public BU PVBuses { get; set; }
        public BU Buses { get; set; }
        public (int Row, int Col) J1Size { get; set; }
        public (int Row, int Col) J2Size { get; set; }
        public (int Row, int Col) J3Size { get; set; }
        public (int Row, int Col) J4Size { get; set; }
        public (int Row, int Col) JSize { get; set; }
    }
}
