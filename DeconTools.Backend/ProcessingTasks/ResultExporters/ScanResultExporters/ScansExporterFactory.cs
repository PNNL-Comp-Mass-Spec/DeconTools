using DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters;

namespace DeconTools.Backend.Data
{
    public class ScansExporterFactory
    {

        public static ScanResultExporter CreateScansExporter(Globals.MSFileType fileType, Globals.ExporterType exporterType, string outputFileName)
        {
            ScanResultExporter scansExporter;
            switch (fileType)
            {
                case Globals.MSFileType.PNNL_UIMF:

                    switch (exporterType)
                    {
                        case Globals.ExporterType.Text:
                            scansExporter = new UIMFScanResult_TextFileExporter(outputFileName);
                            break;
                        case Globals.ExporterType.Sqlite:
                            scansExporter = new UIMFScanResult_SqliteExporter(outputFileName);
                            break;
                        default:
                            scansExporter = new UIMFScanResult_TextFileExporter(outputFileName);
                            break;
                    }
                    break;
                default:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.Text:
                            scansExporter = new BasicScanResult_TextFileExporter(outputFileName);
                            break;
                        case Globals.ExporterType.Sqlite:
                            scansExporter = new BasicScanResult_SqliteExporter(outputFileName);
                            break;
                        default:
                            scansExporter = new BasicScanResult_TextFileExporter(outputFileName);
                            break;
                    }

                    break;
            }
            return scansExporter;

        }



    }
}
