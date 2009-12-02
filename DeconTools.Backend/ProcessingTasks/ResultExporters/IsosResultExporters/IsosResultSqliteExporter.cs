using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public abstract class IsosResultSqliteExporter:IIsosResultExporter
    {
        private int triggerValue;
        protected DbConnection cnn;
        
        #region Properties
        #endregion

        #region Public Methods
        public override void ExportIsosResults(DeconTools.Backend.Core.ResultCollection resultList)
        {
            addIsosResults(resultList);
        }
        #endregion



        #region Private Methods
        #endregion


        protected abstract void buildTables();
        protected abstract void addIsosResults(DeconTools.Backend.Core.ResultCollection resultList);


        public override int TriggerToExport
        {
            get
            {
                return triggerValue;
            }
            set
            {
                triggerValue = value;
            }
        }
    }
}
