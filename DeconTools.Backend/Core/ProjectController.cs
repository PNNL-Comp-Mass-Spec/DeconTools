
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

        public bool OverwriteLogFile { get; set; }

        protected Globals.ExporterType getExporterTypeFromOldParameters(OldDecon2LSParameters oldDecon2LSParameters)
        {
            switch (oldDecon2LSParameters.HornTransformParameters.ExportFileType)
            {
                case DeconToolsV2.HornTransform.enmExportFileType.SQLITE:
                    return Globals.ExporterType.SQLite;
                    
                case DeconToolsV2.HornTransform.enmExportFileType.TEXT:
                    return Globals.ExporterType.TEXT;
                    
                default:
                    return Globals.ExporterType.TEXT;
                    
            }
        }

    }
}
