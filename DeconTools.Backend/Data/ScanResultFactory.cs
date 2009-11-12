using System;
using System.Collections.Generic;
using System.Text;
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
            bool is_ms_only = (run.GetMSLevel(scanSet.PrimaryScanNumber) == 1);
            ScanResult scanresult;
            if (is_ms_only)
            {
                scanresult = new StandardScanResult(scanSet);
                scanresult.ScanTime = run.GetTime(scanSet.PrimaryScanNumber);
                scanresult.SpectrumType = run.GetMSLevel(scanSet.PrimaryScanNumber);
                scanresult.NumPeaks = scanSet.NumPeaks;
                scanresult.NumIsotopicProfiles = scanSet.NumIsotopicProfiles;
                scanresult.BasePeak = scanSet.BasePeak;
                scanresult.TICValue = scanSet.TICValue;
                
            }
            else
            {
                scanresult = null;
            }
            return scanresult;
        }

        private ScanResult createUIMFScanResult(UIMFRun run, FrameSet frameSet, ScanSet scanSet)
        {
            Check.Require(run is UIMFRun, "UIMFScanResults can only be created from UIMF files");
            Check.Require(run.ScanSetCollection != null && run.ScanSetCollection.ScanSetList.Count > 0, "ScanResult creator failed...ScanSetCollection is empty");
            Check.Require(((UIMFRun)run).FrameSetCollection != null && ((UIMFRun)run).FrameSetCollection.FrameSetList.Count > 0, "ScanResult creator failed...FrameSetCollection is empty");

            UIMFScanResult scanresult;

            int totPeaks = 0;
            int totIsotopicProfiles = 0;
            float tic = 0;
            MSPeak basepeak = new MSPeak();

            bool currentScanListIsLastOne = (scanSet == run.ScanSetCollection.ScanSetList[run.ScanSetCollection.ScanSetList.Count - 1]);

            if (currentScanListIsLastOne)
            {
                foreach (ScanSet s in run.ScanSetCollection.ScanSetList)
                {
                    totPeaks += s.NumPeaks;
                    totIsotopicProfiles += s.NumIsotopicProfiles;

                    tic += s.TICValue;

                    if (s.BasePeak.Intensity > basepeak.Intensity)
                    {
                        basepeak = s.BasePeak;
                    }
                }
                scanresult = new UIMFScanResult(frameSet);
                scanresult.NumIsotopicProfiles = totIsotopicProfiles;
                scanresult.NumPeaks = totPeaks;
                scanresult.BasePeak = basepeak;
                scanresult.TICValue = tic;
                scanresult.ScanTime = run.GetTime(frameSet.PrimaryFrame);
                scanresult.SpectrumType = run.GetMSLevel(frameSet.PrimaryFrame);
                scanresult.FramePressureBack = run.GetFramePressureBack(frameSet.PrimaryFrame);
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
