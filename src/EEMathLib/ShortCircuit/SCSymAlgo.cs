using EEMathLib.ShortCircuit.Data;
using EEMathLib.ShortCircuit.ZMX;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MC = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>;

namespace EEMathLib.ShortCircuit
{
    public static class SCSymAlgo
    {
        /// <summary>
        /// Given a Z impedance matrix, calculate
        /// 3 phase symmetrical fault current at each bus.
        /// </summary>
        /// <param name="znw">Z Impedance matrix</param>
        /// <returns>A dictionary of bus ID key and its fault curent</returns>
        public static IDictionary<string, Complex> CalcCurrentAllBus(this ZNetwork znw)
        {
            var res = znw.Buses.Values.Aggregate(new Dictionary<string, Complex>(), (acc, bus) =>
            {
                var v = bus.Data?.Voltage ?? 1.0;
                var z = znw.Z[bus.BusIndex, bus.BusIndex];
                // calculate fault current at each bus
                acc.Add(bus.ID, v / z); 
                return acc;
            });
            return res;
        }

        /// <summary>
        /// Given a Z impedance matrix, calculate
        /// 3 phase symmetrical fault current at a faulted bus.
        /// </summary>
        /// <param name="znw">Z Impedance matrix</param>
        /// <param name="faultedBusId">Bus ID of a faulted bus</param>
        /// <returns>Fault current at a faulted bus</returns>
        public static Complex CalcCurrent(this ZNetwork znw, string faultedBusId)
        {
            var bfault = znw.Buses[faultedBusId];
            var z = znw.Z[bfault.BusIndex, bfault.BusIndex];
            var v = bfault.Data?.Voltage ?? 1.0;
            var ifault = v / z;
            return ifault;
        }

        /// <summary>
        /// Given a Z impedance matrix, calculate the voltage at each bus 
        /// during a 3 phase symmetrical fault current at a faulted bus.
        /// This calculation is based on Vdrop = Z * Ifault.
        /// </summary>
        /// <param name="znw">Z Impedance matrix</param>
        /// <param name="faultedBusId">Bus ID of a faulted bus</param>
        /// <param name="faultCurrent">Faulted current at faulted bus</param>
        /// <returns>Bus voltages of all buses during a fault at a faulted bus.
        /// The result is a column matrix of dimension Nx1. The matrix index 
        /// is the same as the bus index of ZNetwork.</returns>
        public static MC CalcBusesVoltage(this ZNetwork znw, string faultedBusId, Complex faultCurrent) 
        {
            var bfault = znw.Buses[faultedBusId];
            // pre-fault voltage at faulted bus
            var v = bfault.Data?.Voltage ?? 1.0;

            // current column matrix Nx1
            var mxI = MC.Build.Dense(znw.Buses.Count, 1);
            mxI[bfault.BusIndex, 0] = -faultCurrent;

            // Vdrop across branches between buses
            // Vdrop = Z * Ifault
            var mxVdrop = znw.Z * mxI; // Vdrop = Z * Ifault

            // Voltage at each bus during fault
            // Vbus = Vfault + Vdrop
            mxVdrop.MapInplace(e => v + e); // Vbus = Vfault + Vdrop
            return mxVdrop;
        }

