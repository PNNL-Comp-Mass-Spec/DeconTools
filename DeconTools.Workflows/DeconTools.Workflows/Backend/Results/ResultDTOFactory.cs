using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;

namespace DeconTools.Workflows.Backend.Results
{
    public static class ResultDTOFactory
    {
        public static void CreateTargetedResult(TargetedResultBase result, TargetedResultDTO resultLight)
        {
            WriteStandardInfoToResult(resultLight, result);
        }

        public static TargetedResultDTO CreateTargetedResult(TargetedResultBase result)
        {
            TargetedResultDTO tr;
            if (result is MassTagResult mtResult)
            {
                tr = new UnlabeledTargetedResultDTO();
                WriteStandardInfoToResult(tr, mtResult);
                AddAdditionalInfo(tr, mtResult);
            }
            else if (result is O16O18TargetedResultObject o16o18Result)
            {
                tr = new O16O18TargetedResultDTO();
                WriteStandardInfoToResult(tr, o16o18Result);
                AddAdditionalInfo(tr, o16o18Result);
            }
            else if (result is N14N15_TResult n14n15Result)
            {
                tr = new N14N15TargetedResultDTO();
                WriteStandardInfoToResult(tr, n14n15Result);
                AddAdditionalInfo(tr, n14n15Result);
            }
            else if (result is SipperLcmsTargetedResult sipperTargetedResult)
            {
                tr = new SipperLcmsFeatureTargetedResultDTO();
                WriteStandardInfoToResult(tr, sipperTargetedResult);
                AddAdditionalInfo(tr, sipperTargetedResult);
            }
            else if (result is TopDownTargetedResult topDownTargetedResult)
            {
                tr = new TopDownTargetedResultDTO();
                WriteStandardInfoToResult(tr, topDownTargetedResult);
                AddAdditionalInfo(tr, topDownTargetedResult);
            }
            // ReSharper disable once IdentifierTypo
            else if (result is DeuteratedTargetedResultObject deuteratedTargetedResult)
            {
                tr = new DeuteratedTargetedResultDTO();
                WriteStandardInfoToResult(tr, deuteratedTargetedResult);
                AddAdditionalInfo(tr, deuteratedTargetedResult);
            }
            else
            {
                throw new NotImplementedException();
            }

            return tr;
        }

        private static void AddAdditionalInfo(TargetedResultDTO tr, SipperLcmsTargetedResult result)
        {
            if (!(tr is SipperLcmsFeatureTargetedResultDTO r))
                return;

            r.MatchedMassTagID = ((LcmsFeatureTarget)result.Target).FeatureToMassTagID;
            r.AreaUnderDifferenceCurve = result.AreaUnderDifferenceCurve;
            r.AreaUnderRatioCurve = result.AreaUnderRatioCurve;
            r.AreaUnderRatioCurveRevised = result.AreaUnderRatioCurveRevised;
            r.ChromCorrelationMin = result.ChromCorrelationMin;
            r.ChromCorrelationMax = result.ChromCorrelationMax;
            r.ChromCorrelationAverage = result.ChromCorrelationAverage;
            r.ChromCorrelationMedian = result.ChromCorrelationMedian;
            r.ChromCorrelationStdev = result.ChromCorrelationStDev;
            r.NumCarbonsLabeled = result.NumCarbonsLabeled;
            r.PercentPeptideLabeled = result.PercentPeptideLabeled;
            r.PercentCarbonsLabeled = result.PercentCarbonsLabeled;
            r.NumHighQualityProfilePeaks = result.NumHighQualityProfilePeaks;
            r.LabelDistributionVals = result.LabelDistributionVals?.ToArray();
            r.FitScoreLabeledProfile = result.FitScoreLabeledProfile;
            r.ContiguousnessScore = result.ContiguousnessScore;
            r.RSquaredValForRatioCurve = result.RSquaredValForRatioCurve;
        }

