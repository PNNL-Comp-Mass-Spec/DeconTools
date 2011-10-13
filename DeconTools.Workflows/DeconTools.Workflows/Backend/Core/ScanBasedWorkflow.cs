using System;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.Workflows.Backend.Core
{
    public class ScanBasedWorkflow : TargetedWorkflow
    {
       private I_MSGenerator msgen;


        #region Constructors
        public ScanBasedWorkflow(Run run)
        {
            this.Run = run;
            this.NumScansSummed = 1;
        }

        public ScanBasedWorkflow(Run run, int numScansSummed)
            : this(run)
        {
            this.NumScansSummed = numScansSummed;
            this.MassSpectrumXYData = new XYData();
            this.ResetXYData();

        }



        #endregion

        #region Properties

        public ScanSet scanSetSelected { get; set; }
       

        #endregion

        #region Public Methods

        public void SetScanSet(int scanNum)
        {
            scanSetSelected = CreateScanSet(scanNum);

        }

        private ScanSet CreateScanSet(int scanNum)
        {
            ScanSetFactory ssf = new ScanSetFactory();
            int desiredMSLevel = 1;
            int closestScanNum =   Run.GetClosestMSScan(scanNum, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);
            return ssf.CreateScanSet(this.Run, closestScanNum, this.NumScansSummed);
        }




        #endregion

        #region Private Methods

        #endregion

        public override void Execute()
        {
         

            this.Run.GetMassSpectrum(this.scanSetSelected);

            this.MassSpectrumXYData.Xvalues = this.Run.XYData.Xvalues;
            this.MassSpectrumXYData.Yvalues = this.Run.XYData.Yvalues;

        }

       

        public override void InitializeWorkflow()
        {
           

        }

        

        public int NumScansSummed { get; set; }

        public void ResetXYData()
        {
            this.MassSpectrumXYData.Xvalues = new double[] { 0, 1, 2, 3 };
            this.MassSpectrumXYData.Yvalues = new double[] { 0, 0, 0, 0 };
        }

        public override void InitializeRunRelatedTasks()
        {
            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            msgen = msgenFactory.CreateMSGenerator(this.Run.MSFileType);
        }

        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
