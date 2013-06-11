using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend
{
    public class IqMassAndNetAligner : IqExecutor
    {
        private IqTargetUtilities _targetUtilities = new IqTargetUtilities();
        private PeptideUtils _peptideUtils = new PeptideUtils();
        private List<int> _msgfIdsUsedInNetAlignment = new List<int>();

        #region Constructors

        public IqMassAndNetAligner(WorkflowExecutorBaseParameters parameters, Run run)
            : base(parameters, run)
        {
            IsDataExported = false;

           
            LoessBandwidthNetAlignment = 0.2;
            LoessBandwidthMassAlignment = 0.2;

            ScanToNetAlignmentData = new List<ScanNETPair>();

        }

        #endregion

        #region Properties

        public double LoessBandwidthNetAlignment { get; set; }
        public double LoessBandwidthMassAlignment { get; set; }

        public XYData LoessScanToNetAlignmentXYData { get; set; }

        public List<ScanNETPair> ScanToNetAlignmentData { get; set; }

        public MassAlignmentInfo MassAlignmentInfo { get; set; }

        public NetAlignmentInfo NetAlignmentInfo { get; set; }

        public XYData MassAlignmentXYData { get; set; }


        public List<IqResult> IqResultsForAlignment { get; set; }

        protected List<IqTarget> MassTagReferences { get; set; }

        #endregion

        #region Public Methods
        public void SetMassTagReferences(List<IqTarget> massTagReferences)
        {
            MassTagReferences = massTagReferences;
        }


        public override void LoadAndInitializeTargets(string targetsFilePath)
        {
            if (string.IsNullOrEmpty(targetsFilePath))
            {
                IqLogger.Log.Info("IqMassAndNetAligner - no alignment targets were loaded. The inputted targets file path is NULL.");
                return;
            }
            
            if (!File.Exists(targetsFilePath))
            {
                IqLogger.Log.Info("IqMassAndNetAligner - no alignment targets were loaded. The inputted targets file path is does not exist.");
                return;
            }

            var importer = new IqTargetsFromFirstHitsFileImporter(targetsFilePath);
            Targets = importer.Import().Where(p => p.QualityScore < 0.01).OrderBy(p => p.ID).ToList();

            //Targets = Targets.Where(p => p.Code.Contains("FEQDGENYTGTIDGNMGAYAR")).ToList();

            List<IqTarget> filteredList = new List<IqTarget>();
            //calculate empirical formula for targets using Code and then monoisotopic mass

            foreach (var iqTarget in Targets)
            {

                iqTarget.Code = _peptideUtils.CleanUpPeptideSequence(iqTarget.Code);

                if (_peptideUtils.ValidateSequence(iqTarget.Code))
                {
                    iqTarget.EmpiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(iqTarget.Code, true, true);
                    double calcMonoMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(iqTarget.EmpiricalFormula);
                    double monoMassFromFirstHitsFile = iqTarget.MonoMassTheor;

                    bool massCalculationsAgree = Math.Abs(monoMassFromFirstHitsFile - calcMonoMass) < 0.02;
                    if (massCalculationsAgree)
                    {
                        iqTarget.MonoMassTheor = calcMonoMass;
                        iqTarget.ElutionTimeTheor = iqTarget.ScanLC / (double)Run.MaxLCScan;

                        filteredList.Add(iqTarget);
                        _targetUtilities.UpdateTargetMissingInfo(iqTarget, true);

                        IqTargetMsgfFirstHit chargeStateTarget = new IqTargetMsgfFirstHit();

                        _targetUtilities.CopyTargetProperties(iqTarget, chargeStateTarget);

                        iqTarget.AddTarget(chargeStateTarget);
                    }
                }
            }

            Targets = filteredList;

            TargetedWorkflowParameters workflowParameters = new BasicTargetedWorkflowParameters();
            workflowParameters.ChromNETTolerance = 0.005;
            workflowParameters.ChromGenTolerance = 50;
           
            //define workflows for parentTarget and childTargets
            var parentWorkflow = new ChromPeakDeciderIqWorkflow(Run, workflowParameters);
            var childWorkflow = new ChargeStateChildIqWorkflow(Run, workflowParameters);

            IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, Targets);


            if (Targets.Count>0)
            {
                IqLogger.Log.Info("IqMassAndNetAligner - Loaded " + Targets.Count + " targets for use in mass and net alignment");
            }
            else
            {
                IqLogger.Log.Info("IqMassAndNetAligner - NOTE - no targets have been loaded.");
            }
            


            //IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
            //workflowAssigner.AssignWorkflowToParent(workflow, Targets);


        }


        public void ExecuteAlignment()
        {
            MassAlignmentInfo = DoMassAlignment();
            NetAlignmentInfo = DoNetAlignment();
        }



        public void LoadPreviousIqResults(string filename)
        {
            IqResultImporter importer = new IqResultImporterBasic(filename);
            IqResultsForAlignment = importer.Import();

            IqLogger.Log.Info("IqMassAndNetAligner - Imported " + IqResultsForAlignment.Count + " IqResults for use in alignment. ");


        }


        public void ExportResults(string filename)
        {
            if (IqResultsForAlignment==null || IqResultsForAlignment.Count==0)
            {
                IqLogger.Log.Info("Iq mass and NET aligner trying to export results. But no results exist.");
                return;
            }

            try
            {
                ResultExporter exporter = new IqLabelFreeResultExporter();
                exporter.WriteOutResults(filename, IqResultsForAlignment);
            }
            catch (Exception ex)
            {
                IqLogger.Log.Info("Iq mass and NET aligner trying to export results but there was an error. Error details: " + ex.Message);

            }
           
        }

        #endregion

        #region Private Methods

        #endregion


        public MassAlignmentInfo DoMassAlignment(bool recollectResultsIfAlreadyPresent = false)
        {
            bool needToProcessResults = (recollectResultsIfAlreadyPresent ||
                                         (IqResultsForAlignment == null || IqResultsForAlignment.Count == 0));

            if (needToProcessResults)
            {
                Execute(Targets);
                IqResultsForAlignment = FilterAlignmentResults();
            }




            XYData massAlignmentDataForLoess = PrepareMassAlignmentDataForLoessSmoothing(IqResultsForAlignment);


            int iterationsForMassAlignment = 2;
            var loessInterpolatorForMassAlignment = new LoessInterpolator(LoessBandwidthMassAlignment, iterationsForMassAlignment);


            IqLogger.Log.Info("Applying Loess smoothing to mass alignment data. LoessBandwidth= "+ LoessBandwidthMassAlignment);


            var massAlignmentInfo = new MassAlignmentInfoBasic();
            massAlignmentInfo.ScanAndPpmShiftVals = new XYData();
            massAlignmentInfo.ScanAndPpmShiftVals.Xvalues = massAlignmentDataForLoess.Xvalues;
            massAlignmentInfo.ScanAndPpmShiftVals.Yvalues = loessInterpolatorForMassAlignment.Smooth(massAlignmentDataForLoess.Xvalues, massAlignmentDataForLoess.Yvalues);


            if (massAlignmentInfo.GetNumPoints() > 0)
            {
                massAlignmentInfo.AveragePpmShift = massAlignmentInfo.ScanAndPpmShiftVals.Yvalues.Average();
            }
            else
            {
                massAlignmentInfo.AveragePpmShift = double.NaN;
            }


            if (massAlignmentInfo.GetNumPoints() > 2)
            {
                massAlignmentInfo.StdevPpmShiftData = MathUtils.GetStDev(massAlignmentInfo.ScanAndPpmShiftVals.Yvalues);
            }
            else
            {
                massAlignmentInfo.StdevPpmShiftData = double.NaN;
            }


            IqLogger.Log.Info("Mass alignment complete using " + massAlignmentInfo.GetNumPoints() + " data points");

            IqLogger.Log.Info("Average ppm shift = \t" + massAlignmentInfo.AveragePpmShift.ToString("0.00")+ " +/- " + massAlignmentInfo.StdevPpmShiftData.ToString("0.00"));
            return massAlignmentInfo;
        }

        public NetAlignmentInfo DoNetAlignment(bool recollectResultsIfAlreadyPresent = false)
        {
            bool needToProcessResults = (recollectResultsIfAlreadyPresent ||
                                       (IqResultsForAlignment == null || IqResultsForAlignment.Count == 0));

            if (needToProcessResults)
            {
                Execute(Targets);
                IqResultsForAlignment = FilterAlignmentResults();
            }

            IqResultsForNetAlignment = UpdateMsgfTargetsWithDatabaseNetValues(IqResultsForAlignment);

            IqLogger.Log.Info("Applying Loess smoothing to NET alignment data. LoessBandwidth= "+ LoessBandwidthNetAlignment);
            
            var netAlignmentDataForLoess = PrepareNetAlignmentDataForLoessSmoothing();

           


            int iterationsForNetAlignment = 2;

            var loessInterpolatorForNetAlignment = new LoessInterpolator(LoessBandwidthNetAlignment, iterationsForNetAlignment);
            var loessSmoothedData = new XYData();
            loessSmoothedData.Xvalues = netAlignmentDataForLoess.Select(p => p.Scan).ToArray();
            loessSmoothedData.Yvalues = loessInterpolatorForNetAlignment.Smooth(netAlignmentDataForLoess.Select(p => p.Scan).ToArray(),
                                                                                 netAlignmentDataForLoess.Select(p => p.NET).ToArray());

            var scanToNetVals = new List<ScanNETPair>();
            for (int i = 0; i < loessSmoothedData.Xvalues.Length; i++)
            {
                var xval = loessSmoothedData.Xvalues[i];
                var yval = loessSmoothedData.Yvalues[i];

                if (!double.IsNaN(yval))
                {
                    var scanNETPair = new ScanNETPair(xval, yval);
                    scanToNetVals.Add(scanNETPair);
                }

            }

            var netAlignmentInfo = new NetAlignmentInfoBasic(Run.MinLCScan, Run.MaxLCScan);
            netAlignmentInfo.SetScanToNETAlignmentData(scanToNetVals);


            IqLogger.Log.Info("NET alignment complete using " + scanToNetVals.Count + " data points.");



            return netAlignmentInfo;
        }





        protected List<IqResult> IqResultsForNetAlignment { get; set; }


        private XYData PrepareMassAlignmentDataForLoessSmoothing(List<IqResult> iqResultsForAlignment)
        {
            var scanMassVals = new Dictionary<decimal, double>();

            foreach (IqResult iqResult in iqResultsForAlignment)
            {

                decimal lcScan;
                if (iqResult.ChromPeakSelected==null)
                {
                    lcScan = iqResult.LcScanObs;
                }
                else
                {
                    lcScan = (decimal)Math.Round(iqResult.ChromPeakSelected.XValue, 2);
                }


                
                double yval = iqResult.MassErrorBefore;

                if (!scanMassVals.ContainsKey(lcScan))
                {
                    scanMassVals.Add(lcScan, yval);
                }
            }

            XYData xyDataForInterpolator = new XYData();
            xyDataForInterpolator.Xvalues = scanMassVals.Keys.Select(p => (double)p).ToArray();
            xyDataForInterpolator.Yvalues = scanMassVals.Values.ToArray();

            return xyDataForInterpolator;

        }


        private List<ScanNETPair> PrepareNetAlignmentDataForLoessSmoothing()
        {
            var scanNetDictionary = new SortedDictionary<decimal, double>();

            foreach (IqResult iqResult in IqResultsForNetAlignment)
            {

                decimal lcScan;
                if (iqResult.ChromPeakSelected == null)
                {
                    lcScan = iqResult.LcScanObs;
                }
                else
                {
                    lcScan = (decimal)Math.Round(iqResult.ChromPeakSelected.XValue, 2);
                }

                double yval = iqResult.Target.ElutionTimeTheor;

                if (!scanNetDictionary.ContainsKey(lcScan))
                {
                    scanNetDictionary.Add(lcScan, yval);
                }
            }


            var scanNETPairs = new List<ScanNETPair>();
            foreach (var pair in scanNetDictionary)
            {
                var scanNETPair = new ScanNETPair((double)pair.Key, pair.Value);
                scanNETPairs.Add(scanNETPair);
            }

            return scanNETPairs;

        }



        private List<IqResult> FilterAlignmentResults()
        {
            List<IqResult> filteredResults = new List<IqResult>();

            foreach (var iqResult in Results)
            {
                var childResults = iqResult.ChildResults();
                filteredResults.AddRange(childResults.Where(p => p.FitScore < 0.2));
            }

            return filteredResults.OrderBy(p => p.ChromPeakSelected.XValue).ToList();
        }

        private List<IqResult> UpdateMsgfTargetsWithDatabaseNetValues(List<IqResult> results)
        {
            if (MassTagReferences == null || MassTagReferences.Count == 0) return results;


            var msgfIdsUsedInNetAlignment = new List<int>();
            var query = (from massTag in MassTagReferences
                         join result in results on massTag.Code equals result.Target.Code
                         select new
                         {
                             MassTag = massTag,
                             MsgfTarget = result.Target
                         }).ToList();


            foreach (var thing in query)
            {
                msgfIdsUsedInNetAlignment.Add(thing.MsgfTarget.ID);
                thing.MsgfTarget.ElutionTimeTheor = thing.MassTag.ElutionTimeTheor;
            }

            var resultsUsedForNetAlignment = (from n in results where msgfIdsUsedInNetAlignment.Contains(n.Target.ID) select n).ToList();
            return resultsUsedForNetAlignment;
        }
    }
}
