using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public abstract class ProjectController
    {
        public abstract void Execute();


        private Project project;

        public Project Project
        {
            get { return project; }

        }



        protected Globals.ExporterType getExporterTypeFromOldParameters(OldDecon2LSParameters oldDecon2LSParameters)
        {
            switch (oldDecon2LSParameters.HornTransformParameters.ExportFileType)
            {
                case DeconToolsV2.HornTransform.enmExportFileType.SQLITE:
                    return Globals.ExporterType.SQLite;
                    break;
                case DeconToolsV2.HornTransform.enmExportFileType.TEXT:
                    return Globals.ExporterType.TEXT;
                    break;
                default:
                    return Globals.ExporterType.TEXT;
                    break;
            }
        }

    }
}
