using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public class TaskCleaner
    {
        private TaskCollection taskcollection;

        public TaskCollection Taskcollection
        {
            get { return taskcollection; }
            set { taskcollection = value; }
        }

        public TaskCleaner(TaskCollection taskcollection)
        {
            this.taskcollection = taskcollection;

        }

        public void CleanTasks()
        {
            foreach (Task task in taskcollection.TaskList)
            {
                task.Cleanup();
            }

        }


    }
}
