using System.Collections.Generic;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class UIMFScanResult_SqliteTable:Table
    {
        private string name;
        private List<Field> fieldList;

        #region Constructors
        public UIMFScanResult_SqliteTable(string tableName)
        {
            this.Name = tableName;
            this.FieldList = new List<Field>();
            this.FieldList.Add(new Field("frame_num", "INTEGER Primary Key"));
            this.FieldList.Add(new Field("frame_time", "FLOAT"));
            this.FieldList.Add(new Field("type", "USHORT"));
            this.FieldList.Add(new Field("bpi", "FLOAT"));
            this.FieldList.Add(new Field("bpi_mz", "FLOAT"));
            this.FieldList.Add(new Field("tic", "FLOAT"));
            this.FieldList.Add(new Field("num_peaks", "UINT"));
            this.FieldList.Add(new Field("num_deisotoped", "UINT"));
            this.FieldList.Add(new Field("frame_pressure_unsmoothed", "FLOAT"));
            this.FieldList.Add(new Field("frame_pressure_smoothed", "FLOAT"));

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
