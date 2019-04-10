using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Data
{
    public class ScanResultFactory
    {

        public ScanResult CreateScanResult(Run run)
        {


            ScanResult scanResult;
            if (run is UIMFRun uimfRun)
            {
                scanResult = createUIMFScanResult(uimfRun, uimfRun.CurrentScanSet, uimfRun.CurrentIMSScanSet);
            }
            else
            {
                scanResult = createStandardScanResult(run, run.CurrentScanSet);
            }


            return scanResult;

        }

        private ScanResult createStandardScanResult(Run run, ScanSet scanSet)
        {
            ScanResult scanResult = new StandardScanResult(scanSet)
            {
                ScanTime = run.GetTime(scanSet.PrimaryScanNumber),
                SpectrumType = run.GetMSLevel(scanSet.PrimaryScanNumber),
                NumPeaks = scanSet.NumPeaks,
                NumIsotopicProfiles = scanSet.NumIsotopicProfiles,
                BasePeak = scanSet.BasePeak,
                TICValue = scanSet.TICValue,
                Description = run.GetScanInfo(scanSet.PrimaryScanNumber)
            };


            return scanResult;
        }

        private ScanResult createUIMFScanResult(UIMFRun run, ScanSet lcScanSet, ScanSet scanSet)
        {
            if (!(run is UIMFRun uimfRun))
            {
                Check.Require(false, "UIMFScanResults can only be created from UIMF files");
                return null;
            }

            Check.Require(run.IMSScanSetCollection != null && run.IMSScanSetCollection.ScanSetList.Count > 0, "ScanResult creator failed...ScanSetCollection is empty");
            Check.Require(run.ScanSetCollection != null && run.ScanSetCollection.ScanSetList.Count > 0, "ScanResult creator failed...FrameSetCollection is empty");

            var lcScanSetUIMF = (LCScanSetIMS)lcScanSet;

            UimfScanResult scanResult;

            var totPeaks = 0;
            var totIsotopicProfiles = 0;
            float tic = 0;
            var basePeak = new Peak();

            var currentScanListIsLastOne = (scanSet == uimfRun.IMSScanSetCollection.ScanSetList[uimfRun.IMSScanSetCollection.ScanSetList.Count - 1]);

            if (currentScanListIsLastOne)
            {
                foreach (var scanItem in uimfRun.IMSScanSetCollection.ScanSetList)
                {
                    if (!(scanItem is IMSScanSet imsScanSet))
                    {
                        continue;
                    }

                    totPeaks += imsScanSet.NumPeaks;
                    totIsotopicProfiles += imsScanSet.NumIsotopicProfiles;

                    tic += imsScanSet.TICValue;

                    if (imsScanSet.BasePeak.Height > basePeak.Height)
                    {
                        basePeak = imsScanSet.BasePeak;
                    }
                }
                scanResult = new UimfScanResult(lcScanSet)
                {
                    LCScanNum = lcScanSet.PrimaryScanNumber,
                    NumIsotopicProfiles = totIsotopicProfiles,
                    NumPeaks = totPeaks,
                    BasePeak = basePeak,
                    TICValue = tic,
                    ScanTime = run.GetTime(lcScanSet.PrimaryScanNumber),
                    SpectrumType = run.GetMSLevel(lcScanSet.PrimaryScanNumber),
                    FramePressureSmoothed = lcScanSetUIMF.FramePressureSmoothed,
                    FramePressureUnsmoothed = run.GetFramePressure(lcScanSet.PrimaryScanNumber)
                };

            }
            else
            {
                return null;
            }

            return scanResult;

        }

    }
}
