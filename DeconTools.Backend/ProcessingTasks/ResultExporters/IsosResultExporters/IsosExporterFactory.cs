using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters;

namespace DeconTools.Backend.Data
{
    public class IsosExporterFactory
    {
        
        private int triggerToExportValue;

        public IsosExporterFactory()
            : this(10000)
        {

        }

        public IsosExporterFactory(int triggerToExportValue)
        {
            this.triggerToExportValue = triggerToExportValue;

        }

        public Task CreateIsosExporter(Globals.ResultType resultType, DeconTools.Backend.Globals.ExporterType exporterType, string outputFileName)
        {
            Task isosExporter;

            switch (resultType)
            {
                case Globals.ResultType.BASIC_TRADITIONAL_RESULT:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new BasicIsosResultTextFileExporter(outputFileName, triggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            isosExporter = new BasicIsosResultSqliteExporter(outputFileName, triggerToExportValue);
                            break;
                        default:
                            isosExporter = new BasicIsosResultTextFileExporter(outputFileName, triggerToExportValue);
                            break;
                    }


                    break;
                case Globals.ResultType.UIMF_TRADITIONAL_RESULT:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new UIMFIsosResultTextFileExporter(outputFileName, triggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            isosExporter = new UIMFIsosResultSqliteExporter(outputFileName, triggerToExportValue);
                            break;
                        default:
                            isosExporter = new UIMFIsosResultTextFileExporter(outputFileName, triggerToExportValue);
                            break;
                    }

                    break;
                case Globals.ResultType.IMS_TRADITIONAL_RESULT:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new IMFIsosResult_TextFileExporter(outputFileName, triggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            throw new NotImplementedException();
                            
                        default:
                            isosExporter = new IMFIsosResult_TextFileExporter(outputFileName, triggerToExportValue);
                            break;
                    }

                    break;
                case Globals.ResultType.O16O18_TRADITIONAL_RESULT:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new O16O18IsosResultTextFileExporter(outputFileName, triggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            throw new NotImplementedException();
                            
                        default:
                            isosExporter = new IMFIsosResult_TextFileExporter(outputFileName, triggerToExportValue);
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
