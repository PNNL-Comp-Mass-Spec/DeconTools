using System;
using System.Runtime.InteropServices;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
using NUnit.Framework;
using ThermoRawFileReader;

namespace DeconTools.Testing.ProblemCases
{



    [TestFixture]
    public class JoeBrownTesting
    {


        [Test]
        public void Test1()
        {

            var run =
                new RunFactory().CreateRun(
                    @"D:\Data\From_Joe\MyDeconToolsTester\RawFiles\Transition_Selection_Orbitrap_HCD_8Mar13_Cougar_12-12-36.raw");

            Assert.IsNotNull(run);

            var msGen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            int testScan = 2448;

            run.CurrentScanSet = new ScanSet(testScan);

            DeconToolsPeakDetectorV2 ms1PeakDetector = new DeconToolsPeakDetectorV2();
            ms1PeakDetector.PeakToBackgroundRatio = 1.3;
            ms1PeakDetector.SignalToNoiseThreshold = 2;
            ms1PeakDetector.IsDataThresholded = true;


            DeconToolsPeakDetectorV2 ms2PeakDetectorForCentroidedData = new DeconToolsPeakDetectorV2();
            DeconToolsPeakDetectorV2 ms2PeakDetectorForProfileData = new DeconToolsPeakDetectorV2();


            ms2PeakDetectorForProfileData = new DeconToolsPeakDetectorV2();
            ms2PeakDetectorForProfileData.PeakToBackgroundRatio = ms1PeakDetector.PeakToBackgroundRatio;
            ms2PeakDetectorForProfileData.SignalToNoiseThreshold = ms1PeakDetector.SignalToNoiseThreshold;
            ms2PeakDetectorForProfileData.IsDataThresholded = ms1PeakDetector.IsDataThresholded;


            ms2PeakDetectorForCentroidedData = new DeconToolsPeakDetectorV2(0, 0, Globals.PeakFitType.QUADRATIC, true);
            ms2PeakDetectorForCentroidedData.RawDataType = Globals.RawDataType.Centroided;

            run.ScanSetCollection.Create(run, testScan,testScan , 1, 1, true);

            Console.WriteLine("scan\tz\tinfo");
            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;

                msGen.Execute(run.ResultCollection);
                if (run.GetMSLevel(scan.PrimaryScanNumber) == 1)
                {
                    ms1PeakDetector.Execute(run.ResultCollection);
                }
                else
                {
                    var dataIsCentroided = run.IsDataCentroided(scan.PrimaryScanNumber);

                    Console.WriteLine("Scan " + scan.PrimaryScanNumber + "centroided=" + dataIsCentroided);

                    TestUtilities.DisplayXYValues(run.XYData);



                    if (dataIsCentroided)
                    {
                        ms2PeakDetectorForCentroidedData.Execute(run.ResultCollection);
                    }
                    else
                    {
                        ms2PeakDetectorForProfileData.Execute(run.ResultCollection);
                    }
                }

                Console.WriteLine("---- peaks for Scan " + scan);
                TestUtilities.DisplayPeaks(run.PeakList);
            }



        }

        private XRawFileIO _thermoFileReader;

        [Test]
        public void ReadCollisionEnergyValsTest1()
        {
            var testFile = @"\\proto-2\UnitTest_Files\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            var testScan = 5000;
            _thermoFileReader = new ThermoRawFileReader.XRawFileIO(testFile);

            _thermoFileReader.GetScanInfo(testScan, out clsScanInfo scanInfo);

            Console.WriteLine("RT = " + scanInfo.RetentionTime);

            if (scanInfo.TryGetScanEvent("Ion Injection Time (ms):", out var eventValue, true))
            {
                Console.WriteLine("Ion injection time= " + Convert.ToDouble(eventValue));
            }

        }


         struct PrecursorInfo
        {
            double dIsolationMass;
            double dMonoIsoMass;
            long nChargeState;
            long nScanNumber;
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct FullMSOrderPrecursorInfo
        {
            public double dPrecursorMass;

            public double dIsolationWidth;
            public double dCollisionEnergy;
            public uint uiCollisionEnergyValid;
            public bool bRangeIsValid;
            public double dFirstPrecursorMass;
            public double dLastPrecursorMass;
            public double dIsolationWidthOffset;
        }
    }
}
