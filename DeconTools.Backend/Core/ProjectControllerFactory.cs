using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DeconTools.Backend.ProjectControllers;

namespace DeconTools.Backend.Core
{
    public class ProjectControllerFactory
    {
        #region Constructors

        Globals.ProjectControllerType m_projectControllerType;

        public ProjectControllerFactory(Globals.ProjectControllerType projectControllerType)
        {
            m_projectControllerType = projectControllerType;


        }



        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public ProjectController CreateProjectController(string inputDataFilename, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName)
        {
            return CreateProjectController(inputDataFilename, fileType, paramFileName, null);
        }

        public ProjectController CreateProjectController(string inputDataFilename, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName, BackgroundWorker bw)
        {
            ProjectController projectController;

            switch (m_projectControllerType)
            {

                case Globals.ProjectControllerType.STANDARD:
                    projectController = new OldSchoolProcRunner(inputDataFilename, fileType, paramFileName, bw);
                    break;
                //case Globals.ProjectControllerType.BONES_CONTROLLER:
                //    break;
                //case Globals.ProjectControllerType.RUN_MERGER_CONTROLLER:
                //    break;
                case Globals.ProjectControllerType.KOREA_IMS_PEAKSONLY_CONTROLLER:
                    projectController = new KoreaIMSTextFileProjectController(inputDataFilename, fileType, paramFileName, bw);
                    break;
                default:
                    projectController = new OldSchoolProcRunner(inputDataFilename, fileType, paramFileName, bw);
                    break;
            }

            return projectController;

        }


        #endregion

        #region Private Methods
        #endregion
    }
}
