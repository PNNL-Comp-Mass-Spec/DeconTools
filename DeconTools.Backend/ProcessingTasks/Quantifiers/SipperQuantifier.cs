using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.Quantifiers
{
    public class SipperQuantifier : Task
    {
        private ChromatogramCorrelator _chromatogramCorrelator;
        private DeconToolsSavitzkyGolaySmoother _smoother;
        private PeakChromatogramGenerator _peakChromGen;
        private double _chromScanWindowWidth;

        #region Properties
        protected double MaximumFitScoreForFurtherProcessing { get; set; }
        protected double MinimumRatioAreaForFurtherProcessing { get; set; }
        protected double ChromToleranceInPPM { get; set; }


        public double MinimumRelativeIntensityForChromCorr { get; set; }
        public List<double> ChromatogramRSquaredVals { get; set; }

        #endregion


        #region Constructors
        public SipperQuantifier()
        {
            MaximumFitScoreForFurtherProcessing = 0.1;
            MinimumRatioAreaForFurtherProcessing = 5;
            MinimumRelativeIntensityForChromCorr = 0.025;

            _chromatogramCorrelator = new ChromatogramCorrelator();

            _smoother = new DeconToolsSavitzkyGolaySmoother(1, 1, 2);


            _peakChromGen = new PeakChromatogramGenerator(ChromToleranceInPPM);


            ChromToleranceInPPM = 25;
            ChromatogramRSquaredVals = new List<double>();

        }



        #endregion


        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl.CurrentTargetedResult is SipperLcmsTargetedResult, "Sipper Quantifier only works on Sipper-type result objects");
            Check.Require(resultColl.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag is empty");
            Check.Require(resultColl.Run.CurrentMassTag.IsotopicProfile != null, this.Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");


            SipperLcmsTargetedResult result = (SipperLcmsTargetedResult)resultColl.CurrentTargetedResult;
            result.AreaUnderDifferenceCurve = -9999;
            result.AreaUnderRatioCurve = -9999;

            
            ChromatogramRSquaredVals.Clear();

            Check.Require(result != null, "No MassTagResult has been generated for CurrentMassTag");

            if (result.IsotopicProfile == null || result.IsotopicProfile.Peaklist == null || result.IsotopicProfile.Peaklist.Count < 2)
            {
                
                return;
            }


            var unlabeledIso = resultColl.Run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileUtilities.NormalizeIsotopicProfile(unlabeledIso);

            //PeakUtilities.TrimIsotopicProfile(unlabeledIso, 0.001);


            var subtractedIso = result.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileUtilities.NormalizeIsotopicProfile(subtractedIso);

            int numTheoPeaks = unlabeledIso.Peaklist.Count;

            subtractedIso.Peaklist = subtractedIso.Peaklist.Take(numTheoPeaks).ToList();

            for (int i = 0; i < subtractedIso.Peaklist.Count; i++)
            {
                subtractedIso.Peaklist[i].Height = subtractedIso.Peaklist[i].Height -
                                                   unlabeledIso.Peaklist[i].Height;
            }

            subtractedIso.Peaklist = subtractedIso.Peaklist.Take(numTheoPeaks).ToList();


            for (int i = 0; i < subtractedIso.Peaklist.Count; i++)
            {
                
            }





            var xvals =
                subtractedIso.Peaklist.Select((p, i) => new { peak = p, index = i }).Select(n => (double)n.index).ToList();

            var yvals = subtractedIso.Peaklist.Select(p => (double)p.Height).ToList();

            result.AreaUnderDifferenceCurve = CalculateAreaUnderCubicSplineFit(xvals, yvals);

            for (int i = 0; i < subtractedIso.Peaklist.Count; i++)
            {
                subtractedIso.Peaklist[i].Height = (subtractedIso.Peaklist[i].Height / unlabeledIso.Peaklist[i].Height);
            }

            yvals = subtractedIso.Peaklist.Select(p => (double)p.Height).ToList();

            result.AreaUnderRatioCurve = CalculateAreaUnderCubicSplineFit(xvals, yvals);

            GetLinearRegressionData(result, xvals, yvals);


            bool resultPassesMinimalCriteria = (result.Score < MaximumFitScoreForFurtherProcessing &&
                                                result.AreaUnderRatioCurve > MinimumRatioAreaForFurtherProcessing);

            if (resultPassesMinimalCriteria)
            {
                int indexMostAbundantPeak = unlabeledIso.GetIndexOfMostIntensePeak();
                int maxPeakNum = Math.Min(numTheoPeaks, result.IsotopicProfile.Peaklist.Count);

                double baseMZValue = result.IsotopicProfile.Peaklist[indexMostAbundantPeak].XValue;

                _chromScanWindowWidth = result.ChromPeakSelected.Width *2 ;

                int startScan = result.ScanSet.PrimaryScanNumber - (int)Math.Round(_chromScanWindowWidth / 2, 0);
                int stopScan = result.ScanSet.PrimaryScanNumber + (int)Math.Round(_chromScanWindowWidth / 2, 0);

                _peakChromGen.GenerateChromatogram(resultColl.Run, startScan, stopScan, baseMZValue, ChromToleranceInPPM);


                var basePeakChromXYData = _smoother.Smooth(resultColl.Run.XYData);

                bool baseChromDataIsOK = basePeakChromXYData != null && basePeakChromXYData.Xvalues != null &&
                                     basePeakChromXYData.Xvalues.Length > 3;

                basePeakChromXYData = basePeakChromXYData.TrimData(startScan, stopScan);

                double minIntensity = result.IsotopicProfile.Peaklist[indexMostAbundantPeak].Height*
                                     MinimumRelativeIntensityForChromCorr;

                for (int i = 0; i < maxPeakNum; i++)
                {
                    if (!baseChromDataIsOK) break;

                    if (i == indexMostAbundantPeak)
                    {
                        //the rsquared val for the max peak will 1.0 (since the max peak is the base peak for the comparison)
                        ChromatogramRSquaredVals.Add(1.0);
                    }
                    else if (result.IsotopicProfile.Peaklist[i].Height >= minIntensity)
                    {
                        _peakChromGen.GenerateChromatogram(resultColl.Run, startScan, stopScan, result.IsotopicProfile.Peaklist[i].XValue, ChromToleranceInPPM);
                        var chromPeakXYData = _smoother.Smooth(resultColl.Run.XYData);

                        var  chromDataIsOK = chromPeakXYData != null && chromPeakXYData.Xvalues != null &&
                                     chromPeakXYData.Xvalues.Length > 3;

                        if (chromDataIsOK)
                        {
                            chromPeakXYData = chromPeakXYData.TrimData(startScan, stopScan);

                            double slope;
                            double intercept;
                            double rsquaredVal;
                            _chromatogramCorrelator.GetElutionCorrelationData(basePeakChromXYData, chromPeakXYData,
                                                                              out slope, out intercept, out rsquaredVal);

                            ChromatogramRSquaredVals.Add(rsquaredVal);
                        }
                        else
                        {
                            ChromatogramRSquaredVals.Add(0);
                        }

                    }
                    else
                    {
                        ChromatogramRSquaredVals.Add(0);
                    }
                }

                //trim off zeros
                for (int i = ChromatogramRSquaredVals.Count-1; i >= 0; i--)
                {
                    if (ChromatogramRSquaredVals[i]==0)
                    {
                        ChromatogramRSquaredVals.RemoveAt(i);
                    }
                }

                result.ChromCorrelationMin = ChromatogramRSquaredVals.Min();

                if (ChromatogramRSquaredVals.Count>1)
                {
                    //get the best rsquared value other than the base peak's rsquared value (which is always 1)
                    result.ChromCorrelationMax =
                        (from n in ChromatogramRSquaredVals orderby n descending select n).ToList().ElementAt(1);

                    result.ChromCorrelationMedian = MathUtils.GetMedian(ChromatogramRSquaredVals);
                    result.ChromCorrelationAverage = ChromatogramRSquaredVals.Average();

                    var revisedIsoPeaks =  subtractedIso.Peaklist.Take(ChromatogramRSquaredVals.Count).ToList();

                    xvals = revisedIsoPeaks.Select((p, i) => new { peak = p, index = i }).Select(n => (double)n.index).ToList();

                    yvals = revisedIsoPeaks.Select(p => (double)p.Height).ToList();

                    result.AreaUnderRatioCurveRevised = CalculateAreaUnderCubicSplineFit(xvals, yvals);


                }
                else
                {
                    result.ChromCorrelationMax = 1;
                }

                //Console.WriteLine();
                //Console.WriteLine(result);
                //foreach (var val in ChromatogramRSquaredVals)
                //{
                //    Console.WriteLine(val);
                //}
                

            }




        }




        private void GetLinearRegressionData(SipperLcmsTargetedResult result, List<double> xvals, List<double> yvals)
        {
            List<double> logRatioXVals = new List<double>();
            List<double> logRatioYVals = new List<double>();

            for (int i = 0; i < xvals.Count; i++)
            {
                if (yvals[i] > 0)
                {
                    logRatioXVals.Add(xvals[i]);
                    logRatioYVals.Add(Math.Log(yvals[i]));
                }
            }

            double slope = -9999;
            double intercept = -9999;
            double rsquaredVal = -1;

            if (logRatioYVals.Count > 2)
            {
                try
                {
                    MathUtils.GetLinearRegression(logRatioXVals.ToArray(), logRatioYVals.ToArray(), out slope, out intercept,
                                                  out rsquaredVal);
                }
                catch (Exception)
                {
                    Console.WriteLine(result.Target.ID + "; Error getting linear regression in Sipper quantifier");
                }
            }
            else
            {
                rsquaredVal = 0;
            }

            result.SlopeOfRatioCurve = slope;
            result.InterceptOfRatioCurve = intercept;
            result.RSquaredValForRatioCurve = rsquaredVal;
        }


        private double CalculateAreaUnderCubicSplineFit(List<double> xvals, List<double> yvals)
        {
            // var interp = Interpolate.RationalWithoutPoles(xvals, yvals);

            var interp = new MathNet.Numerics.Interpolation.Algorithms.CubicSplineInterpolation(xvals, yvals);

            double area = interp.Integrate(xvals.Max());

            return area;
        }

    }
}
