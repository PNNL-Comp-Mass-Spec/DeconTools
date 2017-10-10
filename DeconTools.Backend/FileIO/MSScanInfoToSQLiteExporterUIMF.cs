using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.FileIO
{
    public sealed class MSScanInfoToSQLiteExporterUIMF : SQLiteExporter<ScanResult>
    {
        const string m_TABLENAME = "T_MS_ScanSummary";
        List<Field> m_fieldList;

        #region Constructors
        public MSScanInfoToSQLiteExporterUIMF(string fileName)
        {
            FileName = fileName;
            Name = this.ToString();

            InitializeAndBuildTable();
        }
        #endregion

        #region Properties
        public override string TableName => m_TABLENAME;

        public override List<Field> FieldList => m_fieldList ?? (m_fieldList = CreateFieldList());

        #endregion

        #region Private Methods
        protected override List<Field> CreateFieldList()
        {
            var fieldList = new List<Field>
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


            return fieldList;
        }

        protected override void AddResults(System.Data.Common.DbParameterCollection dbParameters, ScanResult result)
        {
            dbParameters[0].Value = ((UimfScanResult)result).ScanSet.PrimaryScanNumber;
            dbParameters[1].Value = ((UimfScanResult)result).ScanTime;
            dbParameters[2].Value = result.SpectrumType;
            dbParameters[3].Value = result.BasePeak.Height;
            dbParameters[4].Value = result.BasePeak.XValue;
            dbParameters[5].Value = result.TICValue;
            dbParameters[6].Value = result.NumPeaks;
            dbParameters[7].Value = result.NumIsotopicProfiles;
            dbParameters[8].Value = ((UimfScanResult)result).FramePressureUnsmoothed;
            dbParameters[9].Value = ((UimfScanResult)result).FramePressureSmoothed;
        }
        #endregion



    }
}
