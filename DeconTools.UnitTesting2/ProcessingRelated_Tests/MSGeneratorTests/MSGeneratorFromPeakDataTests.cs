using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSGeneratorTests
{
    [TestFixture]
    public class MSGeneratorFromPeakDataTests
    {
        string m_testFile = FileRefs.RawDataMSFiles.YAFMSStandardFile2;


        [Test]
        public void GenerateSyntheticMSBasedOnPeakDataTest1()
        {

            var fileOutput_xyvalsBefore = FileRefs.OutputFolderPath + "MENDData_scan311_before.txt";
            var fileOutput_xyvalsAfter = FileRefs.OutputFolderPath + "MENDData_scan311_after.txt";

            Run run = new YAFMSRun(m_testFile);

            var scan = new ScanSet(311);
            run.CurrentScanSet = scan;

            
            
            var peakWidthForAllPeaks = 0.001;
            var synMSGen =
                new DeconTools.Backend.ProcessingTasks.MSGenerators.SyntheticMSGeneratorFromPeakData();


            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            msgen.Execute(run.ResultCollection);
            Assert.AreEqual(15226, run.XYData.Xvalues.Length);

            TestUtilities.WriteToFile(run.XYData, fileOutput_xyvalsBefore);


            run.PeakList = PeakUtilities.CreatePeakDataFromXYData(run.XYData, peakWidthForAllPeaks);
            Assert.AreEqual(15226, run.PeakList.Count);

            synMSGen.Execute(run.ResultCollection);
            Assert.AreNotEqual(15226, run.XYData.Xvalues.Length);

            Console.WriteLine();

            Console.WriteLine(run.XYData.Xvalues.Length);

            
            TestUtilities.WriteToFile(run.XYData,fileOutput_xyvalsAfter);

        }


    }
}
