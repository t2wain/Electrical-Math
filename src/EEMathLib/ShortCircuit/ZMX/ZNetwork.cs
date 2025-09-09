using EEMathLib.ShortCircuit.Data;
using System.Collections.Generic;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit.ZMX
{
    public class ZNetwork
    {
        public MC Z { get; set; }
        public IDictionary<string, IZBus> Buses { get; set; }
        public IDictionary<string, IEZElement> Elements { get; set; }
        public int LastBusIndex { get; set; } = -1;
        public int NextBusIndex => ++LastBusIndex;
        public IDictionary<string, IZBus> ExistBuses { get; set; }
        public IDictionary<string, IEZElement> RemainElements { get; set; }

        public bool ValidateAddElementRefToNewBus(IEZElement element) =>
            element.FromBus != null && !ExistBuses.ContainsKey(element.ToBus.ID);
        public void TrackAddElementRefToNewBus(IEZElement element)
        {
            var bus = element.ToBus;
            ExistBuses.Add(bus.ID, bus);
            RemainElements.Remove(element.ID);
        }

        public bool ValidateAddElementNewToExistBus(IEZElement element)
        {
            var fbId = element.FromBus.ID;
            var tbId = element.ToBus.ID;
            return (ExistBuses.ContainsKey(fbId) && !ExistBuses.ContainsKey(tbId))
                || (!ExistBuses.ContainsKey(fbId) && ExistBuses.ContainsKey(tbId));
        }
        public void TrackAddElementNewToExistBus(IEZElement element, IZBus newBus)
        {
            ExistBuses.Add(newBus.ID, newBus);
            RemainElements.Remove(element.ID);
        }

        public bool ValidateAddElementRefToExistBus(IEZElement element) =>
            element.FromBus == null && ExistBuses.ContainsKey(element.ToBus.ID);
        public void TrackAddElementRefToExistBus(IEZElement element) =>
            RemainElements.Remove(element.ID);

        public bool ValidateAddElementExistToExistBus(IEZElement element) =>
            ExistBuses.ContainsKey(element.FromBus.ID) && ExistBuses.ContainsKey(element.ToBus.ID);
        public void TrackAddElementExistToExistBus(IEZElement element) =>
            RemainElements.Remove(element.ID);
    }
}
