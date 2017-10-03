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
            Name = tableName;
            FieldList = new List<Field>();
            FieldList.Add(new Field("feature_id", "INTEGER Primary key"));
            FieldList.Add(new Field("scan_num", "INTEGER"));
            FieldList.Add(new Field("charge", "BYTE"));
            FieldList.Add(new Field("abundance", "INTEGER"));
            FieldList.Add(new Field("mz", "DOUBLE"));
            FieldList.Add(new Field("fit", "FLOAT"));
            FieldList.Add(new Field("average_mw", "DOUBLE"));
            FieldList.Add(new Field("monoisotopic_mw", "DOUBLE"));
            FieldList.Add(new Field("mostabundant_mw", "DOUBLE"));
            FieldList.Add(new Field("fwhm", "FLOAT"));
            FieldList.Add(new Field("signal_noise", "DOUBLE"));
            FieldList.Add(new Field("mono_abundance", "INTEGER"));
            FieldList.Add(new Field("mono_plus2_abundance", "INTEGER"));
            FieldList.Add(new Field("flag", "INTEGER"));
 
        }
        #endregion

        #region Properties
        public override string Name
        {
            get => name;
            set => name = value;
        }
        public override List<Field> FieldList
        {
            get => fieldList;
            set => fieldList = value;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion


    }
}
