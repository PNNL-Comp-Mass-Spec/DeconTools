using System.Collections.Generic;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public sealed class MassTagResult_SqliteTable : Table
    {
        #region Constructors
        public MassTagResult_SqliteTable(string tableName)
        {
            Name = tableName;
            FieldList = new List<Field>
            {
                new Field("feature_id", "INTEGER"),
                new Field("scan_num", "INTEGER"),
                new Field("charge", "BYTE"),
                new Field("abundance", "UINT"),
                new Field("mz", "DOUBLE"),
                new Field("fit", "FLOAT"),
                new Field("net", "FLOAT"),
                new Field("mass_tag_id", "INTEGER"),
                new Field("mass_tag_mz", "DOUBLE"),
                new Field("mass_tag_NET", "FLOAT"),
                new Field("mass_tag_sequence", "STRING"),
                new Field("average_mw", "DOUBLE"),
                new Field("monoisotopic_mw", "DOUBLE"),
                new Field("mostabundant_mw", "DOUBLE"),
                new Field("fwhm", "FLOAT"),
                new Field("signal_noise", "DOUBLE"),
                new Field("mono_abundance", "UINT"),
                new Field("mono_plus2_abundance", "UINT")
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
