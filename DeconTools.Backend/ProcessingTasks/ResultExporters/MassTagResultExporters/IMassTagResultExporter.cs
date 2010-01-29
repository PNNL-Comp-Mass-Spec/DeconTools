using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.MassTagResultExporters
{
    public abstract class IMassTagResultExporter:Task
    {
        #region Constructors
        #endregion

        #region Properties
        public abstract int TriggerToExport { get; set; }

        #endregion

        #region Public Methods
        public abstract void ExportMassTagResults(ResultCollection resultColl);

        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultColl)
        {
            ExportMassTagResults(resultColl);
        }

        protected abstract void addResults(ResultCollection resultColl);
    }
}
