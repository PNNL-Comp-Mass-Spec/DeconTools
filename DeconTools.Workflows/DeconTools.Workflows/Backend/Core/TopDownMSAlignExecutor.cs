using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class TopDownMSAlignExecutor:IqExecutor
    {
        private RunFactory _runFactory = new RunFactory();

        private readonly IqTargetUtilities _targetUtilities = new IqTargetUtilities();
        private readonly IqResultUtilities _iqResultUtilities = new IqResultUtilities();

        private string _resultsDirectory;

        #region constructors

        public TopDownMSAlignExecutor(WorkflowExecutorBaseParameters parameters, Run run) : base(parameters, run)
        {
        }

        public TopDownMSAlignExecutor(WorkflowExecutorBaseParameters parameters, Run run, BackgroundWorker backgroundWorker) : base(parameters, run, backgroundWorker)
        {
        }

        #endregion



        #region public methods

        public override void LoadAndInitializeTargets(string targetsFilePath)
        {
            if (TargetImporter == null)
            {
                TargetImporter = new MSAlignIqTargetImporter(targetsFilePath);
            }

            Targets = TargetImporter.Import();

            foreach (TopDownIqTarget target in Targets)
            {
                setParentNetFromChildren(target);
                _targetUtilities.UpdateTargetMissingInfo(target);
                target.RefineChildTargets();
                target.SetChildrenFromParent();
            }
        }

        /// <summary>
        /// Sets the parents NET value based on the scan numbers observed in the children.
        /// </summary>
        public void setParentNetFromChildren(IqTarget target)
        {
            var children = target.ChildTargets();
            var scanList = new List<int>();
            foreach (IqChargeStateTarget chargeStateTarget in children)
            {
                scanList.Add(chargeStateTarget.ObservedScan);
            }
            scanList.Sort();
            target.ElutionTimeTheor = Run.NetAlignmentInfo.GetNETValueForScan(scanList[scanList.Count / 2]);
        }


        #endregion

        protected override void ReportGeneralProgress(int currentTarget, int totalTargets, ref DateTime lastProgress, int progressIntervalSeconds = 10)
        {
            var currentProgress = (currentTarget / (double)totalTargets);

            if (DateTime.UtcNow.Subtract(lastProgress).TotalSeconds >= progressIntervalSeconds)
            {
                lastProgress = DateTime.UtcNow;
                IqLogger.LogMessage("Processing target " + currentTarget + " of " + totalTargets + "; " + (Math.Round(currentProgress * 100, 1)) + "% Complete.");
            }

            if (_backgroundWorker != null)
            {
                _progressInfo.ProgressInfoString = "Processing Targets: ";
                _backgroundWorker.ReportProgress(Convert.ToInt16(currentProgress * 100), _progressInfo);
            }
        }


        protected override void ExportResults(IqResult iqResult)
        {
            var resultsForExport = _iqResultUtilities.FlattenOutResultTree(iqResult);

            var orderedResults = resultsForExport.OrderBy(p => p.Target.ChargeState).ToList();

            var exportedResults = orderedResults.Where(orderedResult => orderedResult.IsExported).ToList();

            if (IqResultExporter == null)
            {
                IqResultExporter = iqResult.Target.Workflow.CreateExporter();
            }

            SetupResultsFolder();

            IqResultExporter.WriteOutResults(Path.Combine(_resultsDirectory, Run.DatasetName), exportedResults);
        }


        #region private methods


        private void SetupResultsFolder()
        {
            if (string.IsNullOrEmpty(Parameters.OutputDirectoryBase))
            {
                _resultsDirectory = GetDefaultOutputDirectory();
            }
            else
            {
                _resultsDirectory = Path.Combine(Parameters.OutputDirectoryBase, "IqResults");
            }

            if (!Directory.Exists(_resultsDirectory)) Directory.CreateDirectory(_resultsDirectory);


        }


        private string GetDefaultOutputDirectory()
        {
            var defaultOutputDirectory = Run.DatasetDirectoryPath;
            return defaultOutputDirectory;
        }


        #endregion

    }
}
