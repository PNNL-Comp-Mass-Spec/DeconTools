using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public class MSPeakTable :Table
    {
        #region Constructors
        public MSPeakTable(string tableName)
        {
            this.Name = tableName;
            this.FieldList = new List<Field>();
            this.FieldList.Add(new Field("peak_id", "INTEGER Primary key"));
            this.FieldList.Add(new Field("scan_num", "INTEGER"));
            this.FieldList.Add(new Field("mz", "FLOAT"));
            this.FieldList.Add(new Field("intensity", "FLOAT"));
            this.FieldList.Add(new Field("fwhm", "FLOAT"));
            this.FieldList.Add(new Field("msfeatureID", "FLOAT"));
         
            


        }
        #endregion

        #region Properties
        public override List<Field> FieldList { get; set; }

        public override string Name { get; set; }

        #endregion

        #region Public Methods


        #endregion

        #region Private Methods
        #endregion


    }
}
