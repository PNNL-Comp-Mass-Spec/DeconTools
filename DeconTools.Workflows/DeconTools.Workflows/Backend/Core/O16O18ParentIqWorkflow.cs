using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using DeconTools.Workflows.Backend.FileIO;
using GWSGraphLibrary;
using ZedGraph;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18ParentIqWorkflow : IqWorkflow
    {

        protected PeakLeastSquaresFitter PeakFitter;

        private readonly DeconToolsPeakDetectorV2 _msPeakDetector;
        private bool _headerLogged;
        private BasicGraphControl _graphGenerator;

        #region Constructors

        public O16O18ParentIqWorkflow(Run run, TargetedWorkflowParameters parameters)
            : base(run, parameters)
        {
            PeakFitter = new PeakLeastSquaresFitter();
            _msPeakDetector = new DeconToolsPeakDetectorV2(parameters.MSPeakDetectorPeakBR,
                                                           parameters.MSPeakDetectorSigNoise,
                                                           DeconTools.Backend.Globals.PeakFitType.QUADRATIC,
                                                           run.IsDataThresholded);





        }

        public O16O18ParentIqWorkflow(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {

        }

        #endregion

        #region Properties

        public string OutputFolderForGraphs { get; set; }

        public bool GraphsAreOutputted { get; set; }

        #endregion

        #region Public Methods

        protected override void DoPostInitialization()
        {
            MsFeatureFinder = new O16O18IterativeTff(IterativeTffParameters);
            ChromatogramCorrelator = new O16O18ChromCorrelator(WorkflowParameters.ChromSmootherNumPointsInSmooth, 0.025,
                                                               WorkflowParameters.ChromGenTolerance,
                                                               WorkflowParameters.ChromGenToleranceUnit);

        }

        protected override void ExecuteWorkflow(IqResult result)
        {
            var children = result.Target.ChildTargets().ToList();
            foreach (var child in children)
            {
                child.DoWorkflow();
                var childResult = child.GetResult();

                var chromPeakLevelResults = childResult.ChildResults();

                var filteredChromPeakResults = chromPeakLevelResults.Where(r => r.IsotopicProfileFound).ToList();

                childResult.FavoriteChild = SelectBestChromPeakIqResult(childResult, filteredChromPeakResults);

                GetDataFromFavoriteChild(childResult);
            }

            result.FavoriteChild = SelectBestChargeStateChildResult(result);
            GetDataFromFavoriteChild(result);

            var favResult = result.FavoriteChild;

            getRSquaredVal(result, out var rSquaredVal, out var slope);
            var favChargeState = result.FavoriteChild?.Target.ChargeState ?? 0;
            var favMz = result.FavoriteChild?.Target.MZTheor ?? 0;

            if (!_headerLogged)
            {
                _headerLogged = true;
                IqLogger.LogMessage("\t" + "TargetID" + "\t\t\t" + "M/Z" + "\t" + "Charge" + "\t" + "LCScan" + "\t" + "RSquared" + "\t" + "Slope");
            }

            IqLogger.LogMessage("\t" + result.Target.ID + "\t\t\t" + favMz.ToString("0.000") + "\t" + favChargeState + "\t" + result.LcScanObs + "\t" + rSquaredVal + "\t" + slope);

            //now get the mass spectrum given the info from the favorite child charge state result

            if (favResult != null)
            {
                var scanSet = new ScanSetFactory().CreateScanSet(Run, favResult.LCScanSetSelected.PrimaryScanNumber,
                                                                     WorkflowParameters.NumMSScansToSum);

                var selectedChromPeak = favResult.ChromPeakSelected;
                var sigma = selectedChromPeak.Width / 2.35;
                var chromScanWindowWidth = 4 * sigma;

                //Determines where to start and stop chromatogram correlation
                var startScan = scanSet.PrimaryScanNumber - (int)Math.Round(chromScanWindowWidth / 2, 0);
                var stopScan = scanSet.PrimaryScanNumber + (int)Math.Round(chromScanWindowWidth / 2, 0);

                var massSpectrum = MSGenerator.GenerateMS(Run, scanSet);

                foreach (var iqTarget in children)
                {
                    var childStateIqResult = (O16O18IqResult)iqTarget.GetResult();

                    childStateIqResult.IqResultDetail.MassSpectrum = massSpectrum.TrimData(iqTarget.MZTheor - 3, iqTarget.MZTheor + 8);

                    var msPeakList = _msPeakDetector.FindPeaks(childStateIqResult.IqResultDetail.MassSpectrum.Xvalues,
                                                                      childStateIqResult.IqResultDetail.MassSpectrum.Yvalues);


                    childStateIqResult.CorrelationData = ChromatogramCorrelator.CorrelateData(Run, childStateIqResult, startScan, stopScan);


                    childStateIqResult.CorrelationO16O18SingleLabel = childStateIqResult.GetCorrelationO16O18SingleLabel();
                    childStateIqResult.CorrelationO16O18DoubleLabel = childStateIqResult.GetCorrelationO16O18DoubleLabel();
                    childStateIqResult.CorrelationBetweenSingleAndDoubleLabel = childStateIqResult.GetCorrelationBetweenSingleAndDoubleLabel();

                    childStateIqResult.RatioO16O18DoubleLabel = childStateIqResult.GetRatioO16O18DoubleLabel();
                    childStateIqResult.RatioO16O18SingleLabel = childStateIqResult.GetRatioO16O18SingleLabel();
                    childStateIqResult.RatioSingleToDoubleLabel = childStateIqResult.GetRatioSingleToDoubleLabel();

                    childStateIqResult.ObservedIsotopicProfile = MsFeatureFinder.IterativelyFindMSFeature(
                        childStateIqResult.IqResultDetail.MassSpectrum,
                        iqTarget.TheorIsotopicProfile);


                    if (childStateIqResult.ObservedIsotopicProfile != null)
                    {
                        var observedIsoList = childStateIqResult.ObservedIsotopicProfile.Peaklist.Cast<Peak>().Take(4).ToList();    //first 4 peaks excludes the O18 double label peak (fifth peak)
                        var theorPeakList = iqTarget.TheorIsotopicProfile.Peaklist.Select(p => (Peak)p).Take(4).ToList();
                        childStateIqResult.FitScore = PeakFitter.GetFit(theorPeakList, observedIsoList, 0.05, WorkflowParameters.MSToleranceInPPM);

                        var o18Iso = childStateIqResult.ConvertO16ProfileToO18(iqTarget.TheorIsotopicProfile, 4);
                        theorPeakList = o18Iso.Peaklist.Select(p => (Peak)p).ToList();
                        observedIsoList = childStateIqResult.ObservedIsotopicProfile.Peaklist.Cast<Peak>().Skip(4).ToList();    //skips the first 4 peaks and thus includes the O18 double label isotopic profile
                        childStateIqResult.FitScoreO18Profile = PeakFitter.GetFit(theorPeakList, observedIsoList, 0.05, WorkflowParameters.MSToleranceInPPM);



                        childStateIqResult.InterferenceScore = InterferenceScorer.GetInterferenceScore(childStateIqResult.ObservedIsotopicProfile, msPeakList);
                        childStateIqResult.MZObs = childStateIqResult.ObservedIsotopicProfile.MonoPeakMZ;
                        childStateIqResult.MonoMassObs = childStateIqResult.ObservedIsotopicProfile.MonoIsotopicMass;
                        childStateIqResult.MZObsCalibrated = Run.GetAlignedMZ(childStateIqResult.MZObs);
                        childStateIqResult.MonoMassObsCalibrated = (childStateIqResult.MZObsCalibrated - DeconTools.Backend.Globals.PROTON_MASS) * childStateIqResult.Target.ChargeState;
                        childStateIqResult.ElutionTimeObs = ((ChromPeak)favResult.ChromPeakSelected).NETValue;


                    }
                    else
                    {
                        childStateIqResult.FitScore = -1;
                        childStateIqResult.InterferenceScore = -1;
                    }


                    getRSquaredVal(childStateIqResult, out rSquaredVal, out slope);
                    IqLogger.LogMessage("\t\t\t" + childStateIqResult.Target.ID + "\t" + childStateIqResult.Target.MZTheor.ToString("0.000") + "\t" + childStateIqResult.Target.ChargeState
                        + "\t" + childStateIqResult.LcScanObs + "\t" + childStateIqResult.FitScore.ToString("0.000") + "\t" + rSquaredVal + "\t" + slope);


                    childStateIqResult.LCScanSetSelected = favResult.LCScanSetSelected;
                    childStateIqResult.LcScanObs = favResult.LcScanObs;

                    if (GraphsAreOutputted)
                    {
                        if (_graphGenerator == null) _graphGenerator = new BasicGraphControl();

                        ExportGraphs(childStateIqResult);
                    }




                }
            }
        }




        private void ExportGraphs(IqResult result)
        {
            if (string.IsNullOrEmpty(OutputFolderForGraphs))
            {
                OutputFolderForGraphs = Path.Combine(Run.DataSetPath, "OutputGraphs");
            }

            if (!Directory.Exists(OutputFolderForGraphs)) Directory.CreateDirectory(OutputFolderForGraphs);

            ExportMassSpectrumGraph(result);
            ExportChromGraph(result);
        }

        private void ExportMassSpectrumGraph(IqResult result)
        {
            _graphGenerator.GraphHeight = 600;
            _graphGenerator.GraphWidth = 800;

            _graphGenerator.GenerateGraph(result.IqResultDetail.MassSpectrum.Xvalues, result.IqResultDetail.MassSpectrum.Yvalues);
            var line = _graphGenerator.GraphPane.CurveList[0] as LineItem;
            line.Line.IsVisible = true;
            line.Symbol.Size = 3;
            line.Line.Width = 2;
            line.Symbol.Type = SymbolType.None;
            line.Color = Color.Black;

            _graphGenerator.GraphPane.XAxis.Title.Text = "m/z";
            _graphGenerator.GraphPane.YAxis.Title.Text = "intensity";
            _graphGenerator.GraphPane.XAxis.Scale.MinAuto = true;
            _graphGenerator.GraphPane.YAxis.Scale.MinAuto = false;
            _graphGenerator.GraphPane.YAxis.Scale.MaxAuto = false;

            _graphGenerator.GraphPane.YAxis.Scale.Min = 0;


            _graphGenerator.GraphPane.YAxis.Scale.Max = result.IqResultDetail.MassSpectrum.GetMaxY();
            _graphGenerator.GraphPane.YAxis.Scale.Format = "0";


            _graphGenerator.GraphPane.XAxis.Scale.FontSpec.Size = 12;
            var outputGraphFilename = Path.Combine(OutputFolderForGraphs,
                                                      result.Target.ID + "_" +
                                                      result.Target.ChargeState + "_" +
                                                      result.Target.MZTheor.ToString("0.000") + "_MS.png");


            var graphInfoText = "ID= " + result.Target.ID + "; z= " + result.Target.ChargeState + "; m/z= " +
                                   result.Target.MZTheor.ToString("0.000") + "; ScanLC= " + result.LcScanObs;

            _graphGenerator.AddAnnotationRelativeAxis(graphInfoText, 0.3, 0.02);

            _graphGenerator.AddAnnotationAbsoluteXRelativeY("*", result.Target.MZTheor, 0.03);
            _graphGenerator.SaveGraph(outputGraphFilename);
        }

        private void ExportChromGraph(IqResult result)
        {
            if (result.IqResultDetail.Chromatogram == null)
            {
                return;
            }


            var minScan = result.LcScanObs - 1000;
            var maxScan = result.LcScanObs + 1000;


            _graphGenerator.GraphHeight = 600;
            _graphGenerator.GraphWidth = 800;

            result.IqResultDetail.Chromatogram = result.IqResultDetail.Chromatogram.TrimData(minScan, maxScan);


            _graphGenerator.GenerateGraph(result.IqResultDetail.Chromatogram.Xvalues, result.IqResultDetail.Chromatogram.Yvalues);
            var line = _graphGenerator.GraphPane.CurveList[0] as LineItem;
            line.Line.IsVisible = true;
            line.Symbol.Size = 3;
            line.Line.Width = 2;
            line.Symbol.Type = SymbolType.None;
            line.Color = Color.Black;

            _graphGenerator.GraphPane.XAxis.Title.Text = "scan";
            _graphGenerator.GraphPane.YAxis.Title.Text = "intensity";
            _graphGenerator.GraphPane.XAxis.Scale.MinAuto = false;
            _graphGenerator.GraphPane.XAxis.Scale.MaxAuto = false;
            _graphGenerator.GraphPane.YAxis.Scale.MinAuto = false;
            _graphGenerator.GraphPane.YAxis.Scale.MaxAuto = false;
            _graphGenerator.GraphPane.YAxis.Scale.Min = 0;
            _graphGenerator.GraphPane.YAxis.Scale.Max = result.IqResultDetail.Chromatogram.GetMaxY();
            _graphGenerator.GraphPane.YAxis.Scale.Format = "0";

            _graphGenerator.GraphPane.XAxis.Scale.Min = minScan;
            _graphGenerator.GraphPane.XAxis.Scale.Max = maxScan;

            _graphGenerator.GraphPane.XAxis.Scale.FontSpec.Size = 12;
            var outputGraphFilename = Path.Combine(OutputFolderForGraphs,
                                                      result.Target.ID + "_" +
                                                      result.Target.ChargeState + "_" +
                                                      result.Target.MZTheor.ToString("0.000") + "_chrom.png");

            var graphInfoText = "ID= " + result.Target.ID + "; z= " + result.Target.ChargeState + "; m/z= " +
                                   result.Target.MZTheor.ToString("0.000") + "; ScanLC= " + result.LcScanObs;

            _graphGenerator.AddAnnotationRelativeAxis(graphInfoText, 0.3, 0.02);
            _graphGenerator.AddVerticalLineToGraph(result.LcScanObs, 1);

            _graphGenerator.AddAnnotationAbsoluteXRelativeY("*", result.Target.MZTheor, 0.03);
            _graphGenerator.SaveGraph(outputGraphFilename);
        }



        private void getRSquaredVal(IqResult result, out double? rSquaredVal, out double? slope)
        {
            if (result.CorrelationData != null && result.CorrelationData.CorrelationDataItems.Count > 0)
            {
                rSquaredVal = result.CorrelationData.RSquaredValsMedian;

                slope = result.CorrelationData.CorrelationDataItems.First().CorrelationSlope;

            }
            else
            {
                rSquaredVal = -1;
                slope = -1;
            }



        }

        private void GetDataFromFavoriteChild(IqResult result)
        {
            var fav = result.FavoriteChild;

            if (fav == null)
            {
                result.LCScanSetSelected = null;
                result.LcScanObs = -1;
                result.MZObs = 0;
                result.MZObsCalibrated = 0;
                result.Abundance = 0;
                result.ElutionTimeObs = -1;
                result.IsotopicProfileFound = false;
                result.InterferenceScore = 1;
                result.FitScore = 1;
                result.MassErrorBefore = 0;
                result.MassErrorAfter = 0;
                result.MonoMassObs = 0;
                result.MonoMassObsCalibrated = 0;
            }
            else
            {
                result.LCScanSetSelected = fav.LCScanSetSelected;
                result.LcScanObs = fav.LcScanObs;
                result.CorrelationData = fav.CorrelationData;
                result.FitScore = fav.FitScore;
                result.ChromPeakSelected = fav.ChromPeakSelected;
                result.MZObs = fav.MZObs;
                result.MZObsCalibrated = fav.MZObsCalibrated;
                result.MonoMassObs = fav.MonoMassObs;
                result.MonoMassObsCalibrated = fav.MonoMassObsCalibrated;
                result.ElutionTimeObs = fav.ElutionTimeObs;
                result.Abundance = fav.ChromPeakSelected?.Height ?? 0f;


            }
        }

        private IqResult SelectBestChargeStateChildResult(IqResult result)
        {
            var filteredChargeStateResults = result.ChildResults().Where(p => p.FavoriteChild != null).ToList();

            if (filteredChargeStateResults.Count == 0) return null;

            if (filteredChargeStateResults.Count == 1) return filteredChargeStateResults.First();

            //now to deal with the tough issue of multiple charge states having a possible results.

            var filter2 = filteredChargeStateResults.Where(p => p.CorrelationData.CorrelationDataItems.First().CorrelationRSquaredVal > 0.7).ToList();

            if (filter2.Count == 0)
            {
                filter2 = filteredChargeStateResults.Where(p => p.CorrelationData.CorrelationDataItems.First().CorrelationRSquaredVal > 0.5).ToList();
            }

            if (filter2.Count == 0)
            {
                filter2 = filteredChargeStateResults.Where(p => p.CorrelationData.CorrelationDataItems.First().CorrelationRSquaredVal > 0.3).ToList();
            }

            if (filter2.Count == 0)
            {
                //correlation values are no good. Now will sort by fit score.
                filter2 = filteredChargeStateResults.OrderBy(p => p.FitScore).Take(1).ToList();   //sort by fit score let the first one be selected
            }

            if (filter2.Count == 1)
            {
                return filter2.First();
            }

            if (filter2.Count > 1)
            {
                //if we reached here, there are multiple charge state results with good correlation scores.  Take the one of highest intensity.
                return filter2.OrderByDescending(p => p.Abundance).First();


            }

            return null;


        }

        private IqResult SelectBestChromPeakIqResult(IqResult childResult, IReadOnlyCollection<IqResult> filteredChromPeakResults)
        {
            var numCandidateResults = filteredChromPeakResults.Count;
            IqResult bestChromPeakResult;

            if (numCandidateResults == 0)
            {
                bestChromPeakResult = null;
            }
            else if (numCandidateResults == 1)
            {
                bestChromPeakResult = filteredChromPeakResults.First();
            }
            else
            {

                var furtherFilteredResults = new List<IqResult>();
                double lowestFit = 1;
                foreach (var iqResult in filteredChromPeakResults)
                {
                    var r = (O16O18IqResult)iqResult;
                    var currentLowFitVal = Math.Min(r.FitScore, r.FitScoreO18Profile);
                    if (currentLowFitVal < lowestFit)
                    {
                        lowestFit = currentLowFitVal;

                        if (lowestFit < 0.2)
                        {
                            furtherFilteredResults.Add(r);
                        }


                    }
                }

                if (furtherFilteredResults.Count == 0)    // all potential candidates were filtered out above. So need a plan B.
                {
                    bestChromPeakResult = filteredChromPeakResults.OrderBy(p => p.FitScore).Take(3).OrderByDescending(p => p.Abundance).FirstOrDefault();
                }
                else if (furtherFilteredResults.Count == 1)
                {
                    bestChromPeakResult = furtherFilteredResults.First();
                }
                else   //still more than one candidate.  Will try to use another filter
                {
                    var currentBestChromPeakResult = furtherFilteredResults.OrderByDescending(p => p.Abundance).FirstOrDefault();
                    var level3FilteredResults = new List<IqResult>();

                    foreach (var iqResult in furtherFilteredResults)
                    {
                        var furtherFilteredResult = (O16O18IqResult)iqResult;

                        var currentCorrVal = Math.Max(furtherFilteredResult.GetCorrelationO16O18DoubleLabel(), furtherFilteredResult.GetCorrelationBetweenSingleAndDoubleLabel());

                        if (currentCorrVal > 0.7)
                        {
                            level3FilteredResults.Add(furtherFilteredResult);
                        }

                    }


                    if (level3FilteredResults.Count == 0)
                    {
                        bestChromPeakResult = currentBestChromPeakResult;
                    }
                    else if (level3FilteredResults.Count == 1)
                    {
                        bestChromPeakResult = level3FilteredResults.First();
                    }
                    else
                    {
                        //fit score is good. Correlation good. But still have multiple possibilities
                        bestChromPeakResult = level3FilteredResults.OrderByDescending(p => p.Abundance).First();
                    }


                }
            }


            if (bestChromPeakResult != null)
            {
                childResult.CorrelationData = bestChromPeakResult.CorrelationData;
                childResult.LcScanObs = bestChromPeakResult.LcScanObs;
                childResult.ChromPeakSelected = bestChromPeakResult.ChromPeakSelected; //check this
                childResult.LCScanSetSelected = bestChromPeakResult.LCScanSetSelected;


                var elutionTime = childResult.ChromPeakSelected == null
                                      ? 0d
                                      : ((ChromPeak)bestChromPeakResult.ChromPeakSelected).NETValue;
                childResult.ElutionTimeObs = elutionTime;

                childResult.Abundance = (float)(childResult.ChromPeakSelected == null
                                                     ? 0d
                                                     : ((ChromPeak)bestChromPeakResult.ChromPeakSelected).Height);
            }

            return bestChromPeakResult;
        }

        #endregion

        #region Private Methods

        #endregion


        protected internal override IqResult CreateIQResult(IqTarget target)
        {
            return new O16O18IqResult(target);
        }

        public override TargetedWorkflowParameters WorkflowParameters { get; set; }

        public override IqResultExporter CreateExporter()
        {
            return new O16O18IqResultExporter();
        }
    }
}
