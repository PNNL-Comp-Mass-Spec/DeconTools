
using DeconTools.Backend.Runs;
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


        protected Globals.ResultType GetResultType(Run run, OldDecon2LSParameters oldDecon2LSParameters)
        {

            if (run is UIMFRun) return Globals.ResultType.UIMF_TRADITIONAL_RESULT;
            if (run is IMFRun) return Globals.ResultType.IMS_TRADITIONAL_RESULT;

            if (oldDecon2LSParameters.HornTransformParameters.O16O18Media)
            {
                return Globals.ResultType.O16O18_TRADITIONAL_RESULT;
            }
            else
            {
                return Globals.ResultType.BASIC_TRADITIONAL_RESULT;
            }

        }

    }
}
