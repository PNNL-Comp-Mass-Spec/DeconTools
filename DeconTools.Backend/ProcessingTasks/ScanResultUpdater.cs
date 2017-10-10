using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;


namespace DeconTools.Backend.ProcessingTasks
{
    public class ScanResultUpdater : Task
    {

        public bool MS2_IsOutputted { get; set; }

        public ScanResultUpdater()
        {
            MS2_IsOutputted = false;
        }

        public ScanResultUpdater(bool ms2IsOutputted)
        {
            MS2_IsOutputted = ms2IsOutputted;
        }


        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList != null, "ResultCollection is null");
            if (resultList == null)
                return;

            Check.Require(resultList.Run != null, "Run is null");
            if (resultList.Run == null)
                return;

            bool scanIsMS2;

            if (resultList.Run is UIMFRun uimfRun)
            {
                scanIsMS2 = (resultList.Run.GetMSLevel(uimfRun.CurrentScanSet.PrimaryScanNumber) == 2);       //GORD:  update this when you rename 'currentFrameSet'
            }
            else
            {
                scanIsMS2 = (resultList.Run.GetMSLevel(resultList.Run.CurrentScanSet.PrimaryScanNumber) == 2);
            }




            if (!MS2_IsOutputted && scanIsMS2)
            {

            }
            else
            {

                var srf = new ScanResultFactory();
                var scanresult = srf.CreateScanResult(resultList.Run);

                if (scanresult != null)
                {
                    resultList.ScanResultList.Add(scanresult);
                    //Console.WriteLine("ScanResult isotopic profiles = \t" + scanresult.NumIsotopicProfiles);
                }
            }

        }
    }
}
