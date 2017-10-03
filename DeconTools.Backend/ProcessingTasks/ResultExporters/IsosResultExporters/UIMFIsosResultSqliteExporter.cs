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
    public sealed class UIMFIsosResultSqliteExporter : IsosResultSqliteExporter
    {


        #region Constructors
        public UIMFIsosResultSqliteExporter(string fileName)
            : this(fileName, 10000)
        {

        }

        public UIMFIsosResultSqliteExporter(string fileName, int triggerValue)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (File.Exists(fileName)) File.Delete(fileName);

            TriggerToExport = triggerValue;


            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            cnn = fact.CreateConnection();

            if (cnn == null)
                throw new Exception("Factory.CreateConnection returned a null DbConnection object in UIMFIsosResultSqliteExporter");

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
            var command = cnn.CreateCommand();

            command.CommandText = isosResultTable.BuildCreateTableString();
            command.ExecuteNonQuery();
        }

        protected override void addIsosResults(List<IsosResult> isosResultList)
        {
            var myconnection = (SQLiteConnection)cnn;

            using (var mytransaction = myconnection.BeginTransaction())
            {
                using (var mycommand = new SQLiteCommand(myconnection))
                {
                    var featureIDParam = new SQLiteParameter();
                    var frameNumParam = new SQLiteParameter();
                    var scanNumParam = new SQLiteParameter();
                    var chargeParam = new SQLiteParameter();
                    var abundanceParam = new SQLiteParameter();
                    var mzParam = new SQLiteParameter();
                    var fitParam = new SQLiteParameter();
                    var averageMWParam = new SQLiteParameter();
                    var monoIsotopicMWParam = new SQLiteParameter();
                    var mostAbundantMWParam = new SQLiteParameter();
                    var fwhmParam = new SQLiteParameter();
                    var sigNoiseParam = new SQLiteParameter();
                    var monoAbundanceParam = new SQLiteParameter();
                    var monoPlus2AbundParam = new SQLiteParameter();
                    var driftTimeParam = new SQLiteParameter();
                    var origIntensParam = new SQLiteParameter();
                    var tia_origIntensParam = new SQLiteParameter();
                    var flagCodeParam = new SQLiteParameter();


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