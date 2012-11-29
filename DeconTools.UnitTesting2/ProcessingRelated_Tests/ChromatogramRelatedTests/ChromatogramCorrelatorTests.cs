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

            DeconToolsSavitzkyGolaySmoother smoother = new DeconToolsSavitzkyGolaySmoother(1, 1, 2);
            
            PeakChromatogramGenerator peakChromGen = new PeakChromatogramGenerator(chromToleranceInPPM);
            peakChromGen.GenerateChromatogram(run,startScan,stopScan,mt.IsotopicProfile.Peaklist[0].XValue,chromToleranceInPPM);
            run.XYData = smoother.Smooth(run.XYData);
            
            XYData chromdata1 = run.XYData.TrimData(startScan, stopScan);


            peakChromGen.GenerateChromatogram(run, startScan, stopScan, mt.IsotopicProfile.Peaklist[1].XValue, chromToleranceInPPM);
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

    }
}
