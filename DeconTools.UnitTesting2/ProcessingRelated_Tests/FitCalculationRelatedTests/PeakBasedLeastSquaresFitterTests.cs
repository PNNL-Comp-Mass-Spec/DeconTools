using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.DTO;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.FitCalculationRelatedTests
{
    [TestFixture]
    public class PeakBasedLeastSquaresFitterTests
    {
      [Test]
        public void ComparePeakFitterVsAreaFitter()
        {
            string massTagFile1 = Path.Combine(FileRefs.RawDataBasePath, "TargetedWorkflowStandards", "QCShew_peptidesWithObsCountGreaterThan1000.txt");

            //load target 
            var masstagImporter = new MassTagFromTextFileImporter(massTagFile1);
            var targets = masstagImporter.Import().TargetList;


            var run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(1.3, 2, Globals.PeakFitType.QUADRATIC, true);



            var scanSet = new ScanSet(9575);
            run.CurrentScanSet = scanSet;

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            var selectedTarget = targets.First(p => p.ID == 635428 && p.ChargeState == 3);

            var theorFeatureGen = new JoshTheorFeatureGenerator(Backend.Globals.LabellingType.NONE, 0.005);
            theorFeatureGen.GenerateTheorFeature(selectedTarget);


            var peakForFWHM = run.PeakList.First(p => p.XValue > 768.38 && p.XValue < 768.39);



            XYData theorXYdata =
                TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(selectedTarget.IsotopicProfile,
                                                                                    peakForFWHM.Width);

            theorXYdata.NormalizeYData();

            AreaFitter areaFitter = new AreaFitter();
            var areaFitScore=  areaFitter.GetFit(theorXYdata, run.XYData, 0.1);


            PeakLeastSquaresFitter peakLeastSquaresFitter = new PeakLeastSquaresFitter();
            var peakBasedFitScore = peakLeastSquaresFitter.GetFit(new List<Peak>(selectedTarget.IsotopicProfile.Peaklist), run.PeakList, 0.1, 25);


            Console.WriteLine("fit score based on XYData = " + areaFitScore);
            Console.WriteLine("fit score based on Peaks= " + peakBasedFitScore);

          Assert.IsTrue(peakBasedFitScore < 0.1);




        }
    }
}
