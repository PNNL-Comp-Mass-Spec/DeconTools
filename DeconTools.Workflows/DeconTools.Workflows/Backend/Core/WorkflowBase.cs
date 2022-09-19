using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class WorkflowBase
    {
        protected MSGenerator MSGenerator { get; set; }

        #region Public Methods

        public abstract void Execute();

        public virtual void ExecuteTask(Task task)
        {
            if (Result?.FailedResult == false)
            {
                task.Execute(Run.ResultCollection);
            }
        }

        public virtual void InitializeRunRelatedTasks()
        {
            if (Run != null)
            {
                MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
            }
        }

        private Run _run;
        public Run Run
        {
            get => _run;
            set
            {
                if (_run == value)
                {
                    return;
                }

                _run = value;

                if (_run != null)
                {
                    InitializeRunRelatedTasks();
                }
            }
        }

        public TargetedResultBase Result { get; set; }

        #endregion

        public virtual void Execute(IqTarget target)
        {
            throw new NotImplementedException("IqWorkflow not implemented");
        }
    }
}
