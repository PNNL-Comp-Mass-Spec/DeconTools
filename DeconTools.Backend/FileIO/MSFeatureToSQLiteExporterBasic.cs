using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities.SqliteUtils;
using System.Data.Common;

namespace DeconTools.Backend.FileIO
{
    public sealed class MSFeatureToSQLiteExporterBasic : SQLiteExporter<IsosResult>
    {
        const string TABLE_NAME = "T_MSFeatures";
        List<Field> m_fieldList;

        #region Constructors
        public MSFeatureToSQLiteExporterBasic(string fileName)
        {
            Name = this.ToString();
            FileName = fileName;

            InitializeAndBuildTable();
        }

        #endregion

        #region Properties
        public override string TableName => TABLE_NAME;

        public override List<Field> FieldList => m_fieldList ?? (m_fieldList = CreateFieldList());

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
            dbParameters[13].Value = ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags);
        }

        protected override List<Field> CreateFieldList()
        {
            var fieldList = new List<Field>
            {
                new Field("feature_id", "INTEGER Primary key"),
                new Field("scan_num", "INTEGER"),
                new Field("charge", "BYTE"),
                new Field("abundance", "INTEGER"),
                new Field("mz", "DOUBLE"),
                new Field("fit", "FLOAT"),
                new Field("average_mw", "DOUBLE"),
                new Field("monoisotopic_mw", "DOUBLE"),
                new Field("mostabundant_mw", "DOUBLE"),
                new Field("fwhm", "FLOAT"),
                new Field("signal_noise", "DOUBLE"),
                new Field("mono_abundance", "INTEGER"),
                new Field("mono_plus2_abundance", "INTEGER"),
                new Field("flag", "INTEGER")
            };

            return fieldList;
        }
        #endregion

    }
}
