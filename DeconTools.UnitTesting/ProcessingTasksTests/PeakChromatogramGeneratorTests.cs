using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class PeakChromatogramGeneratorTests
    {
        private string xcaliburPeakDataFile = "..\\..\\TestFiles\\XCaliburPeakDataScans5500-6500.txt";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";


        [Test]
        public void getPeakChromatogramTest1()
        {

            MassTag mt = TestUtilities.GetMassTagStandard(1);

            Run run = new XCaliburRun(xcaliburTestfile);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.CurrentMassTag = mt;

            Task peakChromGen = new PeakChromatogramGenerator(10);
            peakChromGen.Execute(run.ResultCollection);

            //TestUtilities.DisplayXYValues(run.ResultCollection);
            Assert.AreEqual(1, run.ResultCollection.MassTagResultList.Count);

            Assert.AreEqual(133, run.XYData.Xvalues.Length);
            Assert.AreEqual(5543,(int) run.XYData.Xvalues[35]);
            Assert.AreEqual(7319569, (int)run.XYData.Yvalues[35]);


        }

        private void displayChromValues(ResultCollection resultCollection)
        {

          
        }


 
    }
}
