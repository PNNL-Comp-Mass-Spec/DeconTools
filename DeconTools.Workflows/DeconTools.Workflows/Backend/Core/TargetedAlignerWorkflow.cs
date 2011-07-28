﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.Core
{
    /// <summary>
    /// This is a controller class that handles execution of targeted alignment. 
    /// 
    /// </summary>
    public class TargetedAlignerWorkflow : TargetedWorkflow
    {
        TargetedAlignerWorkflowParameters _parameters;

        private List<NETGrouping> _netGroupings;
        private BasicTargetedWorkflow _workflow;
        private TargetedResultRepository _targetedResultRepository;


        private BackgroundWorker _backgroundWorker;

        #region Constructors

        public TargetedAlignerWorkflow(WorkflowParameters workflowParameters)
            : this(null, workflowParameters)
        {

        }

        public TargetedAlignerWorkflow(Run run, WorkflowParameters workflowParameters)
        {
            Run = run;

            Check.Require(workflowParameters is TargetedAlignerWorkflowParameters, "TargetedAlignerWorkflow could not be instantiated. Parameters are not of the correct type.");
            _parameters = (TargetedAlignerWorkflowParameters)workflowParameters;
            _netGroupings = createNETGroupings();
            NumSuccessesPerNETGrouping = new List<int>();
            NumFailuresPerNETGrouping = new List<int>();


        }

        public TargetedAlignerWorkflow(Run run, WorkflowParameters workflowParameters, BackgroundWorker bw)
            : this(run, workflowParameters)
        {
            _backgroundWorker = bw;
        }


        #endregion

        public override void InitializeWorkflow()
        {
            _workflow = new BasicTargetedWorkflow(Run, _parameters);
        }

        #region Properties




        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = value as TargetedAlignerWorkflowParameters;
            }
        }



        public List<MassTag> MassTagList { get; set; }

        #endregion

        #region Public Methods

        public override void Execute()
        {
            Check.Require(Run != null, "Run has not been defined.");

            _workflow = new BasicTargetedWorkflow(Run, _parameters);


            List<MassTagResultBase> resultsPassingCriteria;
            _targetedResultRepository = new TargetedResultRepository();


            bool featuresAreImportedFromFile = (_parameters.ImportedFeaturesFilename != null && _parameters.ImportedFeaturesFilename.Length > 0);
            if (featuresAreImportedFromFile)
            {
                //load them from the Features file
                UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(_parameters.ImportedFeaturesFilename);
                TargetedResultRepository repo = importer.Import();
                _targetedResultRepository.Results = repo.Results;
            }
            else
            {
                Check.Require(Run.ResultCollection.MSPeakResultList != null && Run.ResultCollection.MSPeakResultList.Count > 0, "Dataset's Peak-level data is empty. This is needed for chromatogram generation.");

                //execute targeted feature finding to find the massTags in the raw data
                resultsPassingCriteria = FindTargetsThatPassCriteria();

                _targetedResultRepository.AddResults(resultsPassingCriteria);

                if (_parameters.FeaturesAreSavedToTextFile)
                {
                    saveFeaturesToTextfile();
                }

            }

            bool canDoAlignment = _targetedResultRepository.Results.Count > 0;

            if (canDoAlignment)
            {
                doAlignment();

                if (Run.AlignmentInfo != null)
                {
                    saveAlignmentData();
                }

            }

        }




        public List<MassTagResultBase> FindTargetsThatPassCriteria()
        {
            Check.Require(this.MassTagList != null && this.MassTagList.Count > 0, "MassTags have not been defined.");
            Check.Require(Run != null, "Run is null");

            List<MassTagResultBase> resultsPassingCriteria = new List<MassTagResultBase>();

            int netGroupingCounter = 0;
            foreach (var netGrouping in _netGroupings)
            {
                netGroupingCounter++;

                int progressPercentage = netGroupingCounter * 100 / _netGroupings.Count;

                string progressString = "NET grouping " + netGrouping.Lower + "-" + netGrouping.Upper;
                reportProgess(progressPercentage, progressString);



                var filteredMasstags = (from n in this.MassTagList
                                        where n.NETVal >= netGrouping.Lower && n.NETVal < netGrouping.Upper
                                        select n);

                int numPassingMassTagsInGrouping = 0;
                int numFailingMassTagsInGrouping = 0;


                foreach (var massTag in filteredMasstags)
                {
                    Run.CurrentMassTag = massTag;
                    _workflow.Execute();

                    var result = Run.ResultCollection.GetMassTagResult(massTag);

                    if (resultPassesCriteria(result))
                    {

                        string progressInfo = massTag.ID + "; m/z= " + massTag.MZ.ToString("0.0000") + "; NET= " + massTag.NETVal.ToString("0.000") + "; found in scan: " + result.GetScanNum();

                        reportProgess(progressPercentage, progressInfo);
                        resultsPassingCriteria.Add(result);   //where passing results are added
                        numPassingMassTagsInGrouping++;
                    }
                    else
                    {
                        numFailingMassTagsInGrouping++;
                    }

                    if (numPassingMassTagsInGrouping >= _parameters.NumDesiredMassTagsPerNETGrouping)
                    {
                        break;   //found enough massTags in this grouping
                    }

                    if (numFailingMassTagsInGrouping > _parameters.NumMaxAttemptsPerNETGrouping)
                    {
                        break;  // too many failed massTags in this grouping. Will move on to next grouping
                    }

                }

                NumFailuresPerNETGrouping.Add(numFailingMassTagsInGrouping);
                NumSuccessesPerNETGrouping.Add(numPassingMassTagsInGrouping);

                string progressInfo2 = "NET grouping " + netGrouping.Lower + "-" + netGrouping.Upper + " COMPLETE. Found massTags= " + numPassingMassTagsInGrouping + "; Missing massTags = " + numFailingMassTagsInGrouping;
                reportProgess(progressPercentage, progressInfo2);

                if (_backgroundWorker != null && _backgroundWorker.CancellationPending)
                {

                    resultsPassingCriteria.Clear();
                    return resultsPassingCriteria;
                }

            }

            return resultsPassingCriteria;




        }



        public void SetMassTags(List<MassTag> massTagList)
        {
            MassTagList = massTagList;
        }

        public void SetMassTags(string massTagFilename)
        {
            MassTagFromTextFileImporter importer = new MassTagFromTextFileImporter(massTagFilename);
            MassTagCollection mtc = importer.Import();
            MassTagList = mtc.MassTagList;
        }

        public List<int> NumSuccessesPerNETGrouping { get; set; }
        public List<int> NumFailuresPerNETGrouping { get; set; }


        #endregion

        #region Private Methods

        private void saveFeaturesToTextfile()
        {
            string outputfolder;

            if (_parameters.ExportAlignmentFolder == null || _parameters.ExportAlignmentFolder.Length == 0)
            {
                outputfolder = Run.DataSetPath;
            }
            else
            {
                outputfolder = _parameters.ExportAlignmentFolder;
            }


            string exportTargetedFeaturesFile = outputfolder + "\\" + Run.DatasetName + "_alignedFeatures.txt";

            UnlabelledTargetedResultToTextExporter exporter = new UnlabelledTargetedResultToTextExporter(exportTargetedFeaturesFile);
            exporter.ExportResults(_targetedResultRepository.Results);
        }

        private void saveAlignmentData()
        {
            if (_parameters.AlignmentInfoIsExported)
            {
                string outputfolder;

                if (_parameters.ExportAlignmentFolder == null || _parameters.ExportAlignmentFolder.Length == 0)
                {
                    outputfolder = Run.DataSetPath;
                }
                else
                {
                    outputfolder = _parameters.ExportAlignmentFolder;
                }

                string exportNETAlignmentFilename = outputfolder + "\\" + Run.DatasetName + "_NETAlignment.txt";
                string exportMZAlignmentFilename = outputfolder + "\\" + Run.DatasetName + "_MZAlignment.txt";

                MassAlignmentInfoToTextExporter mzAlignmentExporter = new MassAlignmentInfoToTextExporter(exportMZAlignmentFilename);
                mzAlignmentExporter.ExportAlignmentInfo(Run.AlignmentInfo);

                NETAlignmentInfoToTextExporter netAlignmentExporter = new NETAlignmentInfoToTextExporter(exportNETAlignmentFilename);
                netAlignmentExporter.ExportAlignmentInfo(Run.AlignmentInfo);

            }
        }

        private void doAlignment()
        {
            NETAndMassAligner aligner = new NETAndMassAligner();
            aligner.SetFeaturesToBeAligned(_targetedResultRepository.Results);
            aligner.SetReferenceMassTags(this.MassTagList);
            aligner.Execute(this.Run);
        }

        private List<NETGrouping> createNETGroupings()
        {
            double[] netDivisions = new double[] { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
            List<NETGrouping> netgroupings = new List<NETGrouping>();

            for (int i = 0; i < netDivisions.Length - 1; i++)
            {
                NETGrouping grouping = new NETGrouping();
                grouping.Lower = netDivisions[i];
                grouping.Upper = netDivisions[i + 1];

                netgroupings.Add(grouping);


            }

            return netgroupings;

        }

        private void reportProgess(int progressPercentage, string progressString)
        {
            if (_backgroundWorker == null)
            {
                //Console.WriteLine(DateTime.Now + "\t" + progressString);
            }
            else
            {


                _backgroundWorker.ReportProgress(progressPercentage, progressString);
            }
        }

        private bool resultPassesCriteria(MassTagResultBase result)
        {
            bool passesCriteria = true;

            if (result.FailedResult) return false;

            if (result.ChromPeakSelected == null) return false;

            if (result.IsotopicProfile == null) return false;

            if (result.NumQualityChromPeaks > _parameters.NumChromPeaksAllowedDuringSelection) return false;

            if (result.Flags.Count > 0) return false;

            if (result.ChromPeakSelected.Height < _parameters.MinimumChromPeakIntensityCriteria) return false;

            if (result.Score > _parameters.UpperFitScoreAllowedCriteria) return false;

            if (result.InterferenceScore > _parameters.IScoreAllowedCriteria) return false;

            return passesCriteria;
        }

        #endregion


        public string GetAlignmentReport1()
        {
            if (_netGroupings == null || _netGroupings.Count == 0) return String.Empty;

            if (this.NumFailuresPerNETGrouping.Count != _netGroupings.Count) return String.Empty;
            if (this.NumSuccessesPerNETGrouping.Count != _netGroupings.Count) return String.Empty;


            StringBuilder sb = new StringBuilder();
            sb.Append("NETGrouping\tSuccesses\tFailures\n");

            for (int i = 0; i < this._netGroupings.Count; i++)
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
