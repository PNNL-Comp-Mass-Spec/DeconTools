using System.Collections.Generic;
using DeconTools.Backend.Core;
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




        TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();
        private double _minRelIntTheorProfile = 0.3;
        private BasicTFF _basicFeatureFinder = new BasicTFF();

        private MSGenerator _msGenerator;


        public SaturationDetector()
        {
            SaturationThreshold = 1e5;   // gord: this is geared for the IMS4 detector that saturates at 1e5
        }


        public SaturationDetector(double saturationThreshold)
            : this()
        {
            SaturationThreshold = saturationThreshold;
        }

        public void GetUnsummedIntensitiesAndDetectSaturation(Run run, IEnumerable<IsosResult> resultList)
        {
            Check.Require(run != null, "SaturationDetector failed. Run is null");
            Check.Require(run.CurrentScanSet != null, "SaturationDetector failed. Current scanset has not been defined");

            if (_msGenerator == null)
            {
                _msGenerator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            }

            if (run is UIMFRun)
            {
                UIMFRun uimfRun = (UIMFRun)run;
                //this creates a Frameset containing only the primary frame.  Therefore no summing will occur
                FrameSet frameset = new FrameSet(uimfRun.CurrentFrameSet.PrimaryFrame);

                //this creates a Scanset containing only the primary scan.  Therefore no summing will occur
                ScanSet scanset = new ScanSet(uimfRun.CurrentScanSet.PrimaryScanNumber);

                //get the mass spectrum +/- 5 da from the range of the isotopicProfile

                uimfRun.CurrentFrameSet = frameset;
                uimfRun.CurrentScanSet = scanset;
                _msGenerator.Execute(run.ResultCollection);
                //uimfRun.GetMassSpectrum(frameset, scanset, run.MSParameters.MinMZ, run.MSParameters.MaxMZ);
            }
            else
            {
                //this creates a Scanset containing only the primary scan.  Therefore no summing will occur
                ScanSet scanset = new ScanSet(run.CurrentScanSet.PrimaryScanNumber);

                run.CurrentScanSet = scanset;

                _msGenerator.Execute(run.ResultCollection);
            }
            foreach (IsosResult result in resultList)
            {

                int indexOfObsMostAbundant = result.IsotopicProfile.GetIndexOfMostIntensePeak();

                double mzOfMostAbundant = result.IsotopicProfile.Peaklist[indexOfObsMostAbundant].XValue;

                int indexOfUnsummedMostAbundantMZ = result.Run.XYData.GetClosestXVal(mzOfMostAbundant);
                if (indexOfUnsummedMostAbundantMZ >= 0)
                {
                    result.IsotopicProfile.OriginalIntensity = result.Run.XYData.Yvalues[indexOfUnsummedMostAbundantMZ];
                    result.IsotopicProfile.IsSaturated = (result.IsotopicProfile.OriginalIntensity >=
                                                          SaturationThreshold);

                    if (result.IsotopicProfile.IsSaturated )
                    {
                        //problem is that with these saturated profiles, they are often trucated because another
                        //isotopic profile was falsely assigned to the back end of it. So we need to find more peaks that should 
                        //belong to the saturated profile. 
                        PeptideTarget theorTarget = new PeptideTarget();

                        theorTarget.ChargeState = (short)result.IsotopicProfile.ChargeState;
                        theorTarget.MonoIsotopicMass = result.IsotopicProfile.MonoIsotopicMass;
                        theorTarget.MZ = (theorTarget.MonoIsotopicMass / theorTarget.ChargeState) + Globals.PROTON_MASS;

                        var averagineFormula = _tomIsotopicPatternGenerator.GetClosestAvnFormula(result.IsotopicProfile.MonoIsotopicMass, false);
                        theorTarget.IsotopicProfile  = _tomIsotopicPatternGenerator.GetIsotopePattern(averagineFormula, _tomIsotopicPatternGenerator.aafIsos);
                        theorTarget.EmpiricalFormula = averagineFormula;
                        theorTarget.CalculateMassesForIsotopicProfile(result.IsotopicProfile.ChargeState);
                       
                        AssignMissingPeaksToSaturatedProfile(run.PeakList, result.IsotopicProfile,theorTarget.IsotopicProfile);


                        //goal is to find the index of the isotopic profile peaks of a peak that is not saturated

                        int indexOfGoodUnsaturatedPeak = -1;

                        for (int i = indexOfObsMostAbundant+1; i < result.IsotopicProfile.Peaklist.Count; i++)
                        {


                            int indexUnsummedData = result.Run.XYData.GetClosestXVal(result.IsotopicProfile.Peaklist[i].XValue);

                            var unsummedIntensity = result.Run.XYData.Yvalues[indexUnsummedData];


                            if (unsummedIntensity < _minRelIntTheorProfile*SaturationThreshold)
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


            int indexOfMostAbundantTheorPeak = theorIsotopicProfile.GetIndexOfMostIntensePeak();

            //find index of peaks lower than 0.3
            int indexOfReferenceTheorPeak = -1;

            for (int i = indexOfMostAbundantTheorPeak; i < theorIsotopicProfile.Peaklist.Count; i++)
            {
                if (theorIsotopicProfile.Peaklist[i].Height < _minRelIntTheorProfile)
                {
                    indexOfReferenceTheorPeak = i;
                    break;
                }

            }

            bool useProvidedPeakIndex = (indexOfPeakUsedInExtrapolation >= 0);

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
            double toleranceInPPM = calcToleranceInPPMFromIsotopicProfile(isotopicProfile);

            //this finds the isotopic profile based on the theor. isotopic profile.
            _basicFeatureFinder.ToleranceInPPM = toleranceInPPM;

            IsotopicProfile iso = _basicFeatureFinder.FindMSFeature(peakList, theorIsotopicProfile, toleranceInPPM, false);

            if (iso != null && iso.Peaklist != null && iso.Peaklist.Count > 1)
            {
                //add the newly found peaks to the saturated isotopic profile
                for (int i = isotopicProfile.Peaklist.Count; i < iso.Peaklist.Count; i++)
                {
                    isotopicProfile.Peaklist.Add(iso.Peaklist[i]);
                }


            }
        }

        private double calcToleranceInPPMFromIsotopicProfile(IsotopicProfile isotopicProfile)
        {
            double toleranceInPPM = 20;
            if (isotopicProfile == null || isotopicProfile.Peaklist == null || isotopicProfile.Peaklist.Count == 0)
            {
                return toleranceInPPM;
            }

            double fwhm = isotopicProfile.GetFWHM();
            double toleranceInMZ = fwhm / 2;

            toleranceInPPM = toleranceInMZ / isotopicProfile.MonoPeakMZ * 1e6;

            return toleranceInPPM;

        }



  

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList != null, "OriginalIntensitiesExtractor failed. ResultCollection is null");

            GetUnsummedIntensitiesAndDetectSaturation(resultList.Run, resultList.IsosResultBin);


        }
    }
}
