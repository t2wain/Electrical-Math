using System;
using System.Numerics;

namespace EEMathLib.LoadFlow
{
    public class BusResult
    {
        public class ErrVal
        {
            public double VErr { get; set; }
            public double AErr { get; set; }
            public double PErr { get; set; }
            public double QErr { get; set; }
        }

        public BusResult()
        {
            Err = new ErrVal();
            ErrRef = new ErrVal();
        }
        public EEBus BusData { get; set; }
        public int BusIndex { get; set; }
        public string ID { get; set; }
        public BusTypeEnum BusType { get; set; }
        public Complex BusVoltage { get; set; }
        public Complex Sbus { get; set; }
        public ErrVal Err { get; set; }
        public ErrVal ErrRef { get; set; }

        public void UpdateVErr(double vcur, double vnxt, int iteration) => 
            UpdateVErr(vnxt - vcur, iteration);

        public void UpdateAErr(double acur, double anext, int iteration) => 
            UpdateAErr(anext - acur, iteration);

        public void UpdatePErr(double pcur, double pnext, int iteration) => 
            UpdatePErr(pnext - pcur, iteration);

        public void UpdateQErr(double qcur, double qnext, int iteration, bool setRef) => 
            UpdateQErr(qnext - qcur, iteration, setRef);

        public void UpdateVErr(double vdelta, int iteration)
        {
            Err.VErr = Math.Abs(vdelta);
            if (iteration == 1 || iteration % 6 == 0)
                ErrRef.VErr = Err.VErr;
        }

        public void UpdateAErr(double adelta, int iteration)
        {
            Err.AErr = Math.Abs(adelta);
            if (iteration == 1 || iteration % 6 == 0)
                ErrRef.AErr = Err.AErr;
        }

        public void UpdatePErr(double pdelta, int iteration)
        {
            Err.PErr = Math.Abs(pdelta);
            if (iteration == 1 || iteration % 6 == 0)
                ErrRef.PErr = Err.PErr;
        }

        public void UpdateQErr(double qdelta, int iteration, bool setRef)
        {
            Err.QErr = Math.Abs(qdelta);
            if (setRef)
                Err.QErr = 0;
            if (iteration == 1 || iteration % 6 == 0 || setRef)
                ErrRef.QErr = Err.QErr;
        }

    }
}
