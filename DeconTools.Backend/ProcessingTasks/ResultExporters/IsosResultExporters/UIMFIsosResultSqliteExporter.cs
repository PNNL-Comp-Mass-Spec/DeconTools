using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class UIMFIsosResultSqliteExporter : IsosResultSqliteExporter
    {


        #region Constructors
        public UIMFIsosResultSqliteExporter(string fileName)
            : this(fileName, 10000)
        {

        }

        public UIMFIsosResultSqliteExporter(string fileName, int triggerValue)
        {
            if (File.Exists(fileName)) File.Delete(fileName);


            this.TriggerToExport = triggerValue;


            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            this.cnn = fact.CreateConnection();
            cnn.ConnectionString = "Data Source=" + fileName;

            try
            {
                cnn.Open();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SqlitePeakListExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw;
            }

            buildTables();
        }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        protected override void buildTables()
        {
            Table isosResultTable = new UIMFIsosResult_SqliteTable("T_MSFeatures");
            DbCommand command = cnn.CreateCommand();

            command.CommandText = isosResultTable.BuildCreateTableString();
            command.ExecuteNonQuery();
        }

        protected override void addIsosResults(List<IsosResult> isosResultList)
        {
            SQLiteConnection myconnection = (SQLiteConnection)cnn;

            using (SQLiteTransaction mytransaction = myconnection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(myconnection))
                {
                    SQLiteParameter featureIDParam = new SQLiteParameter();
                    SQLiteParameter frameNumParam = new SQLiteParameter();
                    SQLiteParameter scanNumParam = new SQLiteParameter();
                    SQLiteParameter chargeParam = new SQLiteParameter();
                    SQLiteParameter abundanceParam = new SQLiteParameter();
                    SQLiteParameter mzParam = new SQLiteParameter();
                    SQLiteParameter fitParam = new SQLiteParameter();
                    SQLiteParameter averageMWParam = new SQLiteParameter();
                    SQLiteParameter monoIsotopicMWParam = new SQLiteParameter();
                    SQLiteParameter mostAbundantMWParam = new SQLiteParameter();
                    SQLiteParameter fwhmParam = new SQLiteParameter();
                    SQLiteParameter sigNoiseParam = new SQLiteParameter();
                    SQLiteParameter monoAbundanceParam = new SQLiteParameter();
                    SQLiteParameter monoPlus2AbundParam = new SQLiteParameter();
                    SQLiteParameter driftTimeParam = new SQLiteParameter();
                    SQLiteParameter origIntensParam = new SQLiteParameter();
                    SQLiteParameter tia_origIntensParam = new SQLiteParameter();
                    SQLiteParameter flagCodeParam = new SQLiteParameter();


                    int n;

                    mycommand.CommandText = "INSERT INTO T_MSFeatures ([feature_id],[frame_num],[ims_scan_num],[charge],[abundance],[mz],[fit],[average_mw],[monoisotopic_mw],[mostabundant_mw],[fwhm],[signal_noise],[mono_abundance],[mono_plus2_abundance],[ims_drift_time],[orig_intensity],[TIA_orig_intensity],[flag]) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                    mycommand.Parameters.Add(featureIDParam);
                    mycommand.Parameters.Add(frameNumParam);
                    mycommand.Parameters.Add(scanNumParam);
                    mycommand.Parameters.Add(chargeParam);
                    mycommand.Parameters.Add(abundanceParam);
                    mycommand.Parameters.Add(mzParam);
                    mycommand.Parameters.Add(fitParam);
                    mycommand.Parameters.Add(averageMWParam);
                    mycommand.Parameters.Add(monoIsotopicMWParam);
                    mycommand.Parameters.Add(mostAbundantMWParam);
                    mycommand.Parameters.Add(fwhmParam);
                    mycommand.Parameters.Add(sigNoiseParam);
                    mycommand.Parameters.Add(monoAbundanceParam);
                    mycommand.Parameters.Add(monoPlus2AbundParam);
                    mycommand.Parameters.Add(driftTimeParam);
                    mycommand.Parameters.Add(origIntensParam);
                    mycommand.Parameters.Add(tia_origIntensParam);
                    mycommand.Parameters.Add(flagCodeParam);

                    for (n = 0; n < isosResultList.Count; n++)
                    {
                        featureIDParam.Value = isosResultList[n].MSFeatureID;
                        frameNumParam.Value = (isosResultList[n].ScanSet.PrimaryScanNumber);
                        scanNumParam.Value = ((UIMFIsosResult)isosResultList[n]).IMSScanSet.PrimaryScanNumber;
                        chargeParam.Value = isosResultList[n].IsotopicProfile.ChargeState;
                        abundanceParam.Value = isosResultList[n].IsotopicProfile.GetAbundance();
                        mzParam.Value = isosResultList[n].IsotopicProfile.GetMZ();
                        fitParam.Value = isosResultList[n].IsotopicProfile.GetScore();
                        averageMWParam.Value = isosResultList[n].IsotopicProfile.AverageMass;
                        monoIsotopicMWParam.Value = isosResultList[n].IsotopicProfile.MonoIsotopicMass;
                        mostAbundantMWParam.Value = isosResultList[n].IsotopicProfile.MostAbundantIsotopeMass;
                        fwhmParam.Value = isosResultList[n].IsotopicProfile.GetFWHM();
                        sigNoiseParam.Value = isosResultList[n].IsotopicProfile.GetSignalToNoise();
                        monoAbundanceParam.Value = isosResultList[n].IsotopicProfile.GetMonoAbundance();
                        monoPlus2AbundParam.Value = isosResultList[n].IsotopicProfile.GetMonoPlusTwoAbundance();
                        driftTimeParam.Value = ((UIMFIsosResult)isosResultList[n]).DriftTime;
                        origIntensParam.Value = isosResultList[n].IsotopicProfile.OriginalIntensity;
                        //tia_origIntensParam.Value = isosResultList[n].IsotopicProfile.OriginalTotalIsotopicAbundance;
                        flagCodeParam.Value = ResultValidators.ResultValidationUtils.GetStringFlagCode(isosResultList[n].Flags);
                        mycommand.ExecuteNonQuery();
                    }
                }
                mytransaction.Commit();

            }

        }
    }
}