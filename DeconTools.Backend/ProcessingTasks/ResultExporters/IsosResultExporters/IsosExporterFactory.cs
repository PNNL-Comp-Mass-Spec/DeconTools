using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters;

namespace DeconTools.Backend.Data
{
    public class IsosExporterFactory
    {
        DeconTools.Backend.Globals.ExporterType exporterType;

        private int triggerToExportValue;

        public IsosExporterFactory()
            : this(10000)
        {

        }

        public IsosExporterFactory(int triggerToExportValue)
        {
            this.triggerToExportValue = triggerToExportValue;

        }

        public Task CreateIsosExporter(Globals.MSFileType filetype, DeconTools.Backend.Globals.ExporterType exporterType, string outputFileName)
        {
            Task isosExporter;

            switch (filetype)
            {
                case Globals.MSFileType.PNNL_IMS:

                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new IMFIsosResult_TextFileExporter(outputFileName, triggerToExportValue);
                            break;
                        case Globals.ExporterType.SQLite:
                            throw new NotImplementedException();
                            break;
                        default:
                            isosExporter = new IMFIsosResult_TextFileExporter(outputFileName, triggerToExportValue);
                            break;
                    }

                    break;
                case Globals.MSFileType.PNNL_UIMF:
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

                default:
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
            }
            return isosExporter;

        }


    }
}
