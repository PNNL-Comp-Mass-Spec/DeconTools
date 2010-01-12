using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using DeconTools.Utilities;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Data;
using DeconTools.Backend.Utilities;


namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class RapidDeconvolutorTests
    {
        string imfMSScanTextfile = "..\\..\\Testfiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
        string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
        
        string imfFilepath2 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.Accum_1200.IMF";


        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        public string xcaliburParameterFile1 = "..\\..\\TestFiles\\xcaliburParameterFile1.xml";

        public string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.uimf";


        /// <summary>
        /// This test is on XY values from a text file.
        /// </summary>
        [Test]
        public void test1()
        {

            Run run = new MSScanFromTextFileRun(imfMSScanTextfile, Globals.XYDataFileType.Textfile);
            ResultCollection resultcollection = new ResultCollection(run);

            Task msgen = new GenericMSGenerator();
            msgen.Execute(resultcollection);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(resultcollection);

            Task rapidDecon = new RapidDeconvolutor();
            rapidDecon.Execute(resultcollection);

            int counter = 0;
            foreach (StandardIsosResult result in resultcollection.ResultList)
            {
                IsotopicProfile profile = result.IsotopicProfile;
                Console.Write("------------ profile" + counter + "; Charge state = " + profile.ChargeState
                    + "; Score= " + profile.Score.ToString("0.00") + " ----------------\n");
                int peakcounter = 0;
                Console.Write("idx" + "\t" + "mz" + "\t" + "intensity" + "\t" + "SN" + "\t" + "FWHM" + "\n");
                foreach (MSPeak peak in profile.Peaklist)
                {
                    Console.Write(peakcounter + "\t" + peak.XValue + "\t" + peak.Height + "\t" + peak.SN + "\t" + peak.Width + "\n");
                    peakcounter++;
                }
                counter++;
            }

            Assert.AreEqual(9, resultcollection.ResultList.Count);
            Assert.AreEqual(582.820684517665, Convert.ToDecimal(resultcollection.ResultList[0].IsotopicProfile.Peaklist[0].XValue));
            Assert.AreEqual(2984.0, resultcollection.ResultList[0].IsotopicProfile.Peaklist[0].Height);
            Assert.AreEqual(0.05905123, (decimal)resultcollection.ResultList[0].IsotopicProfile.Peaklist[0].Width);
            Assert.AreEqual(157.0526, (decimal)resultcollection.ResultList[0].IsotopicProfile.Peaklist[0].SN);


            Assert.AreEqual(4593, resultcollection.ResultList[0].IsotopicProfile.GetAbundance());

            Assert.AreEqual(1, resultcollection.ResultList[8].IsotopicProfile.GetNumOfIsotopesInProfile());
            Assert.AreEqual(2488.07303881522, Convert.ToDecimal(resultcollection.ResultList[8].IsotopicProfile.AverageMass));
            Assert.AreEqual(3, resultcollection.ResultList[8].IsotopicProfile.ChargeState);
            Assert.AreEqual(18802, Convert.ToDecimal(resultcollection.ResultList[8].IsotopicProfile.IntensityAggregate));
            Assert.AreEqual(2486.09777364184, Convert.ToDecimal(resultcollection.ResultList[8].IsotopicProfile.MonoIsotopicMass));



        }

        /// <summary>
        /// This test is performed on the actual IMF file. Values should be the same as above
        /// </summary>
        [Test]
        public void deconvoluteIMFDataTest1()
        {

            Run run = new IMFRun(imfFilepath);
            run.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            run.CurrentScanSet = new ScanSet(233);

            ResultCollection results = new ResultCollection(run);

            Task msGen = new GenericMSGenerator();
            msGen.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            
            Task peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(results);

            Task rapid = new RapidDeconvolutor();
            rapid.Execute(results);

            Assert.AreEqual(9, results.ResultList.Count);
            Assert.AreEqual(582.820684517665, Convert.ToDecimal(results.ResultList[0].IsotopicProfile.Peaklist[0].XValue));
            Assert.AreEqual(2984.0, results.ResultList[0].IsotopicProfile.Peaklist[0].Height);
            Assert.AreEqual(0.05905123, (decimal)results.ResultList[0].IsotopicProfile.Peaklist[0].Width);
            Assert.AreEqual(157.0526, (decimal)results.ResultList[0].IsotopicProfile.Peaklist[0].SN);


            Assert.AreEqual(4593, results.ResultList[0].IsotopicProfile.GetAbundance());

            Assert.AreEqual(1, results.ResultList[8].IsotopicProfile.GetNumOfIsotopesInProfile());
            Assert.AreEqual(2488.07303881522, Convert.ToDecimal(results.ResultList[8].IsotopicProfile.AverageMass));
            Assert.AreEqual(3, results.ResultList[8].IsotopicProfile.ChargeState);
            Assert.AreEqual(18802, Convert.ToDecimal(results.ResultList[8].IsotopicProfile.IntensityAggregate));
            Assert.AreEqual(2486.09777364184, Convert.ToDecimal(results.ResultList[8].IsotopicProfile.MonoIsotopicMass));
            Assert.AreEqual(829.711934128853, Convert.ToDecimal(results.ResultList[8].IsotopicProfile.MonoPeakMZ));


        }
       
        [Test]
        public void deconvoluteXCalibur_MSMS_DataTest1()    // ms/ms data... expect to find no isotopic profiles...
        {

            Run run = new XCaliburRun(xcaliburTestfile);
            run.CurrentScanSet = new ScanSet(6000);

            ResultCollection rc = new ResultCollection(run);

            Task msGen = new GenericMSGenerator();
            msGen.Execute(rc);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(rc);

            Task rapid = new RapidDeconvolutor();
            rapid.Execute(rc);

            Assert.AreEqual(0, rc.ResultList.Count);



        }

        [Test]
        public void deconvoluteXCalibur_MS_DataTest1()    //good ms data...  expect to find isotopic profiles...
        {

            Run run = new XCaliburRun(xcaliburTestfile);
            run.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            run.CurrentScanSet = new ScanSet(6067);

            ResultCollection rc = new ResultCollection(run);

            Task msGen = new GenericMSGenerator();
            msGen.Execute(rc);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            Task peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(rc);

            Task rapid = new RapidDeconvolutor();
            rapid.Execute(rc);

            Assert.AreEqual(95, rc.ResultList.Count);



        }

        [Test]
        public void multipleScansTest1()
        {
            Run run = new IMFRun(imfFilepath);

            ScanSetCollection scanSetCollection = new ScanSetCollection();
            scanSetCollection.ScanSetList.Add(new ScanSet(232));
            scanSetCollection.ScanSetList.Add(new ScanSet(233));
            scanSetCollection.ScanSetList.Add(new ScanSet(234));

            ResultCollection results = new ResultCollection(run);

            foreach (ScanSet scanset in scanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;

                Task msGen = new GenericMSGenerator();
                msGen.Execute(results);

                DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
                detectorParams.PeakBackgroundRatio = 3;
                detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
                detectorParams.SignalToNoiseThreshold = 3;
                detectorParams.ThresholdedData = false;

                Task peakDetector = new DeconToolsPeakDetector(detectorParams);
                peakDetector.Execute(results);

                Task decon = new RapidDeconvolutor();
                decon.Execute(results);

                Task scu = new ScanResultUpdater();
                scu.Execute(results);

            }

            Assert.AreEqual(3, results.ScanResultList.Count);

            Assert.AreEqual(92, results.ScanResultList[0].NumPeaks);
            Assert.AreEqual(82, results.ScanResultList[1].NumPeaks);
            Assert.AreEqual(72, results.ScanResultList[2].NumPeaks);

            Assert.AreEqual(11, results.ScanResultList[0].NumIsotopicProfiles);
            Assert.AreEqual(9, results.ScanResultList[1].NumIsotopicProfiles);
            Assert.AreEqual(9, results.ScanResultList[2].NumIsotopicProfiles);

            Assert.AreEqual(830.045752112968, (Decimal)results.ScanResultList[0].BasePeak.XValue);
            Assert.AreEqual(10438, results.ScanResultList[0].BasePeak.Height);
            Assert.AreEqual(0.09454554, (Decimal)results.ScanResultList[0].BasePeak.Width);
            Assert.AreEqual(434.9167, (Decimal)results.ScanResultList[0].BasePeak.SN);


        }

        /// <summary>
        /// The next test examines an odd peak @ 630 in scan 233.  Shows that the selection of the MZ range
        /// effects the Rapid Score and the selected monoMZ. Rapid sometimes selects the wrong
        /// monoisotopic peak for this cluster. 
        /// </summary>
        [Test]
        public void examineIMFFile_Scan233MZ630()
        {
            Run run = new IMFRun(imfFilepath);
            run.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            run.CurrentScanSet = new ScanSet(233, 229, 237);
            run.MSParameters.MinMZ = 100;
            run.MSParameters.MaxMZ = 631.5;

            int numPeaks = 0;

            for (double n = 629; n < 630; n = n + 0.01)
            {

                ResultCollection rc = new ResultCollection(run);

                Task msGen = new GenericMSGenerator(n, run.MSParameters.MaxMZ);
                msGen.Execute(rc);

                DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
                detectorParams.PeakBackgroundRatio = 3;
                detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
                detectorParams.SignalToNoiseThreshold = 3;
                detectorParams.ThresholdedData = false;

                Task zeroFiller = new DeconToolsZeroFiller(3);
                //zeroFiller.Execute(rc);

                Task peakDetector = new DeconToolsPeakDetector(detectorParams);
                peakDetector.Execute(rc);

                Task decon = new RapidDeconvolutor();
                decon.Execute(rc);


                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < rc.ResultList.Count; i++)
                {
                    IsosResult result = rc.ResultList[i];
                    if (result.IsotopicProfile.Peaklist[0].XValue > 629 && result.IsotopicProfile.Peaklist[0].XValue < 631)
                    {
                        sb.Append(n.ToString("0.00"));
                        sb.Append("\t");
                        sb.Append(result.IsotopicProfile.Peaklist[0].XValue.ToString("0.00"));
                        sb.Append("\t");
                        sb.Append(result.IsotopicProfile.Score.ToString("0.00"));
                        sb.Append("\t");
                        sb.Append(result.Run.PeakList.Count);
                        sb.Append("\t");
                        sb.Append(result.Run.XYData.Xvalues.Length);

                        sb.Append("\n");
                    }
                }
                Console.Write(sb.ToString());


            }



        }

        [Test]
        public void examineRAWFile_Scan6757()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            run.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            run.CurrentScanSet = new ScanSet(6757);
            run.MSParameters.MinMZ = 0;
            run.MSParameters.MaxMZ = 10000;

            ResultCollection rc = new ResultCollection(run);

            Task msGen = new GenericMSGenerator(run.MSParameters.MinMZ, run.MSParameters.MaxMZ);
            msGen.Execute(rc);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task zeroFiller = new DeconToolsZeroFiller(3);
            //zeroFiller.Execute(rc);

            Task peakDetector = new DeconToolsPeakDetector(detectorParams);
            peakDetector.Execute(rc);

            Task decon = new RapidDeconvolutor();
            decon.Execute(rc);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rc.ResultList.Count; i++)
            {
                IsosResult result = rc.ResultList[i];
                if (result.IsotopicProfile.Peaklist[0].XValue > 0 && result.IsotopicProfile.Peaklist[0].XValue < 10000)
                {
                   sb.Append(result.IsotopicProfile.Peaklist[0].XValue.ToString("0.00"));
                    sb.Append("\t");
                    sb.Append(result.IsotopicProfile.Score.ToString("0.00"));
                    sb.Append("\t");
                    sb.Append(result.IsotopicProfile.Peaklist[0].SN.ToString("0.0"));
                    sb.Append("\t");
                    sb.Append(result.Run.PeakList.Count);
                    sb.Append("\t");
                    sb.Append(result.Run.XYData.Xvalues.Length);

                    sb.Append("\n");
                }
            }
            Console.Write(sb.ToString());




        }

        [Test]
        public void examineEffectOfIntensityCutoffOnRapidTest1()
        {

            Project project = Project.getInstance();
            project.Parameters.OldDecon2LSParameters = new OldDecon2LSParameters();


            Run run = new XCaliburRun(xcaliburTestfile, 6000, 7000);
            project.RunCollection.Add(run);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
            scansetCreator.Create();

            ResultCollection results=new ResultCollection(run);


            foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                
                
                Task msgen = new GenericMSGenerator();
                msgen.Execute(results);


                DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
                detectorParams.PeakBackgroundRatio = 3;
                detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
                detectorParams.SignalToNoiseThreshold = 3;
                detectorParams.ThresholdedData = false;
                
                Task peakDetector = new DeconToolsPeakDetector(detectorParams);
                peakDetector.Execute(results);

                for (int i = 1; i < 6; i++)
                {
                    results.ResultList.Clear();

                    Task rapidDecon = new RapidDeconvolutor(i, DeconTools.Backend.ProcessingTasks.IDeconvolutor.DeconResultComboMode.simplyAddIt);
                    rapidDecon.Execute(results);

                    //Console.WriteLine(scanset.PrimaryScanNumber + "\t" + i + "\t" + 
                   //     results.ResultList.Count + "\t" + results.Run.CurrentScanSet.BackgroundIntensity);
                }
                
            }


            

        }

    }
}
