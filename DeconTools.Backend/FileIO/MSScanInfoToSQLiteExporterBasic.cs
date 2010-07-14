using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.FileIO
{
    public class MSScanInfoToSQLiteExporterBasic:SQLiteExporter<ScanResult>
    {
        const string m_TABLENAME = "T_MS_ScanSummary";
        List<Field> m_fieldList;


        #region Constructors
        public MSScanInfoToSQLiteExporterBasic(string fileName)
        {
            this.FileName = fileName;
            this.Name = this.ToString();

            InitializeAndBuildTable();
        }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override string TableName
        {
            get { return m_TABLENAME; }
        }

        public override List<DeconTools.Utilities.SqliteUtils.Field> FieldList
        {
            get
            {
                if (m_fieldList == null)
                {
                    m_fieldList = CreateFieldList();
                }

                return m_fieldList;
            }
        }

        protected override List<DeconTools.Utilities.SqliteUtils.Field> CreateFieldList()
        {
            List<Field> fieldList = new List<Field>();
            fieldList.Add(new Field("scan_num", "INTEGER Primary Key"));
            fieldList.Add(new Field("scan_time", "FLOAT"));
            fieldList.Add(new Field("type", "USHORT"));
            fieldList.Add(new Field("bpi", "FLOAT"));
            fieldList.Add(new Field("bpi_mz", "FLOAT"));
            fieldList.Add(new Field("tic", "FLOAT"));
            fieldList.Add(new Field("num_peaks", "UINT"));
            fieldList.Add(new Field("num_deisotoped", "UINT"));

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