        private static void AddAdditionalInfo(TargetedResultDTO tr, MassTagResult massTagResult)
        {
            // no other info needed now
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void AddAdditionalInfo(TargetedResultDTO tr, TopDownTargetedResult result)
        {
            if (!(tr is TopDownTargetedResultDTO r))
                return;

            r.PrsmList = null;
            r.ChargeStateList = null;
            r.Quantitation = result.ChromPeakSelected?.Height ?? 0;

            r.MatchedMassTagID = ((LcmsFeatureTarget)result.Target).FeatureToMassTagID;
            //r.ProteinName = "";
            //r.ProteinMass = 0.0;
            r.PeptideSequence = result.Target.Code;
            r.MostAbundantChargeState = r.ChargeState;
            r.ChromPeakSelectedHeight = result.ChromPeakSelected?.Height ?? 0;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void AddAdditionalInfo(TargetedResultDTO tr, N14N15_TResult result)
        {
            if (!(tr is N14N15TargetedResultDTO r))
                return;

            r.FitScoreN15 = (float)result.ScoreN15;
            r.IScoreN15 = (float)result.InterferenceScoreN15;
            r.IntensityN15 = result.IsotopicProfileLabeled == null ? 0f : (float)result.IntensityAggregate;
            r.MonoMZN15 = result.IsotopicProfileLabeled?.MonoPeakMZ ?? 0;

            // massError= (theorMZ-alignedObsMZ)/theorMZ * 1e6
            r.MonoMassCalibratedN15 = result.IsotopicProfileLabeled == null ? 0d :
                                          -1 * (result.Target.IsotopicProfileLabeled.MonoIsotopicMass * tr.MassErrorBeforeCalibration / 1e6 -
                                                result.Target.IsotopicProfileLabeled.MonoIsotopicMass);

            r.MonoMassN15 = result.IsotopicProfileLabeled?.MonoIsotopicMass ?? 0;

            r.NETN15 = (float)result.GetNETN15();
            r.NumChromPeaksWithinTolN15 = (short)result.NumChromPeaksWithinToleranceForN15Profile;
            r.NumQualityChromPeaksWithinTolN15 = (short)result.NumChromPeaksWithinToleranceForN15Profile;
            r.Ratio = (float)result.RatioN14N15;
            r.RatioContributionN14 = (float)result.RatioContributionN14;
            r.RatioContributionN15 = (float)result.RatioContributionN15;
            r.ScanN15 = result.GetScanNumN15();

            if (result.ChromPeakSelectedN15 != null)
            {
                var sigma = result.ChromPeakSelectedN15.Width / 2.35;
                r.ScanN15Start = (int)Math.Round(result.ChromPeakSelectedN15.XValue - sigma);
                r.ScanN15End = (int)Math.Round(result.ChromPeakSelectedN15.XValue + sigma);
            }
        }

        private static void AddAdditionalInfo(TargetedResultDTO tr, O16O18TargetedResultObject result)
        {
            if (!(tr is O16O18TargetedResultDTO r))
                return;

            r.IntensityI0 = GetIntensityFromIso(result.IsotopicProfile, 0);
            r.IntensityI2 = GetIntensityFromIso(result.IsotopicProfile, 2);
            r.IntensityI4 = GetIntensityFromIso(result.IsotopicProfile, 4);
            r.IntensityTheorI0 = GetIntensityFromIso(result.Target.IsotopicProfile, 0);
            r.IntensityTheorI2 = GetIntensityFromIso(result.Target.IsotopicProfile, 2);
            r.IntensityTheorI4 = GetIntensityFromIso(result.Target.IsotopicProfile, 4);
            r.IntensityI4Adjusted = result.IntensityI4Adjusted;

            r.ChromCorrO16O18DoubleLabel = (float) (result.ChromCorrO16O18DoubleLabel ?? 0f);
            r.ChromCorrO16O18SingleLabel = (float)(result.ChromCorrO16O18SingleLabel ?? 0f);

            r.Ratio = result.RatioO16O18;
            r.RatioFromChromCorr = result.RatioO16O18FromChromCorr;
        }

        private static void AddAdditionalInfo(TargetedResultDTO tr, DeuteratedTargetedResultObject result)
        {
            if (!(tr is DeuteratedTargetedResultDTO r))
                return;

            r.HydrogenI0 = result.HydrogenI0;
            r.HydrogenI1 = result.HydrogenI1;
            r.HydrogenI2 = result.HydrogenI2;
            r.HydrogenI3 = result.HydrogenI3;
            r.HydrogenI4 = result.HydrogenI4;

            r.DeuteriumI0 = result.DeuteriumI0;
            r.DeuteriumI1 = result.DeuteriumI1;
            r.DeuteriumI2 = result.DeuteriumI2;
            r.DeuteriumI3 = result.DeuteriumI3;
            r.DeuteriumI4 = result.DeuteriumI4;

            r.TheoryI0 = result.TheoryI0;
            r.TheoryI1 = result.TheoryI1;
            r.TheoryI2 = result.TheoryI2;
            r.TheoryI3 = result.TheoryI3;
            r.TheoryI4 = result.TheoryI4;

            r.RawI0 = result.RawI0;
            r.RawI1 = result.RawI1;
            r.RawI2 = result.RawI2;
            r.RawI3 = result.RawI3;
            r.RawI4 = result.RawI4;

            r.LabelingEfficiency = result.LabelingEfficiency;
            r.RatioDH = result.RatioDH;
            r.IntensityI0HydrogenMono = result.IntensityI0HydrogenMono;
            r.IntegratedLcAbundance = result.IntegratedLcAbundance;
        }

        private static float GetIntensityFromIso(IsotopicProfile isotopicProfile, int indexOfPeak)
        {
            if (isotopicProfile?.Peaklist == null || isotopicProfile.Peaklist.Count == 0) return -1;

            if (indexOfPeak >= isotopicProfile.Peaklist.Count)
            {
                return 0;
            }

            return isotopicProfile.Peaklist[indexOfPeak].Height;
        }

        public static void WriteStandardInfoToResult(TargetedResultDTO tr, TargetedResultBase result)
        {
            if (result.Target == null)
            {
                throw new Exception("Cannot create result object. MassTag is null (result.Target)");
            }

            if (result.Run == null)
            {
                throw new Exception("Cannot create result object. Run is null (result.Run)");
            }

            tr.DatasetName = result.Run.DatasetName;
            tr.TargetID = result.Target.ID;
            tr.ChargeState = result.Target.ChargeState;
            tr.NumMSScansSummed = result.NumMSScansSummed;
            tr.EmpiricalFormula = result.Target.EmpiricalFormula;
            tr.Code = result.Target.Code;

            tr.IndexOfMostAbundantPeak = result.IsotopicProfile == null ? (short)0 : (short)result.IsotopicProfile.GetIndexOfMostIntensePeak();
            tr.Intensity = result.IsotopicProfile == null ? 0f : (float)result.IntensityAggregate;
            tr.IntensityI0 = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.GetMonoAbundance();
            tr.IntensityMostAbundantPeak = (float?)result.IsotopicProfile?.getMostIntensePeak().Height ?? 0f;
            tr.IScore = (float)result.InterferenceScore;
            tr.MonoMass = result.IsotopicProfile?.MonoIsotopicMass ?? result.Target.MonoIsotopicMass;
            tr.MonoMZ = result.IsotopicProfile?.MonoPeakMZ ?? result.Target.MZ;

            tr.MonoMassCalibrated = result.MonoIsotopicMassCalibrated;
            tr.MassErrorBeforeCalibration = result.MassErrorBeforeAlignment;
            tr.MassErrorAfterCalibration = result.MassErrorAfterAlignment;

            tr.ScanLC = result.GetScanNum();
            tr.NET = (float)result.GetNET();
            tr.NETError = result.Target.NormalizedElutionTime - tr.NET;

            tr.NumChromPeaksWithinTol = result.NumChromPeaksWithinTolerance;
            tr.NumQualityChromPeaksWithinTol = result.NumQualityChromPeaks;
            tr.FitScore = (float)result.Score;

            if (result.ChromPeakSelected != null)
            {
                var sigma = result.ChromPeakSelected.Width / 2.35;
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
