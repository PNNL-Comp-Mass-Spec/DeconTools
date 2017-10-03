using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class IqChargeCorrelator : IqChromCorrelatorBase
    {
        #region Constructors

        public IqChargeCorrelator(int numPointsInSmoother, double minRelativeIntensityForChromCorr = 0.01, double chromToleranceInPPM = 20, DeconTools.Backend.Globals.ToleranceUnit toleranceUnit = DeconTools.Backend.Globals.ToleranceUnit.PPM) 
            : base(numPointsInSmoother, minRelativeIntensityForChromCorr, chromToleranceInPPM, toleranceUnit)
        {
            _chromGen = new PeakChromatogramGenerator();
        }

        #endregion

        #region Properties

        private readonly PeakChromatogramGenerator _chromGen;

        #endregion

        #region Public Methods


        /// <summary>
        /// Performs charge correlation on all peaks within a sequence level target
        /// </summary>
        /// <param name="targetList"></param>
        /// <param name="run"></param>
        /// <param name="correlationThreshold"></param>
        /// <param name="peaksToCorrelate"></param>
        /// <returns></returns>
        public ChargeCorrelationData CorrelateData(List<ChromPeakIqTarget> targetList, Run run, double correlationThreshold = 0.8, int peaksToCorrelate = 3)
        {
            var correlationList = new List<ChargeCorrelationItem>();
            var availableTargets = new List<ChromPeakIqTarget>(targetList);
            //Sorts target list by fit score
            //Lowest to Highest
            var sortedTargetList = targetList.OrderBy(n => n.GetResult().FitScore);

            foreach (var referenceTarget in sortedTargetList)
            {
                //Checks to see if reference peak is in availableTargets list
                if (!availableTargets.Contains(referenceTarget)) continue;

                //New ChargeCorrelationItem for the reference peak
                var referenceCorrelationData = new ChargeCorrelationItem(referenceTarget);

                //Gets the ChromPeaks base width in scans for correlation
                int startScan, stopScan;
                GetBaseScanRange(referenceTarget.ChromPeak, out startScan, out stopScan);

                //Generates an array of XICs for the reference peak
                var referenceMZList = IsotopicProfileUtilities.GetTopNMZValues(referenceTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
                var referenceXIC = GetCorrelationXICs(peaksToCorrelate, referenceMZList, run, startScan, stopScan);

                //Iterates through the rest of the available targets
                foreach (var correlatingTarget in targetList)
                {
                    //Checks if target is available
                    if (!availableTargets.Contains(correlatingTarget)) continue;

                    //Checks if the peak to be correlated has an XValue that falls within the base peak range of the reference peak
                    if (correlatingTarget.ChromPeak.XValue > startScan && correlatingTarget.ChromPeak.XValue < stopScan)
                    {
                        //Generates an XIC array for the peak being correlated. 
                        var correlationMZList =
                            IsotopicProfileUtilities.GetTopNMZValues(
                                correlatingTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
                        var correlationXIC = GetCorrelationXICs(peaksToCorrelate, correlationMZList, run, startScan, stopScan);

                        //Generates new correlation data item for current correlation
                        var correlationData = new ChromCorrelationData();
                        for (var i = 0; i < peaksToCorrelate; i++)
                        {
                            //Checks if either of the XICs are null
                            if (referenceXIC[i] != null && correlationXIC[i] != null)
                            {
                                double slope, intercept, rsquaredval;
                                GetElutionCorrelationData(referenceXIC[i],
                                                          FillInAnyMissingValuesInChromatogram(referenceXIC[i].Xvalues, correlationXIC[i]), out slope,
                                                          out intercept, out rsquaredval);
                                correlationData.AddCorrelationData(slope, intercept, rsquaredval);
                            }
                            else
                            {
                                //A placeholder to show that the data was poor
                                correlationData.AddCorrelationData(-9999, -9999, 0);
                            }
                        }

                        //Checks if the overall correlation median is higher than the correlation threshold
                        if (correlationData.RSquaredValsMedian >= correlationThreshold)
                        {
                            referenceCorrelationData.PeakCorrelationData.Add(correlatingTarget, correlationData);

                            //Removes correlating target from the availableTargets list because it was successfully correlated.
                            availableTargets.Remove(correlatingTarget);
                        }
                    }
                }
                //Removes reference target from availableTargets list
                availableTargets.Remove(referenceTarget);
                correlationList.Add(referenceCorrelationData);
            }
            var chargeCorrelationData = new ChargeCorrelationData();
            chargeCorrelationData.CorrelationData = correlationList;
            return chargeCorrelationData;
        }



        /// <summary>
        /// Performs charge correlation on a pair of ChromPeakIqTarget peaks
        /// </summary>
        /// <param name="referenceTarget"></param>
        /// <param name="compTarget"></param>
        /// <returns></returns>
        public double PairWiseChargeCorrelation(ChromPeakIqTarget referenceTarget, ChromPeakIqTarget compTarget, Run run, int peaksToCorrelate)
        {
            //Gets the ChromPeaks base width in scans for correlation
            int startScan, stopScan;
            GetBaseScanRange(referenceTarget.ChromPeak, out startScan, out stopScan);

            //Generates an array of XICs for the reference peak
            var referenceMZList = IsotopicProfileUtilities.GetTopNMZValues(referenceTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
            var referenceXIC = GetCorrelationXICs(peaksToCorrelate, referenceMZList, run, startScan, stopScan);

            if (compTarget.ChromPeak.XValue > startScan && compTarget.ChromPeak.XValue < stopScan)
            {
                //Generates an XIC array for the peak being correlated. 
                var correlationMZList = IsotopicProfileUtilities.GetTopNMZValues(compTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
                var correlationXIC = GetCorrelationXICs(peaksToCorrelate, correlationMZList, run, startScan, stopScan);

                //Generates new correlation data item for current correlation
                var rsquaredVals = new double[peaksToCorrelate];
                for (var i = 0; i < peaksToCorrelate; i++)
                {
                    //Checks if either of the XICs are null
                    if (referenceXIC[i] != null && correlationXIC[i] != null)
                    {
                        double slope, intercept, rsquaredval;
                        GetElutionCorrelationData(referenceXIC[i], FillInAnyMissingValuesInChromatogram(referenceXIC[i].Xvalues, correlationXIC[i]), out slope, out intercept, out rsquaredval);
                        rsquaredVals[i] = rsquaredval;
                    }
                    else
                    {
                        //A placeholder to show that the data was poor
                        rsquaredVals[i] = 0;
                    }
                }
                return MathUtils.GetMedian(rsquaredVals);
            }
            return 0;
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Gets the scan range for the peaks that need to be correlated together 
        /// </summary>
        /// <param name="referencePeak"></param>
        /// <param name="startScan"></param>
        /// <param name="stopScan"></param>
        private void GetBaseScanRange(ChromPeak referencePeak, out int startScan, out int stopScan)
        {
            var baseWidth = (4 * (referencePeak.Width / 2.35));
            startScan = Convert.ToInt32(Math.Floor(referencePeak.XValue - (0.5 * baseWidth)));
            stopScan = Convert.ToInt32(Math.Ceiling(referencePeak.XValue + (0.5 * baseWidth)));
        }


        /// <summary>
        /// Pulls all XICs needed to perform charge correlation
        /// </summary>
        /// <param name="peaksToCorrelate"></param>
        /// <param name="MZList"></param>
        /// <param name="run"></param>
        /// <param name="startScan"></param>
        /// <param name="stopScan"></param>
        /// <returns></returns>
        private XYData[] GetCorrelationXICs(int peaksToCorrelate, IEnumerable<double> MZList, Run run, int startScan, int stopScan)
        {
            var XICArray = new XYData[peaksToCorrelate];
            var index = 0;
            foreach (var mz in MZList)
            {
                XICArray[index] = Smoother.Smooth(_chromGen.GenerateChromatogram(run, startScan, stopScan, mz, 20));
                index++;
            }
            return XICArray;
        }

        #endregion

    }
}
