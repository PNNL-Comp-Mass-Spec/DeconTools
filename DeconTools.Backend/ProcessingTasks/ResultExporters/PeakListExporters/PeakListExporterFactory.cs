using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters
{
    public class PeakListExporterFactory
    {

        public PeakListExporterFactory()
        {

        }

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public Task Create(Globals.ExporterType exporterType, Globals.MSFileType fileType, int triggerValue, string outputFileName)
        {
            Task exporter;

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


        #endregion

        #region Private Methods
        #endregion
    }
}
