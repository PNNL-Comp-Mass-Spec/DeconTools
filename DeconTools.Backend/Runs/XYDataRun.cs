using System;
using DeconTools.Backend.Core;


namespace DeconTools.Backend.Runs
{
    public abstract class XYDataRun :Run
    {
        
        protected XYDataRun()
        {
            xyData = new XYData();
            
        }



    
        private XYData xyData;
        public override XYData XYData
        {
            get
            {
                return xyData;
            }
            set
            {
                xyData = value;
            }
        }

        public override int GetNumMSScans()
        {
            if (xyData.Xvalues == null || xyData.Yvalues == null ||
                xyData.Xvalues.Length == 0 || xyData.Yvalues.Length == 0) return 0;
            
            return 1;      // there is only one MS scan in this type of Run
            
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return 1;
        }
        
        public override double GetTime(int scanNum)
        {
            throw new NotImplementedException();
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            return -1;
        }

        public override XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException();
        }
    }
}
