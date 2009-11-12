using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks
{
    public class OriginalIntensitiesExtractor : Task
    {




        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList != null, "OriginalIntensitiesExtractor failed. ResultCollection is null");
            Check.Require(resultList.Run != null, "OriginalIntensitiesExtractor failed. Run is null");
            Check.Require(resultList.Run.CurrentScanSet != null, "OriginalIntensitiesExtractor failed. Current scanset has not been defined");

            if (resultList.Run is UIMFRun)
            {
                UIMFRun uimfRun = (UIMFRun)resultList.Run;
                //this creates a Frameset containing only the primary frame.  Therefore no summing will occur
                FrameSet frameset = new FrameSet(uimfRun.CurrentFrameSet.PrimaryFrame);

                //this creates a Scanset containing only the primary scan.  Therefore no summing will occur
                ScanSet scanset = new ScanSet(uimfRun.CurrentScanSet.PrimaryScanNumber);

                //get the mass spectrum +/- 5 da from the range of the isotopicProfile
                uimfRun.GetMassSpectrum(scanset, frameset, resultList.Run.MSParameters.MinMZ, resultList.Run.MSParameters.MaxMZ);
            }
            else
            {
                //this creates a Scanset containing only the primary scan.  Therefore no summing will occur
                ScanSet scanset = new ScanSet(resultList.Run.CurrentScanSet.PrimaryScanNumber);

                //get the mass spectrum +/- 5 da from the range of the isotopicProfile
                resultList.Run.GetMassSpectrum(scanset, resultList.Run.MSParameters.MinMZ, resultList.Run.MSParameters.MaxMZ);
            }
            foreach (IsosResult result in resultList.CurrentScanIsosResultBin)
            {



                double targetMZ = result.IsotopicProfile.GetMZofMostAbundantPeak();


                int indexOfMostAbundantMZ = result.Run.XYData.GetClosestXVal(targetMZ);
                if (indexOfMostAbundantMZ >= 0)
                {
                    result.IsotopicProfile.OriginalIntensity = result.Run.XYData.Yvalues[indexOfMostAbundantMZ];
                }

                double summedIntensity = 0;
                foreach (MSPeak peak in result.IsotopicProfile.Peaklist)
                {
                    int indexOfMZ = result.Run.XYData.GetClosestXVal(peak.MZ);
                    if (indexOfMZ >= 0)
                    {
                        summedIntensity += result.Run.XYData.Yvalues[indexOfMZ];
                    }
                }

                result.IsotopicProfile.Original_Total_isotopic_abundance = summedIntensity;

            }
        }
    }
}
