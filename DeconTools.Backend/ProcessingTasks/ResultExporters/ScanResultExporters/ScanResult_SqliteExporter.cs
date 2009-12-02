using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public abstract class ScanResult_SqliteExporter : IScanResultExporter
    {
        protected DbConnection cnn;

        #region Properties
        #endregion

        #region Public Methods
        public override void ExportScanResults(DeconTools.Backend.Core.ResultCollection resultList)
        {
            addScanResults(resultList);
        }
        #endregion

        #region Private Methods
        #endregion

        protected abstract void buildTables();
        protected abstract void addScanResults(DeconTools.Backend.Core.ResultCollection resultList);

    
    }
}
