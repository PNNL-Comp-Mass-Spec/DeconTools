using System.Collections.Generic;
using System.Data.Common;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public abstract class IsosResultSqliteExporter:IsosResultExporter
    {
        protected DbConnection cnn;

        #region Properties
        #endregion

        #region Public Methods
        public override void ExportIsosResults(List<IsosResult> isosResultList)
        {
            addIsosResults(isosResultList);
        }
        #endregion

        #region Private Methods
        #endregion

        protected abstract void buildTables();
        protected abstract void addIsosResults(List<IsosResult> isosResultList);

        public override int TriggerToExport { get; set; }
    }
}
