using System;
using DeconTools.Backend;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ChromatogramRelatedTests
{
    [TestFixture]
    public class ChromatogramCorrelatorTests
    {

        [Test]
        public void CorrelationTest1()
        {
            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            PeptideTarget mt = TestUtilities.GetMassTagStandard(1);
            run.CurrentMassTag = mt;

            JoshTheorFeatureGenerator unlabelledTheorGenerator = new JoshTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);


            double chromToleranceInPPM = 10;
            int startScan = 5460;
            int stopScan = 5755;

            SavitzkyGolaySmoother smoother = new SavitzkyGolaySmoother(3, 2);
            
            PeakChromatogramGenerator peakChromGen = new PeakChromatogramGenerator(chromToleranceInPPM);
            peakChromGen.GenerateChromatogram(run,startScan,stopScan,mt.IsotopicProfile.Peaklist[0].XValue,chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);
            
            XYData chromdata1 = run.XYData.TrimData(startScan, stopScan);


            peakChromGen.GenerateChromatogram(run, startScan, stopScan, mt.IsotopicProfile.Peaklist[3].XValue, chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);
            
            XYData chromdata2 = run.XYData.TrimData(startScan, stopScan);

          

            //chromdata1.Display();
            //Console.WriteLine();
            //chromdata2.Display();

            ChromatogramCorrelator correlator = new ChromatogramCorrelator();
            double slope=0;
            double intercept=0;
            double rsquaredVal=0;

            correlator.GetElutionCorrelationData(chromdata1, chromdata2, out slope, out intercept, out rsquaredVal);

            Console.WriteLine(mt);

            Console.WriteLine("slope = \t" + slope);
            Console.WriteLine("intercept = \t" + intercept);
            Console.WriteLine("rsquared = \t" + rsquaredVal);


            for (int i = 0; i < chromdata1.Xvalues.Length; i++)
            {
                Console.WriteLine(chromdata1.Xvalues[i] + "\t" + chromdata1.Yvalues[i] + "\t" + chromdata2.Yvalues[i]);
            }


        }


        [Test]
        public void BadCorrelationTest1()
        {
            string dataset =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\Test1_VladAlz\RawData\Alz_P01_A01_097_26Apr12_Roc_12-03-15.RAW";

            Run run = new RunFactory().CreateRun(dataset);

            string peaksDataFile = dataset.ToLower().Replace(".raw", "_peaks.txt");
            PeakImporterFromText peakImporter = new PeakImporterFromText(peaksDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            double chromToleranceInPPM = 10;
            int startScan = 2340;
            int stopScan = 2440;

            SavitzkyGolaySmoother smoother = new SavitzkyGolaySmoother(9, 2);

            double testMZVal1 = 719.80349;

            PeakChromatogramGenerator peakChromGen = new PeakChromatogramGenerator(chromToleranceInPPM);
            peakChromGen.GenerateChromatogram(run, startScan, stopScan, testMZVal1, chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);

            XYData chromdata1 = run.XYData.TrimData(startScan, stopScan);

            double testMZVal2 = 722.325;
            peakChromGen.GenerateChromatogram(run, startScan, stopScan, testMZVal2, chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);

            XYData chromdata2 = run.XYData.TrimData(startScan, stopScan);



            //chromdata1.Display();
            //Console.WriteLine();
            //chromdata2.Display();

            ChromatogramCorrelator correlator = new ChromatogramCorrelator();
            double slope = 0;
            double intercept = 0;
            double rsquaredVal = 0;

            correlator.GetElutionCorrelationData(chromdata1, chromdata2, out slope, out intercept, out rsquaredVal);

           
            Console.WriteLine("slope = \t" + slope);
            Console.WriteLine("intercept = \t" + intercept);
            Console.WriteLine("rsquared = \t" + rsquaredVal);


            for (int i = 0; i < chromdata1.Xvalues.Length; i++)
            {
                Console.WriteLine(chromdata1.Xvalues[i] + "\t" + chromdata1.Yvalues[i] + "\t" + chromdata2.Yvalues[i]);
            }


        }


    }
}
