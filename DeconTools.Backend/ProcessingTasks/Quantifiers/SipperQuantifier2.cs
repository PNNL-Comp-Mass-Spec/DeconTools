using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.Quantifiers
{
    public class SipperQuantifier2:Task
    {
        private double _chromScanWindowWidth;

        private ChromatogramCorrelatorTask _chromatogramCorrelatorTask;

        #region Constructors

        public SipperQuantifier2()
        {
            MaximumFitScoreForFurtherProcessing = 0.15;
            MinimumRatioAreaForFurtherProcessing = 5;
            MinimumRelativeIntensityForChromCorr = 0.025;
            MinimumRSquaredValForQuant = 0.75;

            _chromatogramCorrelatorTask = new ChromatogramCorrelatorTask();
            _chromatogramCorrelatorTask.ChromToleranceInPPM = 25;
            ChromatogramRSquaredVals = new List<double>();

        }

        #endregion

        #region Properties
        protected double MaximumFitScoreForFurtherProcessing { get; set; }
        protected double MinimumRatioAreaForFurtherProcessing { get; set; }
        protected double ChromToleranceInPPM { get; set; }
        public double MinimumRelativeIntensityForChromCorr { get; set; }
        public List<double> ChromatogramRSquaredVals { get; set; }

        public IsotopicProfile NormalizedIso { get; set; }

        /// <summary>
        /// Normalized Isotopic profile but intensities adjusted using ChromatogramCorrelationData
        /// </summary>
        public IsotopicProfile NormalizedAdjustedIso { get; set; }
        protected double MinimumRSquaredValForQuant { get; set; }

        protected double MinimumRelativeIntensityForRatioCalc { get; set; }


        #endregion

        #region Public Methods



        #endregion

        #region Private Methods

        #endregion

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.CurrentTargetedResult is SipperLcmsTargetedResult, "Sipper Quantifier only works on Sipper-type result objects");
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag is empty");
            Check.Require(resultList.Run.CurrentMassTag.IsotopicProfile != null, this.Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");


            SipperLcmsTargetedResult result = (SipperLcmsTargetedResult)resultList.CurrentTargetedResult;
            result.AreaUnderDifferenceCurve = -9999;
            result.AreaUnderRatioCurve = -9999;


            ChromatogramRSquaredVals.Clear();


            Check.Require(result != null, "No MassTagResult has been generated for CurrentMassTag");

            if (result.IsotopicProfile == null || result.IsotopicProfile.Peaklist == null || result.IsotopicProfile.Peaklist.Count < 2)
            {

                return;
            }

            var theorUnlabelledIso = resultList.Run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileUtilities.NormalizeIsotopicProfile(theorUnlabelledIso);

            int indexMostAbundantTheorPeak = theorUnlabelledIso.GetIndexOfMostIntensePeak();

            NormalizedIso = result.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileUtilities.NormalizeIsotopicProfileToSpecificPeak(NormalizedIso, indexMostAbundantTheorPeak);



            if (result.Flags.Count > 0)
            {

                string flagstring = "";

                foreach (var resultFlag in result.Flags)
                {
                    flagstring += resultFlag.Description + ";";
                }

                result.ErrorDescription = flagstring.TrimEnd(';');
            }


            bool resultPassesMinimalCriteria = (result.Score < MaximumFitScoreForFurtherProcessing && result.Flags.Count == 0);





            if (resultPassesMinimalCriteria)
            {
                _chromScanWindowWidth = result.ChromPeakSelected.Width * 2;

                int startScan = result.ScanSet.PrimaryScanNumber - (int)Math.Round(_chromScanWindowWidth / 2, 0);
                int stopScan = result.ScanSet.PrimaryScanNumber + (int)Math.Round(_chromScanWindowWidth / 2, 0);

                ChromCorrelationData chromCorrelationData = _chromatogramCorrelatorTask.CorrelatePeaksWithinIsotopicProfile(resultList.Run, NormalizedIso, startScan,
                                                                                stopScan);

                ChromatogramRSquaredVals.AddRange(chromCorrelationData.CorrelationDataItems.Select(p => p.CorrelationRSquaredVal.GetValueOrDefault(-1)).ToList());

                //trim off zeros
                for (int i = ChromatogramRSquaredVals.Count - 1; i >= 0; i--)
                {
                    if (ChromatogramRSquaredVals[i] <= 0)
                    {
                        ChromatogramRSquaredVals.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }

                result.ChromCorrelationMin = ChromatogramRSquaredVals.Min();

                if (ChromatogramRSquaredVals.Count > 1)
                {
                    //get the best rsquared value other than the base peak's rsquared value (which is always 1)
                    result.ChromCorrelationMax =
                        (from n in ChromatogramRSquaredVals orderby n descending select n).ToList().ElementAt(1);

                    result.ChromCorrelationMedian = MathUtils.GetMedian(ChromatogramRSquaredVals);
                    result.ChromCorrelationAverage = ChromatogramRSquaredVals.Average();

                    if (ChromatogramRSquaredVals.Count > 2)
                    {
                        result.ChromCorrelationStdev = MathUtils.GetStDev(ChromatogramRSquaredVals);
                    }
                    else
                    {
                        result.ChromCorrelationStdev = -1;
                    }



                }
                else
                {
                    result.ChromCorrelationMax = 1;
                }


                NormalizedAdjustedIso = NormalizedIso.CloneIsotopicProfile();

                UpdateIsoIntensitiesUsingChromCorrData(chromCorrelationData, NormalizedAdjustedIso);



            }


        }


        private void UpdateIsoIntensitiesUsingChromCorrData(ChromCorrelationData chromCorrelationData, IsotopicProfile iso)
        {
            int totalChromCorrDataItems = chromCorrelationData.CorrelationDataItems.Count;

            double heightMaxPeak = iso.getMostIntensePeak().Height;

            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                if (i < totalChromCorrDataItems)
                {

                    var correctedRelIntensity = chromCorrelationData.CorrelationDataItems[i].CorrelationSlope;

                    if (correctedRelIntensity.HasValue)
                    {
                        iso.Peaklist[i].Height = (float)(heightMaxPeak * correctedRelIntensity);
                    }



                }

            }


        }
    }
}
