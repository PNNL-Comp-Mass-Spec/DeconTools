using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class NegIonModeTesting
    {
        [Test]
        public void Test1()
        {


            string testfile = @"D:\Data\From_Nicola\AC2_Neg_highpH_14Apr13_Sauron_13-04-03.raw";

            Run run = new RunFactory().CreateRun(testfile);

            TestUtilities.DisplayRunInformation(run);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            ScanSet scanSet = new ScanSet(4100);
            run.CurrentScanSet = scanSet;

            msgen.Execute(run.ResultCollection);

            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.Execute(run.ResultCollection);

            run.PeakList = run.PeakList.Where(p => p.XValue > 742 && p.XValue < 749).ToList();

            HornDeconvolutor hornDeconvolutor = new HornDeconvolutor();
            hornDeconvolutor.ChargeCarrierMass = -1.00727649;

            hornDeconvolutor.Execute(run.ResultCollection);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);


        }

    }
}
