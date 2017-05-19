using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities.SqliteUtils;
using DeconTools.Utilities;
using System.Data.Common;

namespace DeconTools.Backend.FileIO
{
    public class MSFeatureToSQLiteExporterBasic:SQLiteExporter<IsosResult>
    {
        const string m_TABLENAME = "T_MSFeatures";
        List<Field> m_fieldList;


        #region Constructors
        public MSFeatureToSQLiteExporterBasic(string fileName)
        {
            this.Name = this.ToString();
            this.FileName = fileName;


            InitializeAndBuildTable();
        }

        #endregion

        #region Properties
        public override string TableName
        {
            get
            {
                return m_TABLENAME;
            }
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

        #endregion


        #region Private Methods
        protected override void AddResults(DbParameterCollection dbParameters, IsosResult result)
        {
            dbParameters[0].Value = result.MSFeatureID;
            dbParameters[1].Value = result.ScanSet.PrimaryScanNumber;
            dbParameters[2].Value = result.IsotopicProfile.ChargeState;
            dbParameters[3].Value = result.IsotopicProfile.GetAbundance();
            dbParameters[4].Value = result.IsotopicProfile.GetMZ();
            dbParameters[5].Value = result.IsotopicProfile.GetScore();
            dbParameters[6].Value = result.IsotopicProfile.AverageMass;
            dbParameters[7].Value = result.IsotopicProfile.MonoIsotopicMass;
            dbParameters[8].Value = result.IsotopicProfile.MostAbundantIsotopeMass;
            dbParameters[9].Value = result.IsotopicProfile.GetFWHM();
            dbParameters[10].Value = result.IsotopicProfile.GetSignalToNoise();
            dbParameters[11].Value = result.IsotopicProfile.GetMonoAbundance();
            dbParameters[12].Value = result.IsotopicProfile.GetMonoPlusTwoAbundance();
            dbParameters[13].Value = DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags);
        }
        
        
        protected override List<Field> CreateFieldList()
        {
            var fieldList = new List<Field>();
            fieldList.Add(new Field("feature_id", "INTEGER Primary key"));
            fieldList.Add(new Field("scan_num", "INTEGER"));
            fieldList.Add(new Field("charge", "BYTE"));
            fieldList.Add(new Field("abundance", "INTEGER"));
            fieldList.Add(new Field("mz", "DOUBLE"));
            fieldList.Add(new Field("fit", "FLOAT"));
            fieldList.Add(new Field("average_mw", "DOUBLE"));
            fieldList.Add(new Field("monoisotopic_mw", "DOUBLE"));
            fieldList.Add(new Field("mostabundant_mw", "DOUBLE"));
            fieldList.Add(new Field("fwhm", "FLOAT"));
            fieldList.Add(new Field("signal_noise", "DOUBLE"));
            fieldList.Add(new Field("mono_abundance", "INTEGER"));
            fieldList.Add(new Field("mono_plus2_abundance", "INTEGER"));
            fieldList.Add(new Field("flag", "INTEGER"));

            return fieldList;

        }
        #endregion

      
    }
}
