using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.FileIO.TargetedResultFileIO;
using DeconTools.Backend.Workflows;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.NETAlignment
{
    /// <summary>
    /// This is a controller class that handles execution of targeted alignment. 
    /// 
    /// </summary>
    public class TargetedAligner
    {
        WorkflowParameters _parameters;
        Run _run;
       
        private List<NETGrouping> _netGroupings;
        private BasicTargetedWorkflow _workflow;
        private TargetedResultRepository _targetedResultRepository;


        private BackgroundWorker _backgroundWorker;

        #region Constructors
        
        public TargetedAligner(Run run, DeconToolsTargetedWorkflowParameters workflowParameters)
        {
            _run = run;
            _parameters = workflowParameters;
            _netGroupings = createNETGroupings();
            _workflow = new BasicTargetedWorkflow(_run, workflowParameters);

            IsAlignmentInfoExported = true;

            NumberOfDesiredMassTagsFoundPerNETGrouping = 25;
            NumberOfChromPeaksWithinToleranceAllowed = 1;

            NumberOfMaxAttemptsPerNETGrouping = 200;
            this.UpperFitScoreAllowedCriteria = 0.1;
            this.MinimumChromPeakIntensityCriteria = 2.5e5f;
            this.IScoreAllowedCriteria = 0.15;
            this.AreFeaturesSavedToTextFile = true;

        }

        #endregion

        #region Properties

        public bool IsAlignmentInfoExported { get; set; }
        
        
        public string ExportAlignmentFolder { get; set; }

        public string ImportedFeaturesFilename { get; set; }

        #endregion

        #region Public Methods

        public void Execute()
        {
            Check.Require(_run != null, "Run has not been defined.");
      


            //Use Features file if it exists; if not, find the quality Features
            List<MassTagResultBase> resultsPassingCriteria;
            _targetedResultRepository = new TargetedResultRepository();

            if (ImportedFeaturesFilename == null || ImportedFeaturesFilename.Length == 0)
            {
               Check.Require(_run.ResultCollection.MSPeakResultList != null && _run.ResultCollection.MSPeakResultList.Count > 0, "Dataset's Peak-level data is empty. This is needed for chromatogram generation.");


               resultsPassingCriteria =  FindTargetsThatPassCriteria();
               _targetedResultRepository.AddResults(resultsPassingCriteria);

               if (AreFeaturesSavedToTextFile)
               {
                   string outputfolder;

                   if (ExportAlignmentFolder == null || ExportAlignmentFolder.Length == 0)
                   {
                       outputfolder = _run.DataSetPath;
                   }
                   else
                   {
                       outputfolder = ExportAlignmentFolder;
                   }


                   string exportTargetedFeaturesFile = outputfolder + "\\" + _run.DatasetName + "_alignedFeatures.txt";

                   UnlabelledTargetedResultToTextExporter exporter = new UnlabelledTargetedResultToTextExporter(exportTargetedFeaturesFile);
                   exporter.ExportResults(_targetedResultRepository.Results);

               }


            }
            else
            {
                //load them from the Features file

                UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(ImportedFeaturesFilename);
                TargetedResultRepository repo = importer.Import();
                _targetedResultRepository.Results = repo.Results;

             

                

            }

            
            //do alignment
            NETAndMassAligner aligner = new NETAndMassAligner();
            aligner.SetFeaturesToBeAligned(_targetedResultRepository.Results);
            aligner.SetReferenceMassTags(this.MassTagList);
            aligner.Execute(this._run);

            //save the alignment data
            if (IsAlignmentInfoExported)
            {
                string outputfolder;

                if (ExportAlignmentFolder == null || ExportAlignmentFolder.Length == 0)
                {
                    outputfolder = _run.DataSetPath;
                }
                else
                {
                    outputfolder = ExportAlignmentFolder;
                }

                string exportNETAlignmentFilename = outputfolder + "\\" + _run.DatasetName + "_NETAlignment.txt";
                string exportMZAlignmentFilename = outputfolder + "\\" + _run.DatasetName + "_MZAlignment.txt";

                MassAlignmentInfoToTextExporter mzAlignmentExporter = new MassAlignmentInfoToTextExporter(exportMZAlignmentFilename);
                mzAlignmentExporter.ExportAlignmentInfo(_run.AlignmentInfo);

                NETAlignmentInfoToTextExporter netAlignmentExporter = new NETAlignmentInfoToTextExporter(exportNETAlignmentFilename);
                netAlignmentExporter.ExportAlignmentInfo(_run.AlignmentInfo);

            }
        }



        public List<MassTagResultBase> FindTargetsThatPassCriteria()
        {
            Check.Require(this.MassTagList != null && this.MassTagList.Count > 0, "MassTags have not been defined.");
            Check.Require(_run != null, "Run is null");

            List<MassTagResultBase> resultsPassingCriteria = new List<MassTagResultBase>();

            int netGroupingCounter = 0;
            foreach (var netGrouping in _netGroupings)
            {
                string progressString = "NET grouping " + netGrouping.Lower;
                reportProgess(progressString);

                netGroupingCounter++;

                var filteredMasstags = (from n in this.MassTagList
                                        where n.NETVal >= netGrouping.Lower && n.NETVal < netGrouping.Upper
                                        select n);

                int numPassingMassTagsInGrouping = 0;
                int numFailingMassTagsInGrouping = 0;
                

                foreach (var massTag in filteredMasstags)
                {
                    _run.CurrentMassTag = massTag;
                    _workflow.Execute();

                    var result = _run.ResultCollection.GetMassTagResult(massTag);

                    if (resultPassesCriteria(result))
                    {
                        reportProgess(massTag.ID + "; z= " + massTag.ChargeState + " added.");
                        resultsPassingCriteria.Add(result);   //where passing results are added
                        numPassingMassTagsInGrouping++;
                    }
                    else
                    {
                        numFailingMassTagsInGrouping++;
                    }

                    if (numPassingMassTagsInGrouping >= NumberOfDesiredMassTagsFoundPerNETGrouping)
                    {
                        break;   //found enough massTags in this grouping
                    }

                    if (numFailingMassTagsInGrouping > NumberOfMaxAttemptsPerNETGrouping)
                    {
                        break;  // too many failed massTags in this grouping. Will move on to next grouping
                    }

                }
            }

            return resultsPassingCriteria;




        }

        private void reportProgess(string progressString)
        {
            if (_backgroundWorker == null)
            {
                Console.WriteLine(DateTime.Now + "\t" + progressString);
            }
        }

        private bool resultPassesCriteria(MassTagResultBase result)
        {
            bool passesCriteria = true;

            if (result.ChromPeakSelected == null) return false;

            if (result.IsotopicProfile == null) return false;

            if (result.NumChromPeaksWithinTolerance > NumberOfChromPeaksWithinToleranceAllowed) return false;
            


            if (result.ChromPeakSelected.Height < MinimumChromPeakIntensityCriteria) return false;

            if (result.Score > UpperFitScoreAllowedCriteria) return false;

            if (result.InterferenceScore > IScoreAllowedCriteria) return false;

            if (result.Flags.Count > 0) return false;



            return passesCriteria;
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




        #endregion

        #region Private Methods

        #endregion

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
        public List<MassTag> MassTagList { get; set; }

        public double UpperFitScoreAllowedCriteria { get; set; }

        public double IScoreAllowedCriteria { get; set; }

        public float MinimumChromPeakIntensityCriteria { get; set; }

        public int NumberOfDesiredMassTagsFoundPerNETGrouping { get; set; }

        public int NumberOfMaxAttemptsPerNETGrouping { get; set; }

        
        /// <summary>
        /// Flag for indicating whether or not to export features involved in the alignment process. 
        /// </summary>
        public bool AreFeaturesSavedToTextFile { get; set; }

        public int NumberOfChromPeaksWithinToleranceAllowed { get; set; }
    }
}
