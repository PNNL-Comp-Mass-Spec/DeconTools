using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.FileIO
{
    public sealed class MSScanInfoToSQLiteExporterBasic : SQLiteExporter<ScanResult>
    {
        const string TABLE_NAME = "T_MS_ScanSummary";
        List<Field> m_fieldList;


        #region Constructors
        public MSScanInfoToSQLiteExporterBasic(string fileName)
        {
            FileName = fileName;
            Name = this.ToString();

            InitializeAndBuildTable();
        }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override string TableName => TABLE_NAME;

        public override List<Field> FieldList => m_fieldList ?? (m_fieldList = CreateFieldList());

        protected override List<Field> CreateFieldList()
        {
            var fieldList = new List<Field>
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

            return fieldList;
        }

        protected override void AddResults(System.Data.Common.DbParameterCollection dbParameters, ScanResult result)
        {
            dbParameters[0].Value = result.ScanSet.PrimaryScanNumber;
            dbParameters[1].Value = result.ScanTime;
            dbParameters[2].Value = result.SpectrumType;
            dbParameters[3].Value = result.BasePeak.Height;
            dbParameters[4].Value = result.BasePeak.XValue;
            dbParameters[5].Value = result.TICValue;
            dbParameters[6].Value = result.NumPeaks;
            dbParameters[7].Value = result.NumIsotopicProfiles;
        }
    }
}
