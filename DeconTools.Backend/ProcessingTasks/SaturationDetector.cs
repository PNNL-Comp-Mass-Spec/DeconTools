using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class SaturationDetector : Task
    {
        /// <summary>
        /// User-defined threshold that marks the intensity level above which detector saturation occurs.
        /// </summary>
        public double SaturationThreshold { get; set; }

        readonly TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();
        private double _minRelIntTheorProfile = 0.3;
        private readonly BasicTFF _basicFeatureFinder = new BasicTFF();

        private MSGenerator _msGenerator;

        public SaturationDetector()
        {
            SaturationThreshold = 1e5;   //NOTE: this is geared for the IMS4 detector that saturates at 1e5
        }

        public SaturationDetector(double saturationThreshold)
            : this()
        {
            SaturationThreshold = saturationThreshold;
        }

        public void GetUnsummedIntensitiesAndDetectSaturation(Run run, IEnumerable<IsosResult> resultList)
        {
            Check.Require(run != null, "SaturationDetector failed. Run is null");
            if (run == null)
                return;

            if (_msGenerator == null)
            {
                _msGenerator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            }

            if (run is UIMFRun uimfRun)
            {
                if (uimfRun.CurrentScanSet == null) throw new NullReferenceException("CurrentScanSet is null. You need to set it.");
                if (uimfRun.CurrentIMSScanSet == null) throw new NullReferenceException("CurrentIMSScanSet is null. You need to set it.");

                //this creates a FrameSet containing only the primary frame.  Therefore no summing will occur
                var lcScanSet = new ScanSet(uimfRun.CurrentScanSet.PrimaryScanNumber);

                //this creates a ScanSet containing only the primary scan.  Therefore no summing will occur
                var imsScanSet = new IMSScanSet(uimfRun.CurrentIMSScanSet.PrimaryScanNumber);

                //get the mass spectrum +/- 5 da from the range of the isotopicProfile

                uimfRun.CurrentScanSet = lcScanSet;
                uimfRun.CurrentIMSScanSet = imsScanSet;
                _msGenerator.Execute(run.ResultCollection);
            }
            else
            {
                if (run.CurrentScanSet == null)
                    throw new NullReferenceException("CurrentScanSet is null. You need to set it.");

                //this creates a ScanSet containing only the primary scan.  Therefore no summing will occur
                var scanSet = new ScanSet(run.CurrentScanSet.PrimaryScanNumber);

                run.CurrentScanSet = scanSet;

                _msGenerator.Execute(run.ResultCollection);
            }

            foreach (var result in resultList)
            {
                var indexOfObsMostAbundant = result.IsotopicProfile.GetIndexOfMostIntensePeak();

                var mzOfMostAbundant = result.IsotopicProfile.Peaklist[indexOfObsMostAbundant].XValue;

                var indexOfUnsummedMostAbundantMZ = run.XYData.GetClosestXVal(mzOfMostAbundant);
                if (indexOfUnsummedMostAbundantMZ >= 0)
                {
                    result.IsotopicProfile.OriginalIntensity = run.XYData.Yvalues[indexOfUnsummedMostAbundantMZ];
                    result.IsotopicProfile.IsSaturated = (result.IsotopicProfile.OriginalIntensity >=
                                                          SaturationThreshold);

                    if (result.IsotopicProfile.IsSaturated)
                    {
                        //problem is that with these saturated profiles, they are often truncated because another
                        //isotopic profile was falsely assigned to the back end of it. So we need to find more peaks that should
                        //belong to the saturated profile.
                        var theorTarget = new PeptideTarget
                        {
                            ChargeState = (short)result.IsotopicProfile.ChargeState,
                            MonoIsotopicMass = result.IsotopicProfile.MonoIsotopicMass
                        };

                        theorTarget.MZ = (theorTarget.MonoIsotopicMass / theorTarget.ChargeState) + Globals.PROTON_MASS;

                        var averagineFormula = _tomIsotopicPatternGenerator.GetClosestAvnFormula(result.IsotopicProfile.MonoIsotopicMass, false);
                        theorTarget.IsotopicProfile = _tomIsotopicPatternGenerator.GetIsotopePattern(averagineFormula, _tomIsotopicPatternGenerator.aafIsos);
                        theorTarget.EmpiricalFormula = averagineFormula;
                        theorTarget.CalculateMassesForIsotopicProfile(result.IsotopicProfile.ChargeState);

                        AssignMissingPeaksToSaturatedProfile(run.PeakList, result.IsotopicProfile, theorTarget.IsotopicProfile);

                        //goal is to find the index of the isotopic profile peaks of a peak that is not saturated

                        var indexOfGoodUnsaturatedPeak = -1;

                        for (var i = indexOfObsMostAbundant + 1; i < result.IsotopicProfile.Peaklist.Count; i++)
                        {
                            var indexUnsummedData = run.XYData.GetClosestXVal(result.IsotopicProfile.Peaklist[i].XValue);

                            var unsummedIntensity = run.XYData.Yvalues[indexUnsummedData];

                            if (unsummedIntensity < _minRelIntTheorProfile * SaturationThreshold)
                            {
                                indexOfGoodUnsaturatedPeak = i;
                                break;
                            }
                        }

                        AdjustSaturatedIsotopicProfile(result.IsotopicProfile, theorTarget.IsotopicProfile,
                                                       indexOfGoodUnsaturatedPeak);
                    }
                }

                //double summedIntensity = 0;
                //foreach (MSPeak peak in result.IsotopicProfile.Peaklist)
                //{
                //    int indexOfMZ = result.Run.XYData.GetClosestXVal(peak.XValue);
                //    if (indexOfMZ >= 0)
                //    {
                //        summedIntensity += result.Run.XYData.Yvalues[indexOfMZ];
                //    }
                //}

                //result.IsotopicProfile.OriginalTotalIsotopicAbundance = summedIntensity;

            }
        }

        public void AdjustSaturatedIsotopicProfile(IsotopicProfile iso, IsotopicProfile theorIsotopicProfile, int indexOfPeakUsedInExtrapolation)
        {
            var indexOfMostAbundantTheorPeak = theorIsotopicProfile.GetIndexOfMostIntensePeak();

            //find index of peaks lower than 0.3
            var indexOfReferenceTheorPeak = -1;

            for (var i = indexOfMostAbundantTheorPeak; i < theorIsotopicProfile.Peaklist.Count; i++)
            {
                if (theorIsotopicProfile.Peaklist[i].Height < _minRelIntTheorProfile)
                {
                    indexOfReferenceTheorPeak = i;
                    break;
                }
            }

            var useProvidedPeakIndex = (indexOfPeakUsedInExtrapolation >= 0);

            double intensityObsPeakUsedForExtrapolation;
            double intensityTheorPeak;
            if (useProvidedPeakIndex)
            {
                if (indexOfPeakUsedInExtrapolation < theorIsotopicProfile.Peaklist.Count && indexOfPeakUsedInExtrapolation < iso.Peaklist.Count)
                {
                    intensityObsPeakUsedForExtrapolation = iso.Peaklist[indexOfPeakUsedInExtrapolation].Height;
                    intensityTheorPeak = theorIsotopicProfile.Peaklist[indexOfPeakUsedInExtrapolation].Height;
                }
                else
                {
                    return;
                }
            }
            else if (indexOfReferenceTheorPeak != -1 && indexOfReferenceTheorPeak < iso.Peaklist.Count)
            {
                intensityObsPeakUsedForExtrapolation = iso.Peaklist[indexOfReferenceTheorPeak].Height;
                intensityTheorPeak = theorIsotopicProfile.Peaklist[indexOfReferenceTheorPeak].Height;
            }
            else
            {
                return;
            }

            iso.IntensityAggregateAdjusted = intensityObsPeakUsedForExtrapolation / intensityTheorPeak;
        }

        private void AssignMissingPeaksToSaturatedProfile(List<Peak> peakList, IsotopicProfile isotopicProfile, IsotopicProfile theorIsotopicProfile)
        {
            var toleranceInPPM = calcToleranceInPPMFromIsotopicProfile(isotopicProfile);

            //this finds the isotopic profile based on the theor. isotopic profile.
            _basicFeatureFinder.ToleranceInPPM = toleranceInPPM;
            _basicFeatureFinder.NeedMonoIsotopicPeak = false;

            var iso = _basicFeatureFinder.FindMSFeature(peakList, theorIsotopicProfile);

            if (iso?.Peaklist != null && iso.Peaklist.Count > 1)
            {
                //add the newly found peaks to the saturated isotopic profile
                for (var i = isotopicProfile.Peaklist.Count; i < iso.Peaklist.Count; i++)
                {
                    isotopicProfile.Peaklist.Add(iso.Peaklist[i]);
                }
            }
        }

        private double calcToleranceInPPMFromIsotopicProfile(IsotopicProfile isotopicProfile)
        {
            double toleranceInPPM = 20;
            if (isotopicProfile?.Peaklist == null || isotopicProfile.Peaklist.Count == 0)
            {
                return toleranceInPPM;
            }

            var fwhm = isotopicProfile.GetFWHM();
            var toleranceInMZ = fwhm / 2;

            toleranceInPPM = toleranceInMZ / isotopicProfile.MonoPeakMZ * 1e6;

            return toleranceInPPM;
        }

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList != null, "OriginalIntensitiesExtractor failed. ResultCollection is null");
            if (resultList == null)
                return;

            GetUnsummedIntensitiesAndDetectSaturation(resultList.Run, resultList.IsosResultBin);
        }
    }
}
