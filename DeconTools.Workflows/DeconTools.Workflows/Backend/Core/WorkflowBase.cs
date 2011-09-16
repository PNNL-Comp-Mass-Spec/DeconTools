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

        public virtual void ExecuteTask(Task task)
        {
            if (Result!=null && !Result.FailedResult)
            {
                task.Execute(this.Run.ResultCollection);
            }
        }

        

     

        public virtual void InitializeRunRelatedTasks()
        {
            if (Run != null)
            {

                MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
                this.MSGenerator = msgenFactory.CreateMSGenerator(this.Run.MSFileType);

                
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

        public TargetedResultBase Result { get; set; }


        //public static WorkflowBase CreateWorkflow(string workflowParameterFilename)
        //{



        //}

        


        #endregion
    }
}
