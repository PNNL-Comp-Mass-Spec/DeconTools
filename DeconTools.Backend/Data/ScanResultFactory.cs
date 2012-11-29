using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Data
{
    public class ScanResultFactory
    {

        public ScanResultFactory()
        {

        }

        public ScanResult CreateScanResult(Run run)
        {


            ScanResult scanresult;
            if (run is UIMFRun)
            {
                UIMFRun uimfRun = (UIMFRun)run;
                scanresult = createUIMFScanResult(uimfRun, uimfRun.CurrentFrameSet, uimfRun.CurrentIMSScanSet);
            }
            else
            {
                scanresult = createStandardScanResult(run, run.CurrentScanSet);
            }


            return scanresult;

        }

        private ScanResult createStandardScanResult(Run run, ScanSet scanSet)
        {
            

            ScanResult scanresult;
            
                scanresult = new StandardScanResult(scanSet);
                scanresult.ScanTime = run.GetTime(scanSet.PrimaryScanNumber);
                scanresult.SpectrumType = run.GetMSLevel(scanSet.PrimaryScanNumber);
                scanresult.NumPeaks = scanSet.NumPeaks;
                scanresult.NumIsotopicProfiles = scanSet.NumIsotopicProfiles;
                scanresult.BasePeak = scanSet.BasePeak;
                scanresult.TICValue = scanSet.TICValue;
                scanresult.Description = run.GetScanInfo(scanSet.PrimaryScanNumber);

            
            return scanresult;
        }

        private ScanResult createUIMFScanResult(UIMFRun run, ScanSet lcScanset, ScanSet scanSet)
        {
            Check.Require(run is UIMFRun, "UIMFScanResults can only be created from UIMF files");
            Check.Require(run.IMSScanSetCollection != null && run.IMSScanSetCollection.ScanSetList.Count > 0, "ScanResult creator failed...ScanSetCollection is empty");
            Check.Require(run.ScanSetCollection != null && run.ScanSetCollection.ScanSetList.Count > 0, "ScanResult creator failed...FrameSetCollection is empty");

            var lcscansetUIMF = (LCScanSetIMS) lcScanset;

            UimfScanResult scanresult;

            int totPeaks = 0;
            int totIsotopicProfiles = 0;
            float tic = 0;
            Peak basepeak = new Peak();

            bool currentScanListIsLastOne = (scanSet == run.IMSScanSetCollection.ScanSetList[run.IMSScanSetCollection.ScanSetList.Count - 1]);

            if (currentScanListIsLastOne)
            {
                foreach (IMSScanSet imsScanset in run.IMSScanSetCollection.ScanSetList)
                {
                    totPeaks += imsScanset.NumPeaks;
                    totIsotopicProfiles += imsScanset.NumIsotopicProfiles;

                    tic += imsScanset.TICValue;

                    if (imsScanset.BasePeak.Height > basepeak.Height)
                    {
                        basepeak = imsScanset.BasePeak;
                    }
                }
                scanresult = new UimfScanResult(lcScanset);

                scanresult.LCScanNum = lcScanset.PrimaryScanNumber;
                scanresult.NumIsotopicProfiles = totIsotopicProfiles;
                scanresult.NumPeaks = totPeaks;
                scanresult.BasePeak = basepeak;
                scanresult.TICValue = tic;
                scanresult.ScanTime = run.GetTime(lcScanset.PrimaryScanNumber);
                scanresult.SpectrumType = run.GetMSLevel(lcScanset.PrimaryScanNumber);
                scanresult.FramePressureSmoothed = lcscansetUIMF.FramePressureSmoothed;
                scanresult.FramePressureUnsmoothed = run.GetFramePressure(lcScanset.PrimaryScanNumber);

            }
            else
            {
                return null;
            }

            return scanresult;




        }



    }
}
