using System.Collections.Generic;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public sealed class UIMFScanResult_SqliteTable:Table
    {
        #region Constructors
        public UIMFScanResult_SqliteTable(string tableName)
        {
            Name = tableName;
            FieldList = new List<Field>
            {
                new Field("frame_num", "INTEGER Primary Key"),
                new Field("frame_time", "FLOAT"),
                new Field("type", "USHORT"),
                new Field("bpi", "FLOAT"),
                new Field("bpi_mz", "FLOAT"),
                new Field("tic", "FLOAT"),
                new Field("num_peaks", "UINT"),
                new Field("num_deisotoped", "UINT"),
                new Field("frame_pressure_unsmoothed", "FLOAT"),
                new Field("frame_pressure_smoothed", "FLOAT")
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
