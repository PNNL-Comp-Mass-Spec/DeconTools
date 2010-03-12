using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities.SqliteUtils;
using System.Data.SQLite;
using System.IO;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class BasicIsosResultSqliteExporter : IsosResultSqliteExporter
    {
        #region Constructors
        public BasicIsosResultSqliteExporter(string fileName)
            : this(fileName, 100000)
        {

        }

        public BasicIsosResultSqliteExporter(string fileName, int triggerValue)
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
                throw ex;
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
            Table isosResultTable = new BasicIsosResult_SqliteTable("T_MSFeatures");
            DbCommand command = cnn.CreateCommand();

            command.CommandText = isosResultTable.BuildCreateTableString();
            command.ExecuteNonQuery();
        }

        protected override void addIsosResults(DeconTools.Backend.Core.ResultCollection rc)
        {
            SQLiteConnection myconnection = (SQLiteConnection)cnn;

            using (SQLiteTransaction mytransaction = myconnection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(myconnection))
                {
                    SQLiteParameter featureIDParam = new SQLiteParameter();
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
                    SQLiteParameter flagCodeParam = new SQLiteParameter();


                    int n;

                    mycommand.CommandText = "INSERT INTO T_MSFeatures ([feature_id],[scan_num],[charge],[abundance],[mz],[fit],[average_mw],[monoisotopic_mw],[mostabundant_mw],[fwhm],[signal_noise],[mono_abundance],[mono_plus2_abundance],[flag]) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                    mycommand.Parameters.Add(featureIDParam);
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
                    mycommand.Parameters.Add(flagCodeParam);

                    for (n = 0; n < rc.ResultList.Count; n++)
                    {
                        featureIDParam.Value = rc.ResultList[n].MSFeatureID;
                        scanNumParam.Value = rc.ResultList[n].ScanSet.PrimaryScanNumber;
                        chargeParam.Value = rc.ResultList[n].IsotopicProfile.ChargeState;
                        abundanceParam.Value = rc.ResultList[n].IsotopicProfile.GetAbundance();
                        mzParam.Value = rc.ResultList[n].IsotopicProfile.GetMZ();
                        fitParam.Value = rc.ResultList[n].IsotopicProfile.GetScore();
                        averageMWParam.Value = rc.ResultList[n].IsotopicProfile.AverageMass;
                        monoIsotopicMWParam.Value = rc.ResultList[n].IsotopicProfile.MonoIsotopicMass;
                        mostAbundantMWParam.Value = rc.ResultList[n].IsotopicProfile.MostAbundantIsotopeMass;
                        fwhmParam.Value = rc.ResultList[n].IsotopicProfile.GetFWHM();
                        sigNoiseParam.Value = rc.ResultList[n].IsotopicProfile.GetSignalToNoise();
                        monoAbundanceParam.Value = rc.ResultList[n].IsotopicProfile.GetMonoAbundance();
                        monoPlus2AbundParam.Value = rc.ResultList[n].IsotopicProfile.GetMonoPlusTwoAbundance();
                        flagCodeParam.Value = ResultValidators.ResultValidationUtils.GetStringFlagCode(rc.ResultList[n].Flags);
                        mycommand.ExecuteNonQuery();
                    }
                }
                mytransaction.Commit();

            }

        }

    }
}
