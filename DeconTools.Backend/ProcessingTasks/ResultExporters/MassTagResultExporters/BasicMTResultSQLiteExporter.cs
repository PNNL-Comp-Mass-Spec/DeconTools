using System;
using System.Data.Common;
using System.IO;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities.SqliteUtils;
using DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters;
using System.Data.SQLite;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.MassTagResultExporters
{
    public class BasicMTResultSQLiteExporter : IMassTagResultExporter
    {
        protected DbConnection cnn;

        #region Constructors

        public BasicMTResultSQLiteExporter(string fileName)
            : this(fileName, 10000)
        {


        }

        public BasicMTResultSQLiteExporter(string fileName, int triggerValue)
        {
            if (File.Exists(fileName)) File.Delete(fileName);

            TriggerToExport = triggerValue;

            var fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            cnn = fact.CreateConnection();

            if (cnn == null)
                throw new Exception("Factory.CreateConnection returned a null DbConnection instance in BasicMTResultSQLiteExporter");

            cnn.ConnectionString = "Data Source=" + fileName;

            try
            {
                cnn.Open();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("SqlitePeakListExporter failed. Details: " + ex.Message, true);
                throw;
            }

            buildTables();



        }

        private void buildTables()
        {

            Table isosResultTable = new MassTagResult_SqliteTable("T_MassTagResults");
            var command = cnn.CreateCommand();

            command.CommandText = isosResultTable.BuildCreateTableString();
            command.ExecuteNonQuery();
        }


        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        protected override void addResults(Core.ResultCollection rc)
        {
            var myconnection = (SQLiteConnection)cnn;

            using (var mytransaction = myconnection.BeginTransaction())
            {
                using (var mycommand = new SQLiteCommand(myconnection))
                {
                    var featureIDParam = new SQLiteParameter();
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

                    var netParam = new SQLiteParameter();
                    var massTagIDParam = new SQLiteParameter();
                    var massTagMZParam = new SQLiteParameter();
                    var massTagNETParam = new SQLiteParameter();
                    var massTagSequenceParam = new SQLiteParameter();


                    mycommand.CommandText = @"INSERT INTO T_MassTagResults ([feature_id],[scan_num],[charge],[abundance],[mz],[fit],
[net],[mass_tag_id],[mass_tag_mz],[mass_tag_NET],[mass_tag_sequence],
[average_mw],[monoisotopic_mw],[mostabundant_mw],[fwhm],[signal_noise],[mono_abundance],[mono_plus2_abundance]) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                    mycommand.Parameters.Add(featureIDParam);
                    mycommand.Parameters.Add(scanNumParam);
                    mycommand.Parameters.Add(chargeParam);
                    mycommand.Parameters.Add(abundanceParam);
                    mycommand.Parameters.Add(mzParam);
                    mycommand.Parameters.Add(fitParam);

                    mycommand.Parameters.Add(netParam);
                    mycommand.Parameters.Add(massTagIDParam);
                    mycommand.Parameters.Add(massTagMZParam);
                    mycommand.Parameters.Add(massTagNETParam);
                    mycommand.Parameters.Add(massTagSequenceParam);


                    mycommand.Parameters.Add(averageMWParam);
                    mycommand.Parameters.Add(monoIsotopicMWParam);
                    mycommand.Parameters.Add(mostAbundantMWParam);
                    mycommand.Parameters.Add(fwhmParam);
                    mycommand.Parameters.Add(sigNoiseParam);
                    mycommand.Parameters.Add(monoAbundanceParam);

                    mycommand.Parameters.Add(monoPlus2AbundParam);

                    foreach (var item in rc.MassTagResultList)
                    {
                        featureIDParam.Value = item.Value.MSFeatureID;
                        fitParam.Value = item.Value.Score;

                        if (item.Value.ScanSet != null)
                        {
                            scanNumParam.Value = item.Value.ScanSet.PrimaryScanNumber;
                        }
                        netParam.Value = item.Value.GetNET();
                        massTagIDParam.Value = item.Value.Target.ID;
                        massTagMZParam.Value = item.Value.Target.MZ;
                        massTagNETParam.Value = item.Value.Target.NormalizedElutionTime;
                        massTagSequenceParam.Value = item.Value.Target.Code;

                        if (item.Value.IsotopicProfile != null)
                        {
                            chargeParam.Value = item.Value.IsotopicProfile.ChargeState;
                            abundanceParam.Value = item.Value.IsotopicProfile.GetAbundance();
                            mzParam.Value = item.Value.IsotopicProfile.GetMZ();


                            averageMWParam.Value = item.Value.IsotopicProfile.AverageMass;
                            monoIsotopicMWParam.Value = item.Value.IsotopicProfile.MonoIsotopicMass;
                            //mostAbundantMWParam.Value = item.Value.IsotopicProfile.getMostIntensePeak().XValue;
                            fwhmParam.Value = item.Value.IsotopicProfile.GetFWHM();
                            sigNoiseParam.Value = item.Value.IsotopicProfile.GetSignalToNoise();
                            monoAbundanceParam.Value = item.Value.IsotopicProfile.GetMonoAbundance();
                            monoPlus2AbundParam.Value = item.Value.IsotopicProfile.GetMonoPlusTwoAbundance();
                        }
                        mycommand.ExecuteNonQuery();
                    }


                }
                mytransaction.Commit();

            }

        }


        #endregion
        public override void ExportMassTagResults(Core.ResultCollection resultColl)
        {
            addResults(resultColl);
            resultColl.MassTagResultList.Clear();
        }

        public sealed override int TriggerToExport { get; set; }

        public override void Cleanup()
        {
            if (cnn != null)
            {
                try
                {
                    using (var tempConn = cnn )
                    {
                        if (tempConn.State != System.Data.ConnectionState.Closed)
                        {
                            tempConn.Close();
                        }
                    }

                }
                catch (Exception)
                {
                    // Ignore exceptions
                }
            }

            base.Cleanup();
        }
    }
}
