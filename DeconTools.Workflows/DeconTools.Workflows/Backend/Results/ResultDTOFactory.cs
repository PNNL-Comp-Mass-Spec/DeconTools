using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;

namespace DeconTools.Workflows.Backend.Results
{
    public static class ResultDTOFactory
    {

        public static void CreateTargetedResult(TargetedResultBase result, TargetedResultDTO resultLight)
		{
            writeStandardInfoToResult(resultLight, result);

        }

        public static TargetedResultDTO CreateTargetedResult(TargetedResultBase result)
        {
            TargetedResultDTO tr;
            if (result is MassTagResult)
            {
                tr = new UnlabelledTargetedResultDTO();
                writeStandardInfoToResult(tr, result);
                addAdditionalInfo(tr, result as MassTagResult);
            }
            else if (result is O16O18TargetedResultObject)
            {
                tr = new O16O18TargetedResultDTO();
                writeStandardInfoToResult(tr, result);
                addAdditionalInfo(tr, result as O16O18TargetedResultObject);
            }
            else if (result is N14N15_TResult)
            {
                tr = new N14N15TargetedResultDTO();
                writeStandardInfoToResult(tr, result);
                addAdditionalInfo(tr, result as N14N15_TResult);
            }
            else if (result is SipperLcmsTargetedResult)
            {
                tr = new SipperLcmsFeatureTargetedResultDTO();
                writeStandardInfoToResult(tr, result);
                addAdditionalInfo(tr, result as SipperLcmsTargetedResult);
            }
			else if (result is TopDownTargetedResult)
			{
				tr = new TopDownTargetedResultDTO();
				writeStandardInfoToResult(tr, result);
				addAdditionalInfo(tr, result as TopDownTargetedResult);
			}
            else
            {
                throw new NotImplementedException();
            }




            return tr;



        }

        private static void addAdditionalInfo(TargetedResultDTO tr, SipperLcmsTargetedResult result)
        {
            SipperLcmsFeatureTargetedResultDTO r = (SipperLcmsFeatureTargetedResultDTO)tr;
            r.MatchedMassTagID = ((LcmsFeatureTarget)result.Target).FeatureToMassTagID;
            r.AreaUnderDifferenceCurve = result.AreaUnderDifferenceCurve;
            r.AreaUnderRatioCurve = result.AreaUnderRatioCurve;
            r.AreaUnderRatioCurveRevised = result.AreaUnderRatioCurveRevised;
            r.RSquaredValForRatioCurve = result.RSquaredValForRatioCurve;

            r.ChromCorrelationMin = result.ChromCorrelationMin;
            r.ChromCorrelationMax = result.ChromCorrelationMax;
            r.ChromCorrelationAverage = result.ChromCorrelationAverage;
            r.ChromCorrelationMedian = result.ChromCorrelationMedian;
            r.ChromCorrelationStdev = result.ChromCorrelationStdev;
            r.NumCarbonsLabelled = result.NumCarbonsLabelled;
            r.PercentPeptideLabelled = result.PercentPeptideLabelled;
            r.PercentCarbonsLabelled = result.PercentCarbonsLabelled;
            r.NumHighQualityProfilePeaks = result.NumHighQualityProfilePeaks;
            r.LabelDistributionVals = result.LabelDistributionVals.ToArray();
        }


        private static void addAdditionalInfo(TargetedResultDTO tr, MassTagResult massTagResult)
        {
            // no other info needed now
        }

		private static void addAdditionalInfo(TargetedResultDTO tr, TopDownTargetedResult result)
		{
			var r = (TopDownTargetedResultDTO)tr;

			r.PrsmList = null;
			r.ChargeStateList = null;
			r.Quantitation = result.ChromPeakSelected != null ? result.ChromPeakSelected.Height : 0;
			
			r.MatchedMassTagID = ((LcmsFeatureTarget)result.Target).FeatureToMassTagID;
			//r.ProteinName = "";
			//r.ProteinMass = 0.0;
			r.PeptideSequence = result.Target.Code;
			r.MostAbundantChargeState = r.ChargeState;
			r.ChromPeakSelectedHeight = result.ChromPeakSelected != null ? result.ChromPeakSelected.Height : 0;
		}

