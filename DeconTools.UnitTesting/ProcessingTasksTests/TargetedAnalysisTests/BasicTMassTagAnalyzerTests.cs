using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.Data;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend;
using System.Diagnostics;
using System.Linq;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class BasicTMassTagAnalyzerTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string xcaliburPeakDataFile = "..\\..\\TestFiles\\XCaliburPeakDataScans5500-6500.txt";

        private string xcaliburAllPeaksFile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_peaks.txt";

        private string massTagTestList1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_SMART_Probs.csv";

        [Test]
        public void getMS_and_find_one_massTag_test1()
        {
            MassTag mt = TestUtilities.GetMassTagStandard(1);

            Run run = new XCaliburRun(xcaliburTestfile);

            run.CurrentMassTag = mt;
            ScanSet scan = new ScanSet(run.GetClosestMSScan(5512, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST));    //see '5512' elsewhere for how I get this number. This points to an MS/MS scan. So must find the closest MS-level scan
            run.CurrentScanSet = scan;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            peakParams.PeakBackgroundRatio = 1.3;
            peakParams.SignalToNoiseThreshold = 2.0;
            peakParams.ThresholdedData = true;
            Task peakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();

            msgen.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);

            Assert.AreEqual(1, run.ResultCollection.MassTagResultList.Count);

            IMassTagResult result = run.ResultCollection.GetMassTagResult(mt);
            Assert.AreNotEqual(null, result);
            Assert.AreEqual(mt, result.MassTag);
            Assert.AreEqual(759.403435061313m, (decimal)result.IsotopicProfile.Peaklist[0].XValue);
            Assert.AreEqual(14191370m, (decimal)result.IsotopicProfile.Peaklist[0].Height);
            Assert.AreEqual(0.0163658764213324m, (decimal)result.IsotopicProfile.GetFWHM());


        }



        [Test]
        public void importAllPeaks_findOneMassTag_Test1()
        {
            MassTag mt = TestUtilities.GetMassTagStandard(1);


            Run run = new XCaliburRun(xcaliburTestfile);
            run.CurrentMassTag = mt;

            //MassTagCollection massTagColl = new MassTagCollection();
            //MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            //mtidImporter.Import(massTagColl);

            //Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(2, 2, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector();
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1);

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);

            TestUtilities.DisplayXYValues(run.ResultCollection);

            Console.WriteLine("Now generating MS....");
            msgen.Execute(run.ResultCollection);

            TestUtilities.DisplayXYValues(run.ResultCollection);


            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();
            Assert.AreEqual(5495, massTagResult.ScanSet.PrimaryScanNumber);




        }

        [Test]
        public void importAllPeaks_find100MassTags_Test1()
        {

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(2, 2, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector();
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1);

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();

            foreach (MassTag mt in massTagColl.MassTagList)
            {
                run.CurrentMassTag = mt;
                mt.ChargeState = 2;
                mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
                Console.WriteLine(" monoMZ = " + mt.MZ.ToString("0.000") + "; NET = " + mt.NETVal.ToString("0.00") + "; Sequence = " + mt.PeptideSequence + "\n");

                try
                {
                    peakChromGen.Execute(run.ResultCollection);
                    smoother.Execute(run.ResultCollection);
                    peakDet.Execute(run.ResultCollection);
                    chromPeakSel.Execute(run.ResultCollection);
                    msgen.Execute(run.ResultCollection);
                    mspeakDet.Execute(run.ResultCollection);
                    theorFeatureGen.Execute(run.ResultCollection);
                    targetedFeatureFinder.Execute(run.ResultCollection);
                    IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
                    massTagResult.DisplayToConsole();
                    Console.WriteLine("------------------------------ end --------------------------");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Task failed. Message: " + ex.Message + ex.StackTrace);
                }


            }


        }


        [Test]
        public void importAllPeaks_findOneMassTag_Test2()
        {
            int mtID = 138454;
            //int mtID = 202122;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(1, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine(" monoMZ = " + mt.MZ.ToString("0.000") + "; NET = " + mt.NETVal.ToString("0.00") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();

            Console.WriteLine("------------------------------ end --------------------------");





        }


        [Test]
        public void importAllPeaks_MT55376()     //this mass tag isn't being found.... 
        {
            int mtID = 55376;
            //int mtID = 202122;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(1, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine(" monoMZ = " + mt.MZ.ToString("0.000") + "; NET = " + mt.NETVal.ToString("0.00") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();

            Console.WriteLine("------------------------------ end --------------------------");





        }


        [Test]
        public void importAllPeaks_MT56475()     //this mass tag's most abundant peak is the second peak of its theor profile
        {
            int mtID = 56475;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(1, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine(" monoMZ = " + mt.MZ.ToString("0.000") + "; NET = " + mt.NETVal.ToString("0.00") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();

            Assert.AreEqual(1055.0111851272m, (decimal)massTagResult.IsotopicProfile.getMonoPeak().XValue);
            Assert.AreEqual(789730, massTagResult.IsotopicProfile.getMonoPeak().Height);


            Console.WriteLine("------------------------------ end --------------------------");





        }

        [Test]
        public void importAllPeaks_MT24917_ChromPeakSel_Selects_most_intense_peak()     //for this mass tag, it seems that the wrong chromatographic peak is selected and then the FeatureFinder fails to find the isotopic profile
        {
            int mtID = 24917;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(1, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine(" monoMZ = " + mt.MZ.ToString("0.000") + "; NET = " + mt.NETVal.ToString("0.00") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();

            //Assert.AreEqual(1055.0111851272m, (decimal)massTagResult.IsotopicProfile.getMonoPeak().XValue);
            //Assert.AreEqual(789730, massTagResult.IsotopicProfile.getMonoPeak().Height);


            Console.WriteLine("------------------------------ end --------------------------");





        }


        [Test]
        public void importAllPeaks_MT24917_ChromPeakSel_Selects_closest_To_MT_NET()     //for this mass tag, it seems that the wrong chromatographic peak is selected and then the FeatureFinder fails to find the isotopic profile
        {
            int mtID = 24917;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(1, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine(" monoMZ = " + mt.MZ.ToString("0.000") + "; NET = " + mt.NETVal.ToString("0.00") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();

            //Assert.AreEqual(1055.0111851272m, (decimal)massTagResult.IsotopicProfile.getMonoPeak().XValue);
            //Assert.AreEqual(789730, massTagResult.IsotopicProfile.getMonoPeak().Height);


            Console.WriteLine("------------------------------ end --------------------------");





        }

        [Test]
        public void importAllPeaks_find100MassTags_ChromPeakSel_Closest_To_Target()
        {

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder(0.01);

            int successCounter = 0;

            foreach (MassTag mt in massTagColl.MassTagList)
            {
                run.CurrentMassTag = mt;
                mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
                Console.WriteLine("monoMass = " + mt.MonoIsotopicMass.ToString("0.0000") + "; monoMZ = " + mt.MZ.ToString("0.0000") + "; ChargeState = " + mt.ChargeState + "; NET = " + mt.NETVal.ToString("0.000") + "; Sequence = " + mt.PeptideSequence + "\n");

                try
                {
                    peakChromGen.Execute(run.ResultCollection);
                    smoother.Execute(run.ResultCollection);
                    peakDet.Execute(run.ResultCollection);
                    chromPeakSel.Execute(run.ResultCollection);
                    msgen.Execute(run.ResultCollection);
                    mspeakDet.Execute(run.ResultCollection);
                    theorFeatureGen.Execute(run.ResultCollection);
                    targetedFeatureFinder.Execute(run.ResultCollection);
                    IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
                    massTagResult.DisplayToConsole();
                    if (massTagResult.IsotopicProfile != null) successCounter++;
                    Console.WriteLine("------------------------------ end --------------------------");
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Task failed. Message: " + ex.Message + ex.StackTrace);
                }


            }

            Console.WriteLine();
            Console.WriteLine("~~~~~~~~~~~~~ Number of Successes = " + successCounter + " ~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");


        }


        [Test]
        public void importAllPeaks_find100MassTags_timeCheck()
        {

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder(0.01);

            int successCounter = 0;
            List<long> timeData = new List<long>();


            foreach (MassTag mt in massTagColl.MassTagList)
            {
                run.CurrentMassTag = mt;
                mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    peakChromGen.Execute(run.ResultCollection);
                    smoother.Execute(run.ResultCollection);
                    peakDet.Execute(run.ResultCollection);
                    chromPeakSel.Execute(run.ResultCollection);
                    msgen.Execute(run.ResultCollection);
                    mspeakDet.Execute(run.ResultCollection);
                    theorFeatureGen.Execute(run.ResultCollection);
                    targetedFeatureFinder.Execute(run.ResultCollection);

                }
                catch (Exception)
                {

                }

                sw.Stop();
                timeData.Add(sw.ElapsedMilliseconds);
            }

            Console.WriteLine("Average time per mass tag = " + timeData.Average());     //290ms in Release mode



   


        }

        [Test]
        public void importAllPeaks_MT319923_no_MS()
        {
            int mtID = 319923;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(1, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine("monoMass = " + mt.MonoIsotopicMass.ToString("0.0000") + "; monoMZ = " + mt.MZ.ToString("0.0000") + "; ChargeState = " + mt.ChargeState + "; NET = " + mt.NETVal.ToString("0.000") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();


            Console.WriteLine("------------------------------ end --------------------------");





        }

        [Test]
        public void importAllPeaks_MT106915_no_IsotopicProfile()
        {
            int mtID = 106915;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(1, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder(0.01);



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine("monoMass = " + mt.MonoIsotopicMass.ToString("0.0000") + "; monoMZ = " + mt.MZ.ToString("0.0000") + "; ChargeState = " + mt.ChargeState + "; NET = " + mt.NETVal.ToString("0.000") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();


            Console.WriteLine("------------------------------ end --------------------------");





        }

        [Test]
        public void importAllPeaks_MT122790_no_IsotopicProfile()
        {
            int mtID = 122790;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder(0.01);



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine("monoMass = " + mt.MonoIsotopicMass.ToString("0.0000") + "; monoMZ = " + mt.MZ.ToString("0.0000") + "; ChargeState = " + mt.ChargeState + "; NET = " + mt.NETVal.ToString("0.000") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            //TestUtilities.DisplayXYValues(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();


            Console.WriteLine("------------------------------ end --------------------------");





        }


        [Test]
        public void importAllPeaks_MT283547_no_IsotopicProfile()
        {
            int mtID = 283547;

            Run run = new XCaliburRun(xcaliburTestfile);

            MassTagCollection massTagColl = new MassTagCollection();
            MassTagIDGenericImporter mtidImporter = new MassTagIDGenericImporter(massTagTestList1, ',');
            mtidImporter.Import(massTagColl);

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Shewanella_ProdTest_P352", "porky");
            importer.SetMassTagsToRetrieve(massTagColl.MassTagIDList);
            importer.Import(massTagColl);

            Assert.AreEqual(100, massTagColl.MassTagIDList.Count);

            Assert.AreEqual(85, massTagColl.MassTagList.Count);

            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburAllPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Task peakChromGen = new PeakChromatogramGenerator(20);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task zeroFill = new DeconTools.Backend.ProcessingTasks.ZeroFillers.DeconToolsZeroFiller(3);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 1);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder(0.01);



            run.CurrentMassTag = massTagColl.MassTagList.Find(p => p.ID == mtID);
            MassTag mt = run.CurrentMassTag;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------------- MassTag = " + mt.ID + "---------------------------");
            Console.WriteLine("monoMass = " + mt.MonoIsotopicMass.ToString("0.0000") + "; monoMZ = " + mt.MZ.ToString("0.0000") + "; ChargeState = " + mt.ChargeState + "; NET = " + mt.NETVal.ToString("0.000") + "; Sequence = " + mt.PeptideSequence + "\n");


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            TestUtilities.DisplayPeaks(run.ResultCollection);
            chromPeakSel.Execute(run.ResultCollection);
            msgen.Execute(run.ResultCollection);
            //TestUtilities.DisplayXYValues(run.ResultCollection);
            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            IMassTagResult massTagResult = run.ResultCollection.MassTagResultList[mt];
            massTagResult.DisplayToConsole();


            Console.WriteLine("------------------------------ end --------------------------");





        }


    }
}
