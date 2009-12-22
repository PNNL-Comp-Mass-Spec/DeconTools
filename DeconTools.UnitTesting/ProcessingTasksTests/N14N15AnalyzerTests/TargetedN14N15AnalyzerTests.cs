using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.NETAlignment;

namespace DeconTools.UnitTesting.ProcessingTasksTests.N14N15AnalyzerTests
{
    [TestFixture]
    public class TargetedN14N15AnalyzerTests
    {
        private string xcaliburPeakDataFile = "..\\..\\TestFiles\\XCaliburPeakDataScans5500-6500.txt";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void test1()
        {

            MassTag massTag = new MassTag();
            massTag.ID = 56488;
            massTag.MonoIsotopicMass = 2275.1694779;
            massTag.ChargeState = 3;
            massTag.MZ = massTag.MonoIsotopicMass / massTag.ChargeState + 1.00727649;
            massTag.NETVal = 0.3520239f;


            Run run = new XCaliburRun(xcaliburTestfile);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            run.CurrentMassTag = massTag;

            Task peakChromGen = new PeakChromatogramGenerator(10);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(2, 2, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector();
            
            
            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);

            TestUtilities.DisplayPeaks(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);

        }

        [Test]
        public void test2()
        {

            MassTag massTag = new MassTag();
            massTag.ID = 56488;
            massTag.MonoIsotopicMass = 2275.1694779;
            massTag.ChargeState = 3;
            massTag.MZ = massTag.MonoIsotopicMass / massTag.ChargeState + 1.00727649;


            Run run = new XCaliburRun(xcaliburTestfile);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            run.CurrentMassTag = massTag;

            Task peakChromGen = new PeakChromatogramGenerator(10);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(2, 2, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector();


            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);

            TestUtilities.DisplayPeaks(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);

        }


        [Test]
        public void test3()
        {

            MassTag massTag = new MassTag();
            massTag.ID = 56488;
            massTag.MonoIsotopicMass = 2275.1694779;
            massTag.ChargeState = 3;
            massTag.MZ = massTag.MonoIsotopicMass / massTag.ChargeState + 1.00727649;
            massTag.NETVal = 0.3520239f;


            Run run = new XCaliburRun(xcaliburTestfile);
            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);


            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            run.CurrentMassTag = massTag;

            Task peakChromGen = new PeakChromatogramGenerator(10);

            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(2, 2, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector();




            peakChromGen.Execute(run.ResultCollection);
            smoother.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);

            

            TestUtilities.DisplayPeaks(run.ResultCollection);
            TestUtilities.DisplayXYValues(run.ResultCollection);

        }
    }
}
