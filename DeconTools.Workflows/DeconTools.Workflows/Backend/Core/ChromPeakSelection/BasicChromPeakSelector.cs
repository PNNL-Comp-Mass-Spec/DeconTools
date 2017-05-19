using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public sealed class BasicChromPeakSelector : ChromPeakSelectorBase
    {
        #region Constructors


        public BasicChromPeakSelector(ChromPeakSelectorParameters parameters)
        {
            Parameters = parameters;
        }


        #endregion

        #region Properties


        //TODO:   figure out what uses this and why!   Default is 0 - that's all I know
        public int ScanOffSet { get; set; }

        public double ReferenceNETValueForReferenceMode { get; set; }


        public override Peak SelectBestPeak(List<ChromPeakQualityData> peakQualityList, bool filterOutFlaggedIsotopicProfiles)
        {
            throw new NotImplementedException();
        }

        public override ChromPeakSelectorParameters Parameters { get; set; }

        #endregion

       
        

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, "ChromPeakSelector failed. Mass Tag must be defined but it isn't.");
            Check.Require(resultList.Run.PeakList != null, "ChromPeakSelector failed. Peak list has not been established. You need to run a peak detector.");
            Check.Require(resultList.Run.PeakList.Count > 0, "ChromPeakSelector failed. Peak list is empty.");
            Check.Require(resultList.Run.PeakList[0] is ChromPeak, "ChromPeakSelector failed. Input peaklist contains the wrong type of peak");

            var result = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);


            float normalizedElutionTime;

            if (result.Run.CurrentMassTag.ElutionTimeUnit == DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum)
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.ScanLCTarget / (float)result.Run.GetNumMSScans();
            }
            else
            {
                normalizedElutionTime = resultList.Run.CurrentMassTag.NormalizedElutionTime;
            }


            var numPeaksWithinTolerance = 0;
            var bestPeak = (ChromPeak)selectBestPeak(Parameters.PeakSelectorMode,
                resultList.Run.PeakList, normalizedElutionTime,
                Parameters.NETTolerance, out numPeaksWithinTolerance);

            result.AddNumChromPeaksWithinTolerance(numPeaksWithinTolerance);

            result.Run.CurrentScanSet= ChromPeakUtilities.GetLCScanSetForChromPeak(bestPeak, resultList.Run, Parameters.NumScansToSum);

            UpdateResultWithChromPeakAndLCScanInfo(result, bestPeak);


        }

        [Obsolete("Do not use. Will delete in future")]
        public Peak selectBestPeak(DeconTools.Backend.Globals.PeakSelectorMode peakSelectorMode, List<Peak> chromPeakList, float targetNET, double netTolerance, out int numPeaksWithinTolerance)
        {
            var peaksWithinTol = new List<ChromPeak>(); // will collect Chrom peaks that fall within the NET tolerance



            foreach (ChromPeak peak in chromPeakList)
            {
                if (Math.Abs(peak.NETValue - targetNET) <= netTolerance)     //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }

            numPeaksWithinTolerance = peaksWithinTol.Count;


            ChromPeak bestPeak = null;

            switch (peakSelectorMode)
            {
                case DeconTools.Backend.Globals.PeakSelectorMode.ClosestToTarget:
                    var diff = double.MaxValue;

                    for (var i = 0; i < peaksWithinTol.Count; i++)
                    {
                        var currentDiff = Math.Abs(peaksWithinTol[i].NETValue - targetNET);

                        if (currentDiff < diff)
                        {
                            diff = currentDiff;
                            bestPeak = peaksWithinTol[i];
                        }
                    }
                    break;
                case DeconTools.Backend.Globals.PeakSelectorMode.MostIntense:
                    double max = -1;
                    for (var i = 0; i < peaksWithinTol.Count; i++)
                    {
                        double currentIntensity = peaksWithinTol[i].Height;

                        if (currentIntensity > max)
                        {
                            max = currentIntensity;
                            bestPeak = peaksWithinTol[i];
                        }
                    }
                    break;
                case DeconTools.Backend.Globals.PeakSelectorMode.RelativeToOtherChromPeak:
                    diff = double.MaxValue;


                    for (var i = 0; i < peaksWithinTol.Count; i++)
                    {
                        var currentDiff = Math.Abs(peaksWithinTol[i].NETValue - ReferenceNETValueForReferenceMode);

                        if (currentDiff < diff)
                        {
                            diff = currentDiff;
                            bestPeak = peaksWithinTol[i];
                        }
                    }

                    break;

                case DeconTools.Backend.Globals.PeakSelectorMode.N15IntelligentMode:
                    diff = double.MaxValue;

                    //want to only consider peaks that are less than the target NET.  (N15 peptides elutes at the same NET or earlier). 

                    peaksWithinTol.Clear();

                    foreach (ChromPeak peak in chromPeakList)
                    {

                        var currentDiff = ReferenceNETValueForReferenceMode - peak.NETValue;

                        if ((currentDiff) >= 0 && currentDiff <= netTolerance)
                        {
                            peaksWithinTol.Add(peak);
                            if (currentDiff < diff)
                            {
                                diff = currentDiff;
                                bestPeak = peak;
                            }
                        }
                    }

                    numPeaksWithinTolerance = peaksWithinTol.Count;



                    break;
                default:
                    break;


            }

            return bestPeak;
        }



    }
}
