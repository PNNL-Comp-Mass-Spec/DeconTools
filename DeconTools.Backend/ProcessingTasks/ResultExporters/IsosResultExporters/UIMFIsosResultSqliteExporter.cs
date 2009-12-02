using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities.SqliteUtils;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;

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
            Table isosResultTable = new UIMFIsosResult_SqliteTable("T_MSFeatures");
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

                    int n;

                    mycommand.CommandText = "INSERT INTO T_MSFeatures ([feature_id],[frame_num],[ims_scan_num],[charge],[abundance],[mz],[fit],[average_mw],[monoisotopic_mw],[mostabundant_mw],[fwhm],[signal_noise],[mono_abundance],[mono_plus2_abundance],[ims_drift_time],[orig_intensity],[TIA_orig_intensity]) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
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

                    for (n = 0; n < rc.ResultList.Count; n++)
                    {
                        featureIDParam.Value = rc.ResultList[n].MSFeatureID;
                        frameNumParam.Value = ((UIMFIsosResult)rc.ResultList[n]).FrameSet.PrimaryFrame;
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
                        driftTimeParam.Value = rc.ResultList[n].ScanSet.DriftTime;
                        origIntensParam.Value = rc.ResultList[n].IsotopicProfile.OriginalIntensity;
                        tia_origIntensParam.Value = rc.ResultList[n].IsotopicProfile.Original_Total_isotopic_abundance;
                        mycommand.ExecuteNonQuery();
                    }
                }
                mytransaction.Commit();

            }

        }
    }
}