using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;
using System.ComponentModel;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class OldSchoolProcRunnerTests
    {
        string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.uimf";
        string uimfFilepath2 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        string uimfFile3 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
        string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
        string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        string uimfParameterFile1 = "..\\..\\TestFiles\\oldSchoolProcRunnerParameterTest1File.xml";
        string uimfParameterFile2 = "..\\..\\TestFiles\\oldSchoolProcRunnerParameterTestFile2.xml";
        string uimfParameterFile3 = "..\\..\\TestFiles\\oldSchoolProcRunnerParameterTestFile3.xml";
        string uimfParameterHorn1 = "..\\..\\TestFiles\\uimfParameterFileHorn1.xml";
        string uimfParameterHorn2 = "..\\..\\TestFiles\\uimfParameterFileHorn2.xml";


        public string parameterFile4 = "..\\..\\TestFiles\\oldSchoolProcRunnerParameterTestFile4.xml";
        public string parameterFile5 = "..\\..\\TestFiles\\oldSchoolProcRunnerParameterTestFile5_processMSMS.xml";

        public string xcaliburParameterFile1 = "..\\..\\TestFiles\\xcaliburParameterFile1.xml";
        public string xcaliburParameterFile3 = "..\\..\\TestFiles\\xcaliburParameterFile3.xml";
        private string xcaliburParameterFile4_exporttoSqlite = "..\\..\\TestFiles\\xcaliburParameterFile4_exportToSqlite.xml";
        private string xcal_sum5_adv1_scans6000_6050 = "..\\..\\TestFiles\\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Sum5_Advance1_scans6000-6050.xml";


        public string imfParameterFile1 = "..\\..\\TestFiles\\imfParameterFile1.xml";
        public string imfParameterFile2 = "..\\..\\TestFiles\\imfParameterFile2.xml";
        private string imfParameterFile3 = "..\\..\\TestFiles\\oldSchoolProcRunnerParameterTestFile5_IMFTesting.xml";
        private string imfParameterFileHorn1 = "..\\..\\TestFiles\\imfParameterFile_horn1.xml";
        
        string replaceRapidScoreParamFile1 = "..\\..\\TestFiles\\replaceRAPIDScoreParameterFile1.xml";

        string mzxmlFilepath = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_Scans6000-7000.mzXML";
        string mzxmlParameterFile1 = "..\\..\\TestFiles\\mxzml_parameterFile1.xml";

        string agilentFile1 = @"F:\Gord\Data\AgilentD\BSA_TOF4.d";
        string agilentParams1 = "..\\..\\TestFiles\\ParameterFiles\\agilentParams_scans25-27.xml";


        [Test]
        public void loadParametersTest1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFilepath, Globals.MSFileType.PNNL_UIMF, uimfParameterFile1);

            Assert.AreEqual(3, runner.Project.Parameters.NumFramesSummed);
            Assert.AreEqual(1, runner.Project.Parameters.NumScansSummed);
            Assert.AreEqual(true, runner.Project.Parameters.OldDecon2LSParameters.HornTransformParameters.UseRAPIDDeconvolution);


        }

        [Test]
        public void checkRunsAndTasksTest1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFilepath, Globals.MSFileType.PNNL_UIMF, uimfParameterFile1);
            Assert.AreEqual(6, runner.Project.TaskCollection.TaskList.Count);
            Assert.IsInstanceOfType(typeof(UIMFRun), runner.Project.RunCollection[0]);
            Assert.IsInstanceOfType(typeof(RapidDeconvolutor), runner.Project.TaskCollection.TaskList[2]);
            Assert.IsInstanceOfType(typeof(UIMF_MSGenerator), runner.Project.TaskCollection.TaskList[0]);

        }

        [Test]
        public void ExecuteRunnerOnUIMFDataTest1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFilepath2, Globals.MSFileType.PNNL_UIMF, uimfParameterFile2);
            runner.Execute();

        }

        [Test]
        public void ExecuteRunnerOnUIMFDataTest2()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFile3, Globals.MSFileType.PNNL_UIMF, uimfParameterFile2);
            runner.Execute();
        }

        [Test]
        public void ExecuteRunnerOnUIMFDataWithHornDeconTest1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFilepath2, Globals.MSFileType.PNNL_UIMF, uimfParameterHorn1);
            runner.Execute();

            //Assert.AreEqual(265, runner.Project.RunCollection[0].ResultCollection.ResultList.Count);
            //Assert.AreEqual(1, runner.Project.RunCollection[0].ResultCollection.ScanResultList.Count);

            //Assert.AreEqual(4916859, runner.Project.RunCollection[0].ResultCollection.ScanResultList[0].TICValue);


        }

        [Test]
        public void ExecuteRunnerOnXCaliburDataTest1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(xcaliburTestfile, Globals.MSFileType.Finnigan, xcaliburParameterFile1);
            runner.Execute();

            //Assert.AreEqual(12607, runner.Project.RunCollection[0].ResultCollection.ResultList.Count);

        }

        [Test]
        public void ExecuteRunnerOnIMFFileTest1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(imfFilepath, Globals.MSFileType.PNNL_IMS, imfParameterFile1);
            Assert.AreEqual(5, runner.Project.Parameters.NumScansSummed);
            Assert.AreEqual(false, runner.Project.Parameters.OldDecon2LSParameters.HornTransformParameters.UseScanRange);
            runner.Execute();

            Assert.AreEqual(3972, runner.Project.RunCollection[0].ResultCollection.ResultList.Count);

        }


        [Test]
        public void ExecuteRunnerOnIMFFileTest2()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(imfFilepath, Globals.MSFileType.PNNL_IMS, imfParameterFile2);
            Assert.AreEqual(5, runner.Project.Parameters.NumScansSummed);
            Assert.AreEqual(true, runner.Project.Parameters.OldDecon2LSParameters.HornTransformParameters.UseScanRange);
            runner.Execute();

            Assert.AreEqual(5, runner.Project.RunCollection[0].ResultCollection.ResultList.Count);

        }

        [Test]
        public void ExecuteRunnerAndSerializeResultsOnUIMF()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFilepath2, Globals.MSFileType.PNNL_UIMF, uimfParameterFile3);
            runner.IsosResultThreshold = 100;
            runner.Execute();
        }


        [Test]
        public void ExecuteRunnerAndSerializeResultsOnUIMF2()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFilepath2, Globals.MSFileType.PNNL_UIMF, uimfParameterFile3, bw);
            runner.IsosResultThreshold = 1000000;
            runner.Execute();
        }



        [Test]
        public void getAssemblyInfo()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(typeof(Task));
            Console.WriteLine(assembly.ToString());
        }


        [Test]
        public void ExecuteRunnerAndWithPeakExporterTest1()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            OldSchoolProcRunner runner = new OldSchoolProcRunner(xcaliburTestfile, Globals.MSFileType.Finnigan, parameterFile4, bw);
            runner.IsosResultThreshold = 100000;
            runner.Execute();
        }


        [Test]
        public void ExecuteRunnerAndWithPeakExporterTest2()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            OldSchoolProcRunner runner = new OldSchoolProcRunner(imfFilepath, Globals.MSFileType.PNNL_IMS, imfParameterFile3, bw);
            runner.IsosResultThreshold = 100000;
            runner.Execute();
        }


        [Test]
        public void ExecuteRunnerAndWithPeakExporter_ExportMSMSTest1()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            OldSchoolProcRunner runner = new OldSchoolProcRunner(xcaliburTestfile, Globals.MSFileType.Finnigan, parameterFile5);
            runner.IsosResultThreshold = 1000;
            runner.Execute();
        }


        [Test]
        public void ExecuteRunner_withReplaceRapidScoresTest1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(xcaliburTestfile, Globals.MSFileType.Finnigan, replaceRapidScoreParamFile1);
            runner.Execute();

        }



        [Test]
        public void ExecuteRunner_horn_xcaliburTest()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(xcaliburTestfile, Globals.MSFileType.Finnigan, xcaliburParameterFile3);
            runner.Execute();
        }

        [Test]
        public void ExecuteRunner_horn_exportToSqlite()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(xcaliburTestfile, Globals.MSFileType.Finnigan, xcaliburParameterFile4_exporttoSqlite);
            runner.Execute();
        }



        [Test]
        public void ExecuteRunner_mzXML_horn_Test1()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            OldSchoolProcRunner runner = new OldSchoolProcRunner(mzxmlFilepath, Globals.MSFileType.MZXML_Rawdata, mzxmlParameterFile1, bw);
            runner.Execute();
        }

        [Test]
        public void xcaliburSum5_adv1_horn_test1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(xcaliburTestfile, Globals.MSFileType.Finnigan, xcal_sum5_adv1_scans6000_6050);
            runner.Execute();

        }

        [Test]
        public void agilent_horn_Test1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(agilentFile1, Globals.MSFileType.Agilent_TOF, agilentParams1);
            runner.Execute();

        }



        //Dec 2, 2009:  Deserializer not used anymore.  Test is decommissioned...
        //[Test]
        //public void ExecuteRunner_exportOriginalIntensityDataTest2_usingDeserializer()
        //{
        //    OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFilepath2, Globals.MSFileType.PNNL_UIMF, uimfParameterHorn2);
        //    runner.ExporterType = Globals.ExporterType.TEXT;
        //    runner.IsosResultThreshold = 100;
        //    runner.Execute();
        //}



    }
}
