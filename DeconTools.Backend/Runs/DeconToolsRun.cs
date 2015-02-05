using System;
using DeconTools.Backend.Core;


namespace DeconTools.Backend.Runs
{
#if !Disable_DeconToolsV2

    [Serializable]
    public abstract class DeconToolsRun : Run
    {

        public DeconToolsV2.Readers.clsRawData RawData { get; set; }


        public DeconToolsRun()
        {
            XYData = new XYData();

        }


        public override XYData XYData {get;set;}
        

        public override int GetNumMSScans()
        {
            if (RawData == null) return 0;
            return this.RawData.GetNumScans();

        }


        public override string GetScanInfo(int scanNum)
        {
            if (RawData == null)
            {
                return base.GetScanInfo(scanNum);
            }
            else
            {
                return this.RawData.GetScanDescription(scanNum);
            }
        }

        public override double GetTime(int scanNum)
        {
            return this.RawData.GetScanTime(scanNum);
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            try
            {
                return this.RawData.GetMSLevel(scanNum);

            }
            catch (Exception ex)
            {
                if (scanNum > this.GetMaxPossibleLCScanNum())
                {
                    throw new ArgumentOutOfRangeException("Failed to get MS level. Input scan was greater than dataset's max scan.");

                }
                else
                {
                    throw ex;
                }
                
            }
        }

        public override XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException();
        }
    }
#endif
}
