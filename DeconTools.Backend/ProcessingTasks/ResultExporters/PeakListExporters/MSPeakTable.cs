using System.Collections.Generic;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public sealed class MSPeakTable : Table
    {
        #region Constructors
        public MSPeakTable(string tableName)
        {
            Name = tableName;
            FieldList = new List<Field>
            {
                new Field("peak_id", "INTEGER Primary key"),
                new Field("scan_num", "INTEGER"),
                new Field("mz", "FLOAT"),
                new Field("intensity", "FLOAT"),
                new Field("fwhm", "FLOAT"),
                new Field("msfeatureID", "FLOAT")
            };
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
