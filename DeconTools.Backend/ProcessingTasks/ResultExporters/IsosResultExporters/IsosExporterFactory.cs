using System;
using DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters;

namespace DeconTools.Backend.Data
{
    public class IsosExporterFactory
    {
        private const int TriggerToExportValue = 50000;

      
        public static IsosResultExporter CreateIsosExporter(Globals.ResultType resultType, Globals.ExporterType exporterType, string outputFileName)
        {
            IsosResultExporter isosExporter;

            switch (resultType)
            {
                case Globals.ResultType.BASIC_TRADITIONAL_RESULT:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new BasicIsosResultTextFileExporter(outputFileName, TriggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            isosExporter = new BasicIsosResultSqliteExporter(outputFileName, TriggerToExportValue);
                            break;
                        default:
                            isosExporter = new BasicIsosResultTextFileExporter(outputFileName, TriggerToExportValue);
                            break;
                    }


                    break;
                case Globals.ResultType.UIMF_TRADITIONAL_RESULT:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new UIMFIsosResultTextFileExporter(outputFileName, TriggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            isosExporter = new UIMFIsosResultSqliteExporter(outputFileName, TriggerToExportValue);
                            break;
                        default:
                            isosExporter = new UIMFIsosResultTextFileExporter(outputFileName, TriggerToExportValue);
                            break;
                    }

                    break;
                case Globals.ResultType.IMS_TRADITIONAL_RESULT:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new IMFIsosResult_TextFileExporter(outputFileName, TriggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            throw new NotImplementedException();
                            
                        default:
                            isosExporter = new IMFIsosResult_TextFileExporter(outputFileName, TriggerToExportValue);
                            break;
                    }

                    break;
                case Globals.ResultType.O16O18_TRADITIONAL_RESULT:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new O16O18IsosResultTextFileExporter(outputFileName, TriggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            throw new NotImplementedException();
                            
                        default:
                            isosExporter = new IMFIsosResult_TextFileExporter(outputFileName, TriggerToExportValue);
                            break;
                    }
                    break;
                case Globals.ResultType.BASIC_TARGETED_RESULT:
                    throw new ApplicationException("Cannot create IsosExporter for Targeted-type results");
                    
                case Globals.ResultType.O16O18_TARGETED_RESULT:
                    throw new ApplicationException("Cannot create IsosExporter for Targeted-type results");
                    
                case Globals.ResultType.N14N15_TARGETED_RESULT:
                    throw new ApplicationException("Cannot create IsosExporter for Targeted-type results");
                    
                default:
                    throw new ApplicationException("Cannot create IsosExporter for this type of Result: " + resultType);
                    
            }



            return isosExporter;

        }


    }
}
