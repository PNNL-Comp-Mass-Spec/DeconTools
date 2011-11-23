using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.Backend.Core
{
    public class TaskCollection
    {
        public TaskCollection()
            : this(new List<Task>())
        {
        }

        public TaskCollection(List<Task> taskList)
        {
            this.taskList = taskList;
        }




        private List<Task> taskList;

        public List<Task> TaskList
        {
            get { return taskList; }
            set { taskList = value; }
        }

        public Task GetTask(Type type)
        {
            
            //TODO:  find out if there is a better way...
            foreach (Task task in this.taskList)
            {
                if (task.GetType() == type || 
                    task.GetType().BaseType == type || 
                    task.GetType().BaseType.BaseType == type)
                {
                    return task;
                }


            }
            return null;
        }

        public string GetDeconvolutorType()
        {
            Task deconvolutor = getDeconvolutorTask();
            return deconvolutor.ToString();
        }

        private Task getDeconvolutorTask()
        {
            foreach (Task task in taskList)
            {
                if (task is Deconvolutor) return task;
            }
            return null;    //didn't find deconvolutor        }

        }
       
    }
}
