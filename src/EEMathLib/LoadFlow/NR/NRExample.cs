using EEMathLib.DTO;
using EEMathLib.MatrixMath;
using System.Linq;
using LFNR = EEMathLib.LoadFlow.NR.LFNewtonRaphson;
using LFC = EEMathLib.LoadFlow.LFCommon;
using FD = EEMathLib.LoadFlow.NR.LFFastDecoupled;
using EEMathLib.LoadFlow.Data;
using System;

namespace EEMathLib.LoadFlow.NR
{
    public class NRExample : IDisposable
    {
        ILFData _data;

        public NRExample(ILFData data)
        {
            _data = data;
        }

        public bool Calc_PQDelta()
        {
            var nw = _data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = LFNR.ReIndexBusPQ(buses);

            var bus2 = nrBuses.Buses.FirstOrDefault(b => b.ID == "2");

            var mxPQdelta = LFNR.CalcDeltaPQ(nw.YMatrix, nrBuses);
            var rowCount = mxPQdelta.RowCount;
            var c = rowCount == nrBuses.JSize.Row;

            var pdk = mxPQdelta[bus2.Pidx, 0];
            var res = -7.99972;
            c = c && Checker.EQ(pdk, res, 0.001);

            return c;
        }

        public bool Calc_J1()
        {
            var nw = _data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = LFNR.ReIndexBusPQ(buses);

            var J1 = Jacobian.CreateJ1(nw.YMatrix, nrBuses);
            var bus2 = nrBuses.Buses.FirstOrDefault(b => b.ID == "2");
            var bus4 = nrBuses.Buses.FirstOrDefault(b => b.ID == "4");
            var j24 = J1[bus2.Pidx, bus4.Aidx];
            var res = -9.91964;
            var c = Checker.EQ(j24, res, 0.001);
            c = c && J1.RowCount == nrBuses.J1Size.Row 
                && J1.ColumnCount == nrBuses.J1Size.Col;

            return c;
        }

        public bool Calc_JMatrix()
        {
            var nw = _data.CreateNetwork();
            var buses = LFNR.Initialize(nw.Buses);
            var nrBuses = LFNR.ReIndexBusPQ(buses);

            var J1 = Jacobian.CreateJ1(nw.YMatrix, nrBuses);
            var res = MX.ParseMatrix(_data.J1Result);
            var c1 = J1.ToColumnMajorArray().Zip(res.ToColumnMajorArray(), (j, r) =>
            {
                var v = Checker.EQ(j, r, 0.0001);
                return v;
            })
            .Any(v => !v);


            var J2 = Jacobian.CreateJ2(nw.YMatrix, nrBuses);
            var J3 = Jacobian.CreateJ3(nw.YMatrix, nrBuses);
            var J4 = Jacobian.CreateJ4(nw.YMatrix, nrBuses);

            var J = Jacobian.CreateJMatrix(nw.YMatrix, nrBuses);

            var c = Checker.EQ(J1[0, 0], J[0, 0], 0.0001);
            c = c && Checker.EQ(J2[0, 0], J[0, J1.ColumnCount], 0.0001);
            c = c && Checker.EQ(J3[0, 0], J[J1.RowCount, 0], 0.0001);
            c = c && Checker.EQ(J4[0, 0], J[J1.RowCount, J1.ColumnCount], 0.0001);

            return c;
        }

        public bool LFSolve()
        {

            var nw = _data.CreateNetwork();
            var threshold = 0.001;
            var res = LFNR.Solve(nw, threshold, 10, 3);

            if (res.Error == ErrorEnum.Divergence)
                return false;


            var rbuses = LFC.CalcResult(res.Data).ToDictionary(bus => bus.ID);
            var dbuses = _data.LFResult.ToDictionary(bus => bus.ID);

            var c = true;
            foreach (var dbus in dbuses.Values)
            {
                var rb = rbuses[dbus.ID];
                c = c && Checker.EQPct(rb.Voltage, dbus.Voltage, threshold);
                c = c && Checker.EQPct(rb.Angle, dbus.Angle, threshold);

                if (rb.BusType == BusTypeEnum.PQ
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    c = c && Checker.EQPct(rb.Pgen, dbus.Pgen, threshold);
                    c = c && Checker.EQPct(rb.Qgen, dbus.Qgen, threshold);
                }
            }

            return c;
        }

        public bool LFSolve_FastDecoupled()
        {

            var nw = _data.CreateNetwork();
            var threshold = 0.001;
            var res = FD.Solve(nw, threshold, 10, 3);

            if (res.Error == ErrorEnum.Divergence)
                return false;


            var rbuses = LFC.CalcResult(res.Data).ToDictionary(bus => bus.ID);
            var dbuses = _data.LFResult.ToDictionary(bus => bus.ID);

            var c = true;
            foreach (var dbus in dbuses.Values)
            {
                var rb = rbuses[dbus.ID];
                c = c && Checker.EQPct(rb.Voltage, dbus.Voltage, threshold);
                c = c && Checker.EQPct(rb.Angle, dbus.Angle, threshold);

                if (rb.BusType == BusTypeEnum.PQ
                    || rb.BusType == BusTypeEnum.Slack)
                {
                    c = c && Checker.EQPct(rb.Pgen, dbus.Pgen, threshold);
                    c = c && Checker.EQPct(rb.Qgen, dbus.Qgen, threshold);
                }
            }

            return c;
        }

        public void Dispose()
        {
            _data = null;
        }
    }
}
