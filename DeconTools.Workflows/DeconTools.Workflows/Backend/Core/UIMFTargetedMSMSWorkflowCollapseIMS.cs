using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
	public class UIMFTargetedMSMSWorkflowCollapseIMS : TargetedWorkflow
	{
		public Dictionary<ChromPeak, XYData> ChromPeakToXYDataMap { get; set; }

		public UIMFTargetedMSMSWorkflowCollapseIMS(Run run, TargetedWorkflowParameters parameters):base(run,parameters)
        {
            
			this.ChromPeakToXYDataMap = new Dictionary<ChromPeak, XYData>();

            
        }

		public UIMFTargetedMSMSWorkflowCollapseIMS(TargetedWorkflowParameters parameters)
			: this(null, parameters)
		{

		}

        protected override DeconTools.Backend.Globals.ResultType GetResultType()
        {
            return DeconTools.Backend.Globals.ResultType.BASIC_TARGETED_RESULT;
        }

        public override void DoWorkflow()
        {
            Result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
            Result.ResetResult();

            ExecuteTask(_theorFeatureGen);
            ExecuteTask(_chromGen);
            ExecuteTask(_chromSmoother);
            updateChromDataXYValues(Run.XYData);

            ExecuteTask(_chromPeakDetector);
            UpdateChromDetectedPeaks(Run.PeakList);

            UpdateChromPeaksWithXYData();

			// TODO: When/how should I look for the IMS chromatogram inside the LC peak/range we are looking?

			// TODO: The chrom peak selector is only being used currently to calculate fit scores and other stats for each chrom peak.
			// TODO: With this workflow, we are never selecting a single peak as an answer. That is handled in InformedProteomics.
			// TODO: Something else to consider is if the scoring that InformedProteomics does should be the peak selector. Do we want InformedProteomics to give either 0 or 1 answers for a possible target?
            ExecuteTask(_chromPeakSelector);
            //ChromPeakSelected = Result.ChromPeakSelected;

			// TODO: Because we do not use a single best answer, the below tasks do not actually accomplish anything for the workflow.
            //Result.ResetMassSpectrumRelatedInfo();

            //ExecuteTask(MSGenerator);
            //updateMassSpectrumXYValues(Run.XYData);

            //ExecuteTask(_msfeatureFinder);

            //ExecuteTask(_fitScoreCalc);
            //ExecuteTask(_resultValidator);

            //if (_workflowParameters.ChromatogramCorrelationIsPerformed)
            //{
            //    ExecuteTask(_chromatogramCorrelatorTask);
            //}
        }



		private void UpdateChromPeaksWithXYData()
		{
			if (Run.XYData == null || Run.XYData.Xvalues == null) return;

			foreach (ChromPeak peak in Run.PeakList)
			{
				double apex = peak.XValue;
				double width = peak.Width;
				double peakWidthSigma = width / 2.35;    // width@half-height = 2.35σ (Gaussian peak theory)
				double fourSigma = 4 * peakWidthSigma;	// width@base = 4σ (Gaussian peak theory)
				double halfFourSigma = fourSigma / 2.0;

				double minScan = apex - halfFourSigma;
				double maxScan = apex + halfFourSigma;

				XYData filteredXYData = Run.XYData.TrimData(minScan, maxScan);
				this.ChromPeakToXYDataMap.Add(peak, filteredXYData);
			}
		}
	}
}