        private static void addAdditionalInfo(TargetedResultDTO tr, N14N15_TResult result)
        {
            N14N15TargetedResultDTO r = tr as N14N15TargetedResultDTO;
            r.FitScoreN15 = (float)result.ScoreN15;
            r.IScoreN15 = (float)result.InterferenceScoreN15;
            r.IntensityN15 = result.IsotopicProfileLabeled == null ? 0f : (float)result.IsotopicProfileLabeled.IntensityAggregate;
            r.MonoMZN15 = result.IsotopicProfileLabeled == null ? 0 : result.IsotopicProfileLabeled.MonoPeakMZ;
            r.MonoMassCalibratedN15 = result.IsotopicProfileLabeled == null ? 0d : -1 * ((result.Target.IsotopicProfileLabelled.MonoIsotopicMass * tr.MassErrorInPPM / 1e6) - 
                result.Target.IsotopicProfileLabelled.MonoIsotopicMass);   // massError= (theorMZ-alignedObsMZ)/theorMZ * 1e6
            r.MonoMassN15 = result.IsotopicProfileLabeled == null ? 0 : result.IsotopicProfileLabeled.MonoIsotopicMass;
           
            r.NETN15 = (float) result.GetNETN15();
            r.NumChromPeaksWithinTolN15 = (short) result.NumChromPeaksWithinToleranceForN15Profile;
            r.NumQualityChromPeaksWithinTolN15 = (short) result.NumChromPeaksWithinToleranceForN15Profile;
            r.Ratio = (float) result.RatioN14N15;
            r.RatioContributionN14 = (float) result.RatioContributionN14;
            r.RatioContributionN15 = (float) result.RatioContributionN15;
            r.ScanN15 = result.GetScanNumN15();

            if (result.ChromPeakSelectedN15 != null)
            {
                double sigma = result.ChromPeakSelectedN15.Width / 2.35;
                r.ScanN15Start = (int)Math.Round(result.ChromPeakSelectedN15.XValue - sigma);
                r.ScanN15End = (int)Math.Round(result.ChromPeakSelectedN15.XValue + sigma);
            }

            


        }

        private static void addAdditionalInfo(TargetedResultDTO tr, O16O18TargetedResultObject result)
        {
            O16O18TargetedResultDTO r = (O16O18TargetedResultDTO)tr;
            r.IntensityI0 = getIntensityFromIso(result.IsotopicProfile, 0);
            r.IntensityI2 = getIntensityFromIso(result.IsotopicProfile, 2);
            r.IntensityI4 = getIntensityFromIso(result.IsotopicProfile, 4);
            r.IntensityTheorI0 = getIntensityFromIso(result.Target.IsotopicProfile, 0);
            r.IntensityTheorI2 = getIntensityFromIso(result.Target.IsotopicProfile, 2);
            r.IntensityTheorI4 = getIntensityFromIso(result.Target.IsotopicProfile, 4);
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




        public static void writeStandardInfoToResult(TargetedResultDTO tr, TargetedResultBase result)
        {
            if (result.Target == null)
            {
                throw new ArgumentNullException("Cannot create result object. MassTag is null.");
            }

            if (result.Run == null)
            {
                throw new ArgumentNullException("Cannot create result object. Run is null.");
            }


            tr.DatasetName = result.Run.DatasetName;
            tr.TargetID = result.Target.ID;
            tr.ChargeState = result.Target.ChargeState;
            tr.NumMSScansSummed = result.NumMSScansSummed;
            tr.EmpiricalFormula = result.Target.EmpiricalFormula;
            tr.Code = result.Target.Code;

            tr.IndexOfMostAbundantPeak = result.IsotopicProfile == null ? (short)0 : (short)result.IsotopicProfile.GetIndexOfMostIntensePeak();
            tr.Intensity = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.IntensityAggregate;
            tr.IntensityI0 = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.GetMonoAbundance();
            tr.IntensityMostAbundantPeak = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.getMostIntensePeak().Height;
            tr.IScore = (float)result.InterferenceScore;
            tr.MonoMass = result.IsotopicProfile == null ? 0d : result.IsotopicProfile.MonoIsotopicMass;
            tr.MonoMZ = result.IsotopicProfile == null ? 0d : result.IsotopicProfile.MonoPeakMZ;
            tr.MassErrorInPPM = result.IsotopicProfile == null ? 0d : result.GetMassAlignmentErrorInPPM();
            tr.MonoMassCalibrated = result.IsotopicProfile == null ? 0d : -1 * ((result.Target.MonoIsotopicMass * tr.MassErrorInPPM / 1e6) - result.Target.MonoIsotopicMass);   // massError= (theorMZ-alignedObsMZ)/theorMZ * 1e6

            tr.ScanLC = result.GetScanNum();
            tr.NET = (float)result.GetNET();
            tr.NETError = result.Target.NormalizedElutionTime - tr.NET;


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

            tr.ErrorDescription = result.ErrorDescription ?? "";


        }



    }
}
