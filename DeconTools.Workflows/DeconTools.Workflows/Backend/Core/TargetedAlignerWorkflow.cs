using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using DeconTools.Workflows.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    /// <summary>
    /// This is a controller class that handles execution of targeted alignment.
    ///
    /// </summary>
    public class TargetedAlignerWorkflow : TargetedWorkflow
    {
        private TargetedAlignerWorkflowParameters AlignerParameters => WorkflowParameters as TargetedAlignerWorkflowParameters;

        private readonly List<NETGrouping> _netGroupings;
        private BasicTargetedWorkflow _workflow;
        private TargetedResultRepository _targetedResultRepository;

        private readonly BackgroundWorker _backgroundWorker;

        #region Constructors

        public TargetedAlignerWorkflow(TargetedWorkflowParameters workflowParameters)
            : this(null, workflowParameters)
        {

        }

        public TargetedAlignerWorkflow(Run run, TargetedWorkflowParameters workflowParameters)
            : base(run, workflowParameters)
        {

            Check.Require(workflowParameters is TargetedAlignerWorkflowParameters, "Parameter object is of the wrong type.");

            _netGroupings = createNETGroupings();
            NumSuccessesPerNETGrouping = new List<int>();
            NumFailuresPerNETGrouping = new List<int>();

            outputToConsole = true;


        }

        public TargetedAlignerWorkflow(Run run, TargetedWorkflowParameters workflowParameters, BackgroundWorker bw)
            : this(run, workflowParameters)
        {
            _backgroundWorker = bw;
        }


        #endregion

        protected override DeconTools.Backend.Globals.ResultType GetResultType()
        {
            return DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
        }

        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();
            _workflow = new BasicTargetedWorkflow(Run, WorkflowParameters as TargetedWorkflowParameters);
        }

        #region Properties

        public NETAndMassAligner Aligner { get; set; }


        public List<TargetBase> MassTagList { get; set; }

        public bool outputToConsole { get; set; }

        #endregion

        #region Public Methods

        public override void Execute()
        {
            List<TargetedResultBase> resultsPassingCriteria;
            _targetedResultRepository = new TargetedResultRepository();


            var featuresAreImportedFromFile = !string.IsNullOrEmpty(AlignerParameters.ImportedFeaturesFilename);
            if (featuresAreImportedFromFile)
            {
                //load them from the Features file
                var importer = new UnlabeledTargetedResultFromTextImporter(AlignerParameters.ImportedFeaturesFilename);
                var repo = importer.Import();
                _targetedResultRepository.Results = repo.Results;
            }
            else
            {
                Check.Require(Run.ResultCollection.MSPeakResultList != null && Run.ResultCollection.MSPeakResultList.Count > 0, "Dataset's Peak-level data is empty. This is needed for chromatogram generation.");

                //execute targeted feature finding to find the massTags in the raw data

                _workflow = new BasicTargetedWorkflow(Run, AlignerParameters);

                var netGrouping = 0.2;
                double chromTolerance = 5;  //in ppm

                var progressString = "First trying to find alignment targets using narrow mass tolerances.... ";
                ReportProgress(0, progressString);
                var firstPassResults = FindTargetsThatPassSpecifiedMassTolerance(netGrouping,chromTolerance);

                if (firstPassResults.Count<10)
                {
                    //try another netGrouping
                    netGrouping = 0.3;

                    chromTolerance = 20;
                    progressString = "Couldn't find enough. Now trying wider mass tolerance = "+ chromTolerance;
                    ReportProgress(0, progressString);
                    var secondPassResults = FindTargetsThatPassSpecifiedMassTolerance(netGrouping, chromTolerance);
                    firstPassResults.AddRange(secondPassResults);

                }

                if (firstPassResults.Count < 10)
                {
                    netGrouping = 0.4;
                    chromTolerance = 50;

                    progressString = "Ok this is a tough one. Now going even wider. Mass tolerance = " + chromTolerance;
                    ReportProgress(0, progressString);
                    var thirdPassResults = FindTargetsThatPassSpecifiedMassTolerance(netGrouping, chromTolerance);
                    firstPassResults.AddRange(thirdPassResults);

                }

                var ppmErrors = getMassErrors(firstPassResults);
                var filteredUsingGrubbsPPMErrors = MathUtilities.filterWithGrubbsApplied(ppmErrors);


                var canUseNarrowTolerances = executeDecisionOnUsingTightTolerances(filteredUsingGrubbsPPMErrors);





                if (canUseNarrowTolerances)
                {
                    var avgPPMError = filteredUsingGrubbsPPMErrors.Average();
                    var stdev = MathUtilities.GetStDev(filteredUsingGrubbsPPMErrors);

                    var tolerance = Math.Abs(avgPPMError) + 2 * stdev;
                    AlignerParameters.ChromGenTolerance = (int)Math.Ceiling(tolerance);
                    AlignerParameters.MSToleranceInPPM = (int)Math.Ceiling(tolerance);

                    progressString = "STRICT_Matches_AveragePPMError = \t" + avgPPMError.ToString("0.00") + "; Stdev = \t" + stdev.ToString("0.00000");
                    ReportProgress(0, progressString);

                    progressString = "NOTE: using the new PPMTolerance=  " + AlignerParameters.ChromGenTolerance;
                    ReportProgress(0, progressString);

                    _workflow = new BasicTargetedWorkflow(Run, AlignerParameters);

                }
                else
                {

                    double avgPPMError = 0;
                    double stdev = 0;
                    if (filteredUsingGrubbsPPMErrors.Count != 0)
                    {
                        avgPPMError = filteredUsingGrubbsPPMErrors.Average();
                        stdev = MathUtilities.GetStDev(filteredUsingGrubbsPPMErrors);
                    }


                    progressString = "STRICT_Matches_AveragePPMError = \t" + avgPPMError.ToString("0.00") + "; Stdev = \t" + stdev.ToString("0.00000");
                    ReportProgress(0, progressString);

                    progressString = "Cannot use narrow ppm tolerances during NET/Mass alignment. Either the massError was too high or couldn't find enough strict matches.";
                    ReportProgress(0, progressString);

                    // find a way to work with datasets with masses way off but low stdev
                }


                resultsPassingCriteria = FindTargetsThatPassCriteria();

                _targetedResultRepository.AddResults(resultsPassingCriteria);



            }

            var canDoAlignment = _targetedResultRepository.Results.Count > 0;

            if (canDoAlignment)
            {
                doAlignment();
            }
        }

        private bool executeDecisionOnUsingTightTolerances(List<double> ppmErrors)
        {

            if (ppmErrors.Count < 5) return false;

            var avgPPMError = ppmErrors.Average();

            if (avgPPMError > 10) return false;

            return true;

        }

        private List<double> getMassErrors(List<TargetedResultBase> firstPassResults)
        {
            var ppmErrors = new List<double>();


            foreach (var result in firstPassResults)
            {


                var theorMZ = result.GetMZOfMostIntenseTheorIsotopicPeak();
                var observedMZ = result.GetMZOfObservedPeakClosestToTargetVal(theorMZ);


                var ppmError = (theorMZ - observedMZ) / theorMZ * 1e6;

                ppmErrors.Add(ppmError);


            }

            return ppmErrors;
        }

        private List<TargetedResultBase> FindTargetsThatPassSpecifiedMassTolerance(double netGrouping, double chromTolerance)
        {
            Check.Require(MassTagList != null && MassTagList.Count > 0, "MassTags have not been defined.");
            Check.Require(Run != null, "Run is null");

            var workflowParameters = _workflow.WorkflowParameters as TargetedWorkflowParameters;
            workflowParameters.ChromGenTolerance = chromTolerance;



            var resultsPassingCriteria = new List<TargetedResultBase>();

            var netgrouping1 = (from n in _netGroupings where n.Lower >= netGrouping select n).First();

            var filteredMasstags = (from n in MassTagList
                                    where n.NormalizedElutionTime >= netgrouping1.Lower && n.NormalizedElutionTime < netgrouping1.Upper
                                    orderby n.ObsCount descending
                                    select n);

            var numPassingMassTagsInGrouping = 0;
            var numFailingMassTagsInGrouping = 0;


            foreach (var massTag in filteredMasstags)
            {
                Run.CurrentMassTag = massTag;
                _workflow.Execute();

                var result = Run.ResultCollection.GetTargetedResult(massTag);

                if (resultPassesStrictCriteria(result))
                {

                    var theorMZ = result.GetMZOfMostIntenseTheorIsotopicPeak();
                    var obsMZ = result.GetMZOfObservedPeakClosestToTargetVal(theorMZ);
                    var ppmError = (theorMZ - obsMZ) / theorMZ * 1e6;

                    var progressInfo = "STRICT MATCH: " + massTag.ID + "; m/z= " + massTag.MZ.ToString("0.0000") + "; NET= " + massTag.NormalizedElutionTime.ToString("0.000") + "; found in scan: " + result.GetScanNum() + "; PPMError= " + ppmError.ToString("0.00");
                    ReportProgress(0, progressInfo);

                    //ReportProgress(progressPercentage, progressInfo);
                    resultsPassingCriteria.Add(result);   //where passing results are added
                    numPassingMassTagsInGrouping++;
                }
                else
                {
                    numFailingMassTagsInGrouping++;
                }

                if (numPassingMassTagsInGrouping >= 10)
                {
                    break;   //found enough massTags in this grouping
                }

                if (numFailingMassTagsInGrouping > AlignerParameters.NumMaxAttemptsDuringFirstPassMassAnalysis)
                {
                    break;  // too many failed massTags in this grouping. Will move on to next grouping
                }

            }

            return resultsPassingCriteria;




        }






        public List<TargetedResultBase> FindTargetsThatPassCriteria()
        {
            Check.Require(MassTagList != null && MassTagList.Count > 0, "MassTags have not been defined.");
            Check.Require(Run != null, "Run is null");

            var resultsPassingCriteria = new List<TargetedResultBase>();

            var netGroupingCounter = 0;
            foreach (var netGrouping in _netGroupings)
            {
                netGroupingCounter++;

                var progressPercentage = netGroupingCounter * 100 / _netGroupings.Count;

                var progressString = "NET grouping " + netGrouping.Lower + "-" + netGrouping.Upper;
                ReportProgress(progressPercentage, progressString);



                var filteredMasstags = (from n in MassTagList
                                        where n.NormalizedElutionTime >= netGrouping.Lower && n.NormalizedElutionTime < netGrouping.Upper
                                        orderby n.ObsCount descending
                                        select n);

                var numPassingMassTagsInGrouping = 0;
                var numFailingMassTagsInGrouping = 0;


                foreach (var massTag in filteredMasstags)
                {
                    Run.CurrentMassTag = massTag;
                    _workflow.Execute();

                    var result = Run.ResultCollection.GetTargetedResult(massTag);

                    if (resultPassesCriteria(result))
                    {

                        var progressInfo = massTag.ID + "; m/z= " + massTag.MZ.ToString("0.0000") + "; NET= " + massTag.NormalizedElutionTime.ToString("0.000") + "; found in scan: " + result.GetScanNum();

                        ReportProgress(progressPercentage, progressInfo);
                        resultsPassingCriteria.Add(result);   //where passing results are added
                        numPassingMassTagsInGrouping++;
                    }
                    else
                    {
                        numFailingMassTagsInGrouping++;
                    }

                    if (numPassingMassTagsInGrouping >= AlignerParameters.NumDesiredMassTagsPerNETGrouping)
                    {
                        break;   //found enough massTags in this grouping
                    }

                    if (numFailingMassTagsInGrouping > AlignerParameters.NumMaxAttemptsPerNETGrouping)
                    {
                        break;  // too many failed massTags in this grouping. Will move on to next grouping
                    }

                }

                NumFailuresPerNETGrouping.Add(numFailingMassTagsInGrouping);
                NumSuccessesPerNETGrouping.Add(numPassingMassTagsInGrouping);

                var progressInfo2 = "NET grouping " + netGrouping.Lower + "-" + netGrouping.Upper + " COMPLETE. Found massTags= " + numPassingMassTagsInGrouping + "; Missing massTags = " + numFailingMassTagsInGrouping;
                ReportProgress(progressPercentage, progressInfo2);

                if (_backgroundWorker != null && _backgroundWorker.CancellationPending)
                {

                    resultsPassingCriteria.Clear();
                    return resultsPassingCriteria;
                }

            }

            return resultsPassingCriteria;




        }



        public void SetMassTags(List<TargetBase> massTagList)
        {
            MassTagList = massTagList;
        }

        public void SetMassTags(string massTagFilename)
        {
            var importer = new MassTagFromTextFileImporter(massTagFilename);
            var mtc = importer.Import();
            MassTagList = mtc.TargetList;
        }

        public List<int> NumSuccessesPerNETGrouping { get; set; }
        public List<int> NumFailuresPerNETGrouping { get; set; }


        #endregion

        #region Private Methods

        public void SaveFeaturesToTextFile(string outputDirectory)
        {
            var exportTargetedFeaturesFile = Path.Combine(outputDirectory, Run.DatasetName + "_alignedFeatures.txt");

            var exporter = new UnlabeledTargetedResultToTextExporter(exportTargetedFeaturesFile);
            exporter.ExportResults(_targetedResultRepository.Results);
        }

        public void SaveAlignmentData(string outputDirectory)
        {

            var exportNETAlignmentFilename = Path.Combine(outputDirectory, Run.DatasetName + "_NETAlignment.txt");
            var exportMZAlignmentFilename = Path.Combine(outputDirectory, Run.DatasetName + "_MZAlignment.txt");

            var mzAlignmentExporter = new MassAlignmentInfoToTextExporter(exportMZAlignmentFilename);
            mzAlignmentExporter.ExportAlignmentInfo(Run.AlignmentInfo);

            var netAlignmentExporter = new NETAlignmentInfoToTextExporter(exportNETAlignmentFilename);
            netAlignmentExporter.ExportAlignmentInfo(Run.AlignmentInfo);
        }

        private void doAlignment()
        {
            Aligner = new NETAndMassAligner();
            Aligner.SetFeaturesToBeAligned(_targetedResultRepository.Results);
            Aligner.SetReferenceMassTags(MassTagList);
            Aligner.Execute(Run);
        }

        private List<NETGrouping> createNETGroupings()
        {
            var netDivisions = new double[] { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
            var netGroupings = new List<NETGrouping>();

            for (var i = 0; i < netDivisions.Length - 1; i++)
            {
                var grouping = new NETGrouping {
                    Lower = netDivisions[i],
                    Upper = netDivisions[i + 1]
                };

                netGroupings.Add(grouping);
            }

            return netGroupings;
        }

        private void ReportProgress(int progressPercentage, string progressString)
        {
            if (_backgroundWorker == null)
            {
                if (outputToConsole)
                {
                    Console.WriteLine(DateTime.Now + "\t" + progressString);

                }
            }
            else
            {
                _backgroundWorker.ReportProgress(progressPercentage, progressString);
            }
        }

        private bool resultPassesStrictCriteria(TargetedResultBase result)
        {
            var passesCriteria = true;

            if (result.FailedResult) return false;

            if (result.ChromPeakSelected == null) return false;

            if (result.IsotopicProfile == null) return false;

            if (result.NumQualityChromPeaks > 1) return false;

            if (result.Flags.Count > 0) return false;

            if (result.ChromPeakSelected.Height < AlignerParameters.MinimumChromPeakIntensityCriteria) return false;

            if (result.Score > AlignerParameters.UpperFitScoreAllowedCriteria) return false;

            if (result.InterferenceScore > AlignerParameters.IScoreAllowedCriteria) return false;

            return passesCriteria;
        }


        private bool resultPassesCriteria(TargetedResultBase result)
        {
            var passesCriteria = true;

            if (result.FailedResult) return false;

            if (result.ChromPeakSelected == null) return false;

            if (result.IsotopicProfile == null) return false;

            if (result.NumQualityChromPeaks > AlignerParameters.NumChromPeaksAllowedDuringSelection) return false;

            if (result.Flags.Count > 0) return false;

            if (result.ChromPeakSelected.Height < AlignerParameters.MinimumChromPeakIntensityCriteria) return false;

            if (result.Score > AlignerParameters.UpperFitScoreAllowedCriteria) return false;

            if (result.InterferenceScore > AlignerParameters.IScoreAllowedCriteria) return false;

            return passesCriteria;
        }

        #endregion


        public string GetAlignmentReport1()
        {
            if (_netGroupings == null || _netGroupings.Count == 0) return string.Empty;

            if (NumFailuresPerNETGrouping.Count != _netGroupings.Count) return string.Empty;
            if (NumSuccessesPerNETGrouping.Count != _netGroupings.Count) return string.Empty;


            var sb = new StringBuilder();
            sb.Append("NETGrouping\tSuccesses\tFailures\n");

            for (var i = 0; i < _netGroupings.Count; i++)
            {
                sb.Append(_netGroupings[i].Lower + "-" + _netGroupings[i].Upper);
                sb.Append("\t");
                sb.Append(NumSuccessesPerNETGrouping[i]);
                sb.Append("\t");
                sb.Append(NumFailuresPerNETGrouping[i]);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }


    }
}
