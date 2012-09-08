using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class WorkflowBase
    {

        string Name { get; set; }

        protected MSGenerator MSGenerator { get; set; }

        public abstract WorkflowParameters WorkflowParameters { get; set; }


        #region Public Methods

        public abstract void InitializeWorkflow();

        public abstract void Execute();

        public virtual void ExecuteTask(Task task)
        {
            if (Result != null && !Result.FailedResult)
            {
                task.Execute(this.Run.ResultCollection);
            }
        }





        public virtual void InitializeRunRelatedTasks()
        {
            if (Run != null)
            {
                MSGenerator = MSGeneratorFactory.CreateMSGenerator(this.Run.MSFileType);

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

                    if (_run!=null)
                    {
                        InitializeRunRelatedTasks(); 
                    }
                    
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
