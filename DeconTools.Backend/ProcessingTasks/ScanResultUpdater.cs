using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
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
            Check.Require(resultList.Run != null, "Run is null");


            bool scanIsMS2 = (resultList.Run.GetMSLevel(resultList.Run.CurrentScanSet.PrimaryScanNumber) == 2);



            if (!MS2_IsOutputted && scanIsMS2)
            {

            }
            else
            {

                ScanResultFactory srf = new ScanResultFactory();
                ScanResult scanresult = srf.CreateScanResult(resultList.Run);

                if (scanresult != null)
                {
                    resultList.ScanResultList.Add(scanresult);
                    //Console.WriteLine("ScanResult isotopic profiles = \t" + scanresult.NumIsotopicProfiles);
                }
            }

        }
    }
}
