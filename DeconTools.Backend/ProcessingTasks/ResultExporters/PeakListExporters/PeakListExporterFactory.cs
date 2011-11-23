using DeconTools.Backend.ProcessingTasks.PeakListExporters;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters
{
    public class PeakListExporterFactory
    {

        public PeakListExporterFactory()
        {

        }
       
        public static IPeakListExporter Create(Globals.ExporterType exporterType, Globals.MSFileType fileType, int triggerValue, string outputFileName)
        {
            IPeakListExporter exporter;

            switch (exporterType)
            {
                case Globals.ExporterType.TEXT:
                    exporter = new PeakListTextExporter(fileType, triggerValue, outputFileName);
                    break;
                case Globals.ExporterType.SQLite:
                    exporter = new PeakListSQLiteExporter(triggerValue, outputFileName);
                    break;
                default:
                    exporter = new PeakListTextExporter(fileType, triggerValue, outputFileName);
                    break;
            }
            return exporter;

        }


       
    }
}
