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
                scanresult = createUIMFScanResult(uimfRun, uimfRun.CurrentFrameSet, uimfRun.CurrentScanSet);
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

        private ScanResult createUIMFScanResult(UIMFRun run, FrameSet frameSet, ScanSet scanSet)
        {
            Check.Require(run is UIMFRun, "UIMFScanResults can only be created from UIMF files");
            Check.Require(run.ScanSetCollection != null && run.ScanSetCollection.ScanSetList.Count > 0, "ScanResult creator failed...ScanSetCollection is empty");
            Check.Require(((UIMFRun)run).FrameSetCollection != null && ((UIMFRun)run).FrameSetCollection.FrameSetList.Count > 0, "ScanResult creator failed...FrameSetCollection is empty");

            UimfScanResult scanresult;

            int totPeaks = 0;
            int totIsotopicProfiles = 0;
            float tic = 0;
            Peak basepeak = new Peak();

            bool currentScanListIsLastOne = (scanSet == run.ScanSetCollection.ScanSetList[run.ScanSetCollection.ScanSetList.Count - 1]);

            if (currentScanListIsLastOne)
            {
                foreach (ScanSet s in run.ScanSetCollection.ScanSetList)
                {
                    totPeaks += s.NumPeaks;
                    totIsotopicProfiles += s.NumIsotopicProfiles;

                    tic += s.TICValue;

                    if (s.BasePeak.Height > basepeak.Height)
                    {
                        basepeak = s.BasePeak;
                    }
                }
                scanresult = new UimfScanResult(frameSet);

                scanresult.FrameNum = frameSet.PrimaryFrame;
                scanresult.NumIsotopicProfiles = totIsotopicProfiles;
                scanresult.NumPeaks = totPeaks;
                scanresult.BasePeak = basepeak;
                scanresult.TICValue = tic;
                scanresult.ScanTime = run.GetTime(frameSet.PrimaryFrame);
                scanresult.SpectrumType = run.GetMSLevel(frameSet.PrimaryFrame);
                scanresult.FramePressureBack = frameSet.FramePressure;
                scanresult.FramePressureFront = run.GetFramePressureFront(frameSet.PrimaryFrame);

            }
            else
            {
                return null;
            }

            return scanresult;




        }



    }
}
