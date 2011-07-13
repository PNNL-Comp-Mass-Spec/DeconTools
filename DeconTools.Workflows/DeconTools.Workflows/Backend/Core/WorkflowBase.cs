using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class WorkflowBase
    {

        string Name { get; set; }

        protected I_MSGenerator MSGenerator { get; set; }

        public abstract WorkflowParameters WorkflowParameters { get; set; }


        #region Public Methods

        public abstract void InitializeWorkflow();

        public abstract void Execute();

        

        

        public virtual IList<ChromPeak> ChromPeaksDetected { get; set; }

        public virtual ChromPeak ChromPeakSelected { get; set; }

        public virtual XYData MassSpectrumXYData { get; set; }

        public virtual XYData ChromatogramXYData { get; set; }

        public virtual void InitializeRunRelatedTasks()
        {
            if (Run != null)
            {

                MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
                this.MSGenerator = msgenFactory.CreateMSGenerator(this.Run.MSFileType);

                this.Run.ResultCollection.MassTagResultType = this.WorkflowParameters.ResultType;
            }
        }


        private Run _run;
        public Run Run 
        {
            get
            {
                return _run;
            }
            set
            {
                if (_run != value)
                {
                    _run = value;
                    InitializeRunRelatedTasks();
                }

            }
        }

        public MassTagResultBase Result { get; set; }


        //public static WorkflowBase CreateWorkflow(string workflowParameterFilename)
        //{



        //}

        protected void updateChromDetectedPeaks(List<IPeak> list)
        {
            foreach (ChromPeak chrompeak in list)
            {
                this.ChromPeaksDetected.Add(chrompeak);

            }


        }

        protected void updateChromDataXYValues(XYData xydata)
        {
            if (xydata == null)
            {
                //ResetStoredXYData(ChromatogramXYData);
                return;
            }
            else
            {
                this.ChromatogramXYData.Xvalues = xydata.Xvalues;
                this.ChromatogramXYData.Yvalues = xydata.Yvalues;
            }

        }


        protected void updateMassSpectrumXYValues(XYData xydata)
        {
            if (xydata == null)
            {
                //ResetStoredXYData(ChromatogramXYData);
                return;
            }
            else
            {
                this.MassSpectrumXYData.Xvalues = xydata.Xvalues;
                this.MassSpectrumXYData.Yvalues = xydata.Yvalues;
            }
        }


        public virtual void ResetStoredData()
        {
            this.ResetStoredXYData(this.ChromatogramXYData);
            this.ResetStoredXYData(this.MassSpectrumXYData);

            this.Run.XYData = null;
            this.Run.PeakList = new List<IPeak>();

            this.ChromPeaksDetected.Clear();
            this.ChromPeakSelected = null;
        }

        public void ResetStoredXYData(XYData xydata)
        {
            xydata.Xvalues = new double[] { 0, 1, 2, 3 };
            xydata.Yvalues = new double[] { 0, 0, 0, 0 };
        }


        #endregion
    }
}
