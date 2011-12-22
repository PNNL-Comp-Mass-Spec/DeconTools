using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.DTO
{
    public class OriginalIntensitiesExtractorDecommissioned
    {
        ResultCollection results;

        public OriginalIntensitiesExtractorDecommissioned(ResultCollection results)
        {
            this.results = results;

        }

        public List<OriginalIntensitiesDTO> ExtractOriginalIntensities()
        {
            List<OriginalIntensitiesDTO> dto = new List<OriginalIntensitiesDTO>();
            
            foreach (IsosResult result in this.results.ResultList)
            {
                 OriginalIntensitiesDTO data = new OriginalIntensitiesDTO(result);

                if (result.Run is UIMFRun)
                {
                    UIMFIsosResult uimfresult = (UIMFIsosResult)result;
                    UIMFRun uimfRun = (UIMFRun)result.Run;

                    //this creates a Frameset containing only the primary frame.  Therefore no summing will occur
                    FrameSet frameset = new FrameSet(uimfresult.FrameSet.PrimaryFrame);
                    
                    //this creates a Scanset containing only the primary scan.  Therefore no summing will occur
                    ScanSet scanset = new ScanSet(uimfresult.ScanSet.PrimaryScanNumber);
                    
                    //get the mass spectrum +/- 5 da from the range of the isotopicProfile
                    uimfRun.GetMassSpectrum(frameset, scanset, uimfresult.IsotopicProfile.getMonoPeak().XValue - 5, uimfresult.IsotopicProfile.Peaklist.Last().XValue + 5);
                }
                else
                {
                    //this creates a Scanset containing only the primary scan.  Therefore no summing will occur
                    ScanSet scanset = new ScanSet(result.ScanSet.PrimaryScanNumber);
                    
                    //get the mass spectrum +/- 5 da from the range of the isotopicProfile
                    result.Run.GetMassSpectrum(scanset, result.IsotopicProfile.getMonoPeak().XValue - 5, result.IsotopicProfile.Peaklist.Last().XValue + 5);
                }

                double targetMZ = result.IsotopicProfile.MostAbundantIsotopeMass / result.IsotopicProfile.ChargeState + 1.00727649;


                int indexOfMostAbundantMZ  = result.Run.XYData.GetClosestXVal(targetMZ);
                if (indexOfMostAbundantMZ >= 0)
                {
                    data.originalIntensity = result.Run.XYData.Yvalues[indexOfMostAbundantMZ];
                }

                double summedIntensity = 0;
                foreach (MSPeak peak in result.IsotopicProfile.Peaklist)
                {
                    int indexOfMZ = result.Run.XYData.GetClosestXVal(peak.XValue);
                    if (indexOfMZ >= 0)
                    {
                        summedIntensity += result.Run.XYData.Yvalues[indexOfMZ];
                    }
                }

                data.totIsotopicOrginalIntens = summedIntensity;
                dto.Add(data);
            }
            return dto;

        }

    }
}
