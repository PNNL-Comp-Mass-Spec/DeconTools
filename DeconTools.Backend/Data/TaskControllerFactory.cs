using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using System.ComponentModel;

namespace DeconTools.Backend.Data
{
    public class TaskControllerFactory
    {
        private BackgroundWorker backgroundWorker;

        public TaskControllerFactory()
            : this(null)
        {

        }
        public TaskControllerFactory(BackgroundWorker backgroundWorker)
        {
            this.backgroundWorker = backgroundWorker;

        }

        public TaskController CreateTaskController(Globals.MSFileType filetype, TaskCollection taskCollection)
        {
            TaskController taskcontroller;
            switch (filetype)
            {
                case Globals.MSFileType.Undefined:
                    taskcontroller = new BasicTaskController(taskCollection, backgroundWorker);
                    break;
                case Globals.MSFileType.Agilent_TOF:
                    taskcontroller = new BasicTaskController(taskCollection);
                    break;
                case Globals.MSFileType.Ascii:
                    taskcontroller = new BasicTaskController(taskCollection);
                    break;
                case Globals.MSFileType.Bruker:
                    taskcontroller = new BasicTaskController(taskCollection);
                    break;
                case Globals.MSFileType.Bruker_Ascii:
                    taskcontroller = new BasicTaskController(taskCollection);
                    break;
                case Globals.MSFileType.Finnigan:
                    taskcontroller = new BasicTaskController(taskCollection,backgroundWorker);
                    break;
                case Globals.MSFileType.ICR2LS_Rawdata:
                    taskcontroller = new BasicTaskController(taskCollection, backgroundWorker);
                    break;
                case Globals.MSFileType.Micromass_Rawdata:
                    taskcontroller = new BasicTaskController(taskCollection);
                    break;
                case Globals.MSFileType.MZXML_Rawdata:
                    taskcontroller = new BasicTaskController(taskCollection, backgroundWorker);
                    break;
                case Globals.MSFileType.PNNL_IMS:
                    taskcontroller = new BasicTaskController(taskCollection,backgroundWorker);
                    break;
                case Globals.MSFileType.PNNL_UIMF:
                    taskcontroller = new UIMF_TaskController(taskCollection,backgroundWorker);
                    break;
                case Globals.MSFileType.SUNEXTREL:
                    taskcontroller = new BasicTaskController(taskCollection);
                    break;
                default:
                    taskcontroller = new BasicTaskController(taskCollection);
                    break;
            }
            return taskcontroller;


        }


    }
}
