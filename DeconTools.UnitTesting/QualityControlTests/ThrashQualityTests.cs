using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting.QualityControlTests
{
    public class ThrashQualityTests
    {

        public string agilentFile = @"\\proto-10\IMS_DEV2\Agilent_QTOF_DATA\Shew_250ng-r001.d";

        [Test]
        public void agilentDeconTest1()
        {
            RunFactory runFact = new RunFactory();
            Run run = runFact.CreateRun(agilentFile);

            MSGeneratorFactory msfactory = new MSGeneratorFactory();
            Task msgen = msfactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 7;
            peakDet.SigNoiseThreshold = 3;

            HornDeconvolutor decon = new HornDeconvolutor();
            ScanSet scan1821 = new ScanSet(1821);
            run.CurrentScanSet = scan1821;

            msgen.Execute(run.ResultCollection);
            run.XYData = run.XYData.TrimData(450, 520);

            peakDet.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);

            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

        }



   
    }
}
