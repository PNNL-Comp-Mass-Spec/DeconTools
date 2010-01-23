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
            MassTag massTag = new MassTag();
            massTag.ID = 56488;
            massTag.MonoIsotopicMass = 2275.1694779;
            massTag.ChargeState = 3;
            massTag.MZ = massTag.MonoIsotopicMass/massTag.ChargeState+1.00727649;


            Run run = new XCaliburRun(xcaliburTestfile);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            run.CurrentMassTag = massTag;

            Task peakChromGen = new PeakChromatogramGenerator(10);
            peakChromGen.Execute(run.ResultCollection);

            Assert.AreEqual(1, run.ResultCollection.MassTagResultList.Count);

            //Assert.AreEqual(104, run.XYData.Xvalues.Length);
            //Assert.AreEqual(74, run.ResultCollection.MassTagResultList[massTag].ChromValues.Xvalues.Length);

            TestUtilities.DisplayXYValues(run.ResultCollection);


        }

        private void displayChromValues(ResultCollection resultCollection)
        {

          
        }


 
    }
}
