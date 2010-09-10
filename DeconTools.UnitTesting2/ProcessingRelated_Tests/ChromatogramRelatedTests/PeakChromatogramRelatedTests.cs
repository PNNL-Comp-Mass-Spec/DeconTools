using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Algorithms;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ChromatogramRelatedTests
{
    [TestFixture]
    public class PeakChromatogramRelatedTests
    {
        string xcaliburPeakDataFile = "..\\..\\..\\TestFiles\\Chromagram_related\\XCaliburPeakDataScans5500-6500.txt";





        [Test]
        public void getPeakChromatogramTest1()
        {
            MassTag mt = TestUtilities.GetMassTagStandard(1);

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.CurrentMassTag = mt;

            TomTheorFeatureGenerator unlabelledTheorGenerator = new TomTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);

            Task peakChromGen = new PeakChromatogramGenerator(10);
            peakChromGen.Execute(run.ResultCollection);

            Assert.AreEqual(133, run.XYData.Xvalues.Length);
            Assert.AreEqual(5543, (int)run.XYData.Xvalues[35]);
            Assert.AreEqual(7319569, (int)run.XYData.Yvalues[35]);
            run.XYData.Display();
        }


        [Test]
        public void getPeakChromatogramTest2()
        {
            MassTag mt = TestUtilities.GetMassTagStandard(1);

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.CurrentMassTag = mt;

            TomTheorFeatureGenerator unlabelledTheorGenerator = new TomTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);

            PeakChromatogramGenerator peakChromGen = new PeakChromatogramGenerator(10, ChromatogramGeneratorMode.TOP_N_PEAKS);
            peakChromGen.TopNPeaksLowerCutOff = 0.4;
            peakChromGen.Execute(run.ResultCollection);

            Assert.AreEqual(133, run.XYData.Xvalues.Length);
            Assert.AreEqual(5543, (int)run.XYData.Xvalues[35]);
            //Assert.AreEqual(7319569, (int)run.XYData.Yvalues[35]);
            run.XYData.Display();


        }


        [Test]
        public void getPeakChromatogramUsingChromGenTest1()
        {
            double targetMZ = 831.48;
            double toleranceInPPM = 20;

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            ChromatogramGenerator chromGen = new ChromatogramGenerator();
            run.XYData = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, targetMZ, toleranceInPPM);


            for (int i = 0; i < run.XYData.Xvalues.Length; i++)
            {
                run.XYData.Xvalues[i] = run.GetTime(i);

                
            }

       


            run.XYData.Display();


        }




    }
}
