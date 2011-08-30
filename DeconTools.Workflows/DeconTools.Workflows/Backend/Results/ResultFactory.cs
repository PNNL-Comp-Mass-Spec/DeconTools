using System;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Results
{
    public static class ResultFactory
    {

        public static TargetedResult CreateTargetedResult(MassTagResultBase result)
        {
            TargetedResult tr;
            if (result is MassTagResult)
            {
                tr = new UnlabelledTargetedResult();
                writeStandardInfoToResult(tr, result);
                addAdditionalInfo(tr, result as MassTagResult);
            }
            else if (result is O16O18_TResult)
            {
                tr = new O16O18TargetedResult();
                writeStandardInfoToResult(tr, result);
                addAdditionalInfo(tr, result as O16O18_TResult);
            }
            else if (result is N14N15_TResult)
            {
                tr = new N14N15TargetedResult();
                writeStandardInfoToResult(tr, result);
                addAdditionalInfo(tr, result as N14N15_TResult);
            }
            else
            {
                throw new NotImplementedException();
            }

       


            return tr;
            


        }


        private static void addAdditionalInfo(TargetedResult tr, MassTagResult massTagResult)
        {
            // no other info needed now
        }

        private static void addAdditionalInfo(TargetedResult tr, N14N15_TResult n14N15_TResult)
        {
            throw new NotImplementedException();
        }

        private static void addAdditionalInfo(TargetedResult tr, O16O18_TResult result)
        {
            O16O18TargetedResult r = (O16O18TargetedResult)tr;
            r.IntensityI0 = getIntensityFromIso(result.IsotopicProfile, 0);
            r.IntensityI2 = getIntensityFromIso(result.IsotopicProfile, 2);
            r.IntensityI4 = getIntensityFromIso(result.IsotopicProfile, 4);
            r.IntensityTheorI0 = getIntensityFromIso(result.MassTag.IsotopicProfile, 0);
            r.IntensityTheorI2 = getIntensityFromIso(result.MassTag.IsotopicProfile, 2);
            r.IntensityTheorI4 = getIntensityFromIso(result.MassTag.IsotopicProfile, 4);
            r.IntensityI4Adjusted = result.IntensityI4Adjusted;
            r.Ratio = result.RatioO16O18;
        }

        private static float getIntensityFromIso(IsotopicProfile isotopicProfile, int indexOfPeak)
        {
            if (isotopicProfile == null || isotopicProfile.Peaklist == null || isotopicProfile.Peaklist.Count == 0) return -1;

            if (indexOfPeak >= isotopicProfile.Peaklist.Count)
            {
                return 0;
            }
            else
            {
                return isotopicProfile.Peaklist[indexOfPeak].Height;
            }
        }

        


        private static void writeStandardInfoToResult(TargetedResult tr, MassTagResultBase result)
        {
            if (result.MassTag == null)
            {
                throw new ArgumentNullException("Cannot create result object. MassTag is null.");
            }

            if (result.Run == null)
            {
                throw new ArgumentNullException("Cannot create result object. Run is null.");
            }


            tr.DatasetName = result.Run.DatasetName;
            tr.MassTagID = result.MassTag.ID;
            tr.ChargeState = result.MassTag.ChargeState;

            tr.IndexOfMostAbundantPeak = result.IsotopicProfile == null ? (short)0 : (short)result.IsotopicProfile.getIndexOfMostIntensePeak();
            tr.Intensity = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.IntensityAggregate;
            tr.IntensityI0 = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.GetMonoAbundance();
            tr.IntensityMostAbundantPeak = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.getMostIntensePeak().Height;
            tr.IScore = (float)result.InterferenceScore;
            tr.MonoMass = result.IsotopicProfile == null ? 0d: result.IsotopicProfile.MonoIsotopicMass;
            tr.MonoMZ = result.IsotopicProfile == null ? 0d : result.IsotopicProfile.MonoPeakMZ;
            tr.MassErrorInPPM = result.IsotopicProfile == null ? 0d: result.GetMassAlignmentErrorInPPM();
            tr.MonoMassCalibrated = result.IsotopicProfile == null ? 0d : -1 * ((result.MassTag.MonoIsotopicMass * tr.MassErrorInPPM / 1e6) - result.MassTag.MonoIsotopicMass);   // massError= (theorMZ-alignedObsMZ)/theorMZ * 1e6

            tr.ScanLC = result.GetScanNum();
            tr.NET = (float)result.GetNET();
            tr.NETError = result.MassTag.NETVal - tr.NET;
            
            
            tr.NumChromPeaksWithinTol = result.NumChromPeaksWithinTolerance;
            tr.NumQualityChromPeaksWithinTol = result.NumQualityChromPeaks;
            tr.FitScore = (float)result.Score;
            
            if (result.ChromPeakSelected != null)
            {
                double sigma = result.ChromPeakSelected.Width / 2.35;
                tr.ScanLCStart = (int)Math.Round(result.ChromPeakSelected.XValue - sigma);
                tr.ScanLCEnd = (int)Math.Round(result.ChromPeakSelected.XValue + sigma);
            }

            if (result.FailedResult)
            {
                tr.FailureType = result.FailureType.ToString();
            }

        }

       

    }
}
