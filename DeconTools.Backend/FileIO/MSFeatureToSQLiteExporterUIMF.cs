using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.FileIO
{
    public class MSFeatureToSQLiteExporterUIMF : SQLiteExporter<IsosResult>
    {
        const string m_TABLENAME = "T_MSFeatures";
        List<Field> m_fieldList;


        #region Constructors
        public MSFeatureToSQLiteExporterUIMF(string fileName)
        {
            this.FileName = fileName;
            this.Name = this.ToString();

            InitializeAndBuildTable();
        }
        #endregion

        #region Properties
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
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
     

        protected override void AddResults(System.Data.Common.DbParameterCollection dbParameters, IsosResult result)
        {
            dbParameters[0].Value = result.MSFeatureID;
            dbParameters[1].Value = ((UIMFIsosResult)result).ScanSet.PrimaryScanNumber;
            dbParameters[2].Value = ((UIMFIsosResult)result).IMSScanSet.PrimaryScanNumber;
            dbParameters[3].Value = result.IsotopicProfile.ChargeState;
            dbParameters[4].Value = result.IsotopicProfile.GetAbundance();
            dbParameters[5].Value = result.IsotopicProfile.GetMZ();
            dbParameters[6].Value = result.IsotopicProfile.GetScore();
            dbParameters[7].Value = result.IsotopicProfile.AverageMass;
            dbParameters[8].Value = result.IsotopicProfile.MonoIsotopicMass;
            dbParameters[9].Value = result.IsotopicProfile.MostAbundantIsotopeMass;
            dbParameters[10].Value = result.IsotopicProfile.GetFWHM();
            dbParameters[11].Value = result.IsotopicProfile.GetSignalToNoise();
            dbParameters[12].Value = result.IsotopicProfile.GetMonoAbundance();
            dbParameters[13].Value = result.IsotopicProfile.GetMonoPlusTwoAbundance();
            dbParameters[14].Value = ((UIMFIsosResult)result).DriftTime;
            dbParameters[15].Value = result.IsotopicProfile.OriginalIntensity;
            dbParameters[16].Value = result.IsotopicProfile.IsSaturated ? 1 : 0;
            dbParameters[17].Value = DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags);
            dbParameters[18].Value = result.InterferenceScore;
        }

        protected override List<DeconTools.Utilities.SqliteUtils.Field> CreateFieldList()
        {
            var fieldList = new List<Field>();

            fieldList.Add(new Field("feature_id", "INTEGER Primary key"));
            fieldList.Add(new Field("frame_num", "INTEGER"));
            fieldList.Add(new Field("ims_scan_num", "INTEGER"));
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
            fieldList.Add(new Field("ims_drift_time", "FLOAT"));
            fieldList.Add(new Field("orig_intensity", "FLOAT"));
            fieldList.Add(new Field("TIA_orig_intensity", "FLOAT"));
            fieldList.Add(new Field("flag", "INTEGER"));
            fieldList.Add(new Field("interference_score", "float"));

            return fieldList;
        }
    }
}
