using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;


namespace DeconTools.Backend.Runs
{
    public abstract class XYDataRun :Run
    {
       
     
        private DeconTools.Backend.Globals.XYDataFileType fileType;

        public DeconTools.Backend.Globals.XYDataFileType FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }



        public XYDataRun()
        {
            this.xyData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
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
            else
            {
                return 1;      // there is only one MS scan in this type of Run
            }
        }




        public override double GetTime(int scanNum)
        {
            throw new NotImplementedException();
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            return -1;
        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException();
        }
    }
}
