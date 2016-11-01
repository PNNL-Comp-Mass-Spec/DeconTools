using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            List<ChargeCorrelationItem> correlationList = new List<ChargeCorrelationItem>();
            List<ChromPeakIqTarget> availableTargets = new List<ChromPeakIqTarget>(targetList);
            //Sorts target list by fit score
            //Lowest to Highest
            var sortedTargetList = targetList.OrderBy(n => n.GetResult().FitScore);

            foreach (ChromPeakIqTarget referenceTarget in sortedTargetList)
            {
                //Checks to see if reference peak is in availableTargets list
                if (!availableTargets.Contains(referenceTarget)) continue;

                //New ChargeCorrelationItem for the reference peak
                ChargeCorrelationItem referenceCorrelationData = new ChargeCorrelationItem(referenceTarget);

                //Gets the ChromPeaks base width in scans for correlation
                int startScan, stopScan;
                GetBaseScanRange(referenceTarget.ChromPeak, out startScan, out stopScan);

                //Generates an array of XICs for the reference peak
                var referenceMZList = DeconTools.Backend.Utilities.IsotopicProfileUtilities.GetTopNMZValues(referenceTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
                XYData[] referenceXIC = GetCorrelationXICs(peaksToCorrelate, referenceMZList, run, startScan, stopScan);

                //Iterates through the rest of the available targets
                foreach (ChromPeakIqTarget correlatingTarget in targetList)
                {
                    //Checks if target is available
                    if (!availableTargets.Contains(correlatingTarget)) continue;

                    //Checks if the peak to be correlated has an XValue that falls within the base peak range of the reference peak
                    if (correlatingTarget.ChromPeak.XValue > startScan && correlatingTarget.ChromPeak.XValue < stopScan)
                    {
                        //Generates an XIC array for the peak being correlated. 
                        var correlationMZList =
                            DeconTools.Backend.Utilities.IsotopicProfileUtilities.GetTopNMZValues(
                                correlatingTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
                        XYData[] correlationXIC = GetCorrelationXICs(peaksToCorrelate, correlationMZList, run, startScan, stopScan);

                        //Generates new correlation data item for current correlation
                        ChromCorrelationData correlationData = new ChromCorrelationData();
                        for (int i = 0; i < peaksToCorrelate; i++)
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
            ChargeCorrelationData chargeCorrelationData = new ChargeCorrelationData();
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
            var referenceMZList = DeconTools.Backend.Utilities.IsotopicProfileUtilities.GetTopNMZValues(referenceTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
            XYData[] referenceXIC = GetCorrelationXICs(peaksToCorrelate, referenceMZList, run, startScan, stopScan);

            if (compTarget.ChromPeak.XValue > startScan && compTarget.ChromPeak.XValue < stopScan)
            {
                //Generates an XIC array for the peak being correlated. 
                var correlationMZList = DeconTools.Backend.Utilities.IsotopicProfileUtilities.GetTopNMZValues(compTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
                XYData[] correlationXIC = GetCorrelationXICs(peaksToCorrelate, correlationMZList, run, startScan, stopScan);

                //Generates new correlation data item for current correlation
                double[] rsquaredVals = new double[peaksToCorrelate];
                for (int i = 0; i < peaksToCorrelate; i++)
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
            double baseWidth = (4 * (referencePeak.Width / 2.35));
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
            XYData[] XICArray = new XYData[peaksToCorrelate];
            int index = 0;
            foreach (double mz in MZList)
            {
                XICArray[index] = Smoother.Smooth(_chromGen.GenerateChromatogram(run, startScan, stopScan, mz, 20));
                index++;
            }
            return XICArray;
        }

        #endregion

    }
}
