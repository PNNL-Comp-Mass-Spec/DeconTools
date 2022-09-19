using System.Collections.Generic;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public sealed class BasicScanResult_SqliteTable : Table
    {
        #region Constructors

        public BasicScanResult_SqliteTable(string tableName)
        {
            Name = tableName;
            FieldList = new List<Field>
            {
                new Field("scan_num", "INTEGER Primary Key"),
                new Field("scan_time", "FLOAT"),
                new Field("type", "USHORT"),
                new Field("bpi", "FLOAT"),
                new Field("bpi_mz", "FLOAT"),
                new Field("tic", "FLOAT"),
                new Field("num_peaks", "UINT"),
                new Field("num_deisotoped", "UINT")
            };
        }

        #endregion

        #region Properties
        public override string Name { get; set; }

        public override List<Field> FieldList { get; set; }

        #endregion

    }
}
