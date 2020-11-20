using System.Collections.Generic;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public sealed class BasicIsosResult_SqliteTable : Table
    {
        #region Constructors
        public BasicIsosResult_SqliteTable(string tableName)
        {
            Name = tableName;
            FieldList = new List<Field>
            {
                new Field("feature_id", "INTEGER Primary key"),
                new Field("scan_num", "INTEGER"),
                new Field("charge", "BYTE"),
                new Field("abundance", "INTEGER"),
                new Field("mz", "DOUBLE"),
                new Field("fit", "FLOAT"),
                new Field("average_mw", "DOUBLE"),
                new Field("monoisotopic_mw", "DOUBLE"),
                new Field("mostabundant_mw", "DOUBLE"),
                new Field("fwhm", "FLOAT"),
                new Field("signal_noise", "DOUBLE"),
                new Field("mono_abundance", "INTEGER"),
                new Field("mono_plus2_abundance", "INTEGER"),
                new Field("flag", "INTEGER")
            };
        }
        #endregion

        #region Properties
        public override string Name { get; set; }

        public override List<Field> FieldList { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
