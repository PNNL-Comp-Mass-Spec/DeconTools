﻿using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.FileIO
{
    public sealed class MSFeatureToSQLiteExporterUIMF : SQLiteExporter<IsosResult>
    {
        const string TABLE_NAME = "T_MSFeatures";
        List<Field> m_fieldList;

        #region Constructors
        public MSFeatureToSQLiteExporterUIMF(string fileName)
        {
            FileName = fileName;
            Name = this.ToString();

            InitializeAndBuildTable();
        }
        #endregion

        #region Properties
        public override string TableName => TABLE_NAME;

        public override List<Field> FieldList => m_fieldList ?? (m_fieldList = CreateFieldList());

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
            dbParameters[17].Value = ProcessingTasks.ResultValidators.ResultValidationUtils.GetStringFlagCode(result.Flags);
            dbParameters[18].Value = result.InterferenceScore;
        }

        protected override List<Field> CreateFieldList()
        {
            var fieldList = new List<Field>
            {
                new Field("feature_id", "INTEGER Primary key"),
                new Field("frame_num", "INTEGER"),
                new Field("ims_scan_num", "INTEGER"),
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
                new Field("ims_drift_time", "FLOAT"),
                new Field("orig_intensity", "FLOAT"),
                new Field("TIA_orig_intensity", "FLOAT"),
                new Field("flag", "INTEGER"),
                new Field("interference_score", "float")
            };

            return fieldList;
        }
    }
}