        /// <summary>
        /// Given a Z impedance matrix, calculate the voltage at each bus 
        /// during a 3 phase symmetrical fault current at a faulted bus.
        /// This calculation is based on impedance ratios of branch
        /// impedance (zkn) and faulted bus impedance (znn).
        /// </summary>
        /// <param name="znw">Z Impedance matrix</param>
        /// <param name="faultedBusId">Bus ID of the faulted bus</param>
        /// <returns>Bus voltages of all buses during a fault at a faulted bus.
        /// The result is a column matrix of dimension Nx1. The matrix index 
        /// is the same as the bus index of ZNetwork.</returns>
        public static MC CalcBusesVoltage(this ZNetwork znw, string faultedBusId)
        {
            var bn = znw.Buses[faultedBusId];

            // pre-fault voltage at faulted bus
            var vf = bn.Data?.Voltage ?? 1.0;

            // fault impdendance at faulted bus
            var znn = znw.Z[bn.BusIndex, bn.BusIndex];

            var mxV = znw.Buses.Values.Aggregate(MC.Build.Dense(znw.Buses.Count, 1), (acc, bus) =>
            {
                // branch impedance between buses
                var bk = bus;
                var zkn = znw.Z[bk.BusIndex, bn.BusIndex];

                // znn is the total fault impedance and zkn branch impedance between buses.
                // And so the vdrop across the branch is a ratio of the impedances.
                acc[bus.BusIndex, 0] = vf - zkn / znn;

                return acc;
            });
            return mxV;
        }

        /// <summary>
        /// Given a Z impedance matrix, calculate the fault current flow
        /// from each bus contributed to the faulted bus during a
        /// 3 phase symmetrical fault at the faulted bus.
        /// </summary>
        /// <param name="znw">Z Impedance matrix</param>
        /// <param name="faultedBusId">Bus ID of the faulted bus</param>
        /// <returns>Fault currents from all buses flow to a faulted bus.
        /// The result is a column matrix of dimension Nx1. The matrix index 
        /// is the same as the bus index of ZNetwork.</returns>
        public static MC CalcBusFlow(this ZNetwork znw, string faultedBusId)
        {
            // voltage at each bus during fault
            var ifault = znw.CalcCurrent(faultedBusId);
            var mxBusV = CalcBusesVoltage(znw, faultedBusId, ifault);
            return znw.CalcBusFlow(faultedBusId, mxBusV);
        }

        /// <summary>
        /// Given a Z impedance matrix, calculate the fault current flow
        /// from each bus contributed to the faulted bus during a
        /// 3 phase symmetrical fault at the faulted bus.
        /// </summary>
        /// <param name="znw">Z Impedance matrix</param>
        /// <param name="faultedBusId">Bus ID of the faulted bus</param>
        /// <param name="busVoltages">Bus voltages during fault condition at faulted bus, 
        /// a column matrix of Nx1</param>
        /// <returns>Fault currents from all buses flow to a faulted bus.
        /// The result is a column matrix of dimension Nx1. The matrix index 
        /// is the same as the bus index of ZNetwork.</returns>
        public static MC CalcBusFlow(this ZNetwork znw, string faultedBusId, MC busVoltages)
        {
            var bn = znw.Buses[faultedBusId];

            var vf = bn.Data?.Voltage ?? 1.0;
            // voltage delta across branch 
            // between each bus to faulted bus
            var mxDv = vf - busVoltages;

            // current from across branch
            // between each bus to faulted bus
            var mxI = znw.Z.Inverse() * mxDv;

            return mxI;
        }

        /// <summary>
        /// Given a Z impedance matrix, calculate the fault current flow
        /// across each impedance element.
        /// </summary>
        /// <param name="znw">Z Impedance matrix</param>
        /// <param name="busVoltages">Bus voltages during fault condition at faulted bus, 
        /// a column matrix of Nx1</param>
        /// <returns>Fault currents across all elements (by element ID)  
        /// during a fault condition at a faulted bus.</returns>
        public static IDictionary<string, Complex> CalcElementFlow(this ZNetwork znw, MC busVoltages)
        {
            var dI = new Dictionary<string, Complex>();
            foreach (var el in znw.Elements.Values)
            {
                // voltage at from bus
                var fv = Complex.One;
                if (el.FromBus is IZBus fb)
                    fv = busVoltages[fb.BusIndex, 0];
                // voltage a to bus
                var tv = busVoltages[el.ToBus.BusIndex, 0];
                // I = Vdrop / Z
                var i = (tv - fv) / el.Z;
                dI.Add(el.ID, i);
            }
            return dI;
        }

    }
}
