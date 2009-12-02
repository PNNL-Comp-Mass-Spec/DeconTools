using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class BasicIsosResult_SqliteTable:Table
    {
        private string name;
        private List<Field> fieldList;

        #region Constructors
        public BasicIsosResult_SqliteTable(string tableName)
        {
            this.Name = tableName;
            this.FieldList = new List<Field>();
            this.FieldList.Add(new Field("feature_id", "INTEGER Primary key"));
            this.FieldList.Add(new Field("scan_num", "INTEGER"));
            this.FieldList.Add(new Field("charge", "BYTE"));
            this.FieldList.Add(new Field("abundance", "UINT"));
            this.FieldList.Add(new Field("mz", "DOUBLE"));
            this.FieldList.Add(new Field("fit", "FLOAT"));
            this.FieldList.Add(new Field("average_mw", "DOUBLE"));
            this.FieldList.Add(new Field("monoisotopic_mw", "DOUBLE"));
            this.FieldList.Add(new Field("mostabundant_mw", "DOUBLE"));
            this.FieldList.Add(new Field("fwhm", "FLOAT"));
            this.FieldList.Add(new Field("signal_noise", "DOUBLE"));
            this.FieldList.Add(new Field("mono_abundance", "UINT"));
            this.FieldList.Add(new Field("mono_plus2_abundance", "UINT"));
 
        }
        #endregion

        #region Properties
        public override string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public override List<Field> FieldList
        {
            get
            {
                return fieldList;
            }
            set
            {
                fieldList = value;
            }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion


    }
}
