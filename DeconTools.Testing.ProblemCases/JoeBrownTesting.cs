using System;
using System.Runtime.InteropServices;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
using MSFileReaderLib;
using NUnit.Framework;

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



        private MSFileReaderLib.IXRawfile5 _msfileReader;
      
        [Test]
        public void ReadCollisionEnergyValsTest1()
        {
            string testFile = @"\\proto-5\BionetDataXfer\People\Tao\CPTAC\CPTAC_10_Pep_QC_long_rampCE_13Mar19_Gandal_W22511A1.raw";


            //testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            int testScan = 2519;
            _msfileReader = (IXRawfile5)new MSFileReaderLib.MSFileReader_XRawfile();
            _msfileReader.Open(testFile);
            _msfileReader.SetCurrentController(0, 1);
            string pbstrFilter = null;
            double pdRT = 0;
            
            _msfileReader.RTFromScanNum(testScan, ref pdRT);
            Console.WriteLine("RT = " + pdRT);

            object value = null;
            _msfileReader.GetTrailerExtraValueForScanNum(testScan, "Ion Injection Time (ms):", ref value);
            Console.WriteLine("Ion injection time= " + Convert.ToDouble(value));
            
            double pdCollisionEnergy = 0;
            _msfileReader.GetCollisionEnergyForScanNum(testScan, 1, ref pdCollisionEnergy);
            Console.WriteLine("Collision energy ="  +  Convert.ToDouble(pdCollisionEnergy));

            int pnNumInstMethods = 0;
            _msfileReader.GetNumInstMethods(ref pnNumInstMethods);
            Console.WriteLine("numMethods= " + Convert.ToDouble(pnNumInstMethods));

            string pbstrInstMethod = null;
            _msfileReader.GetInstMethod(0, ref pbstrInstMethod);
            Console.WriteLine(pbstrInstMethod);

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
