using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class UIMFDriftTimeExtractorTests
    {
        private string uimfFilepath = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";


        [Test]
        public void uimf_data_not_preLoaded()        // in this case the drift time is calculated every time by looking up sqlite data from uimf file
        {

            Run run = new UIMFRun(uimfFilepath);

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, 800,801,3, 1);
            fscc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 220, 270, 9, 1);
            sscc.Create();



            UIMF_MSGenerator msgen = new UIMF_MSGenerator();
            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 6;
            peakDet.SigNoiseThreshold = 3;

            HornDeconvolutor decon = new HornDeconvolutor();
            UIMFDriftTimeExtractor driftTimeExtractor = new UIMFDriftTimeExtractor();

            StringBuilder sb = new StringBuilder();

            foreach (var frame in ((UIMFRun)run).FrameSetCollection.FrameSetList)
            {
                ((UIMFRun)run).CurrentFrameSet = frame;

                foreach (var scan in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scan;

                    msgen.Execute(run.ResultCollection);
                    peakDet.Execute(run.ResultCollection);
                    decon.Execute(run.ResultCollection);
                    driftTimeExtractor.Execute(run.ResultCollection);
                }
                
            }

            foreach (var item in run.ResultCollection.ResultList)
            {
                UIMFIsosResult isosresult = (UIMFIsosResult)item;

                sb.Append(isosresult.FrameSet.PrimaryFrame);
                sb.Append("\t");
                sb.Append(isosresult.ScanSet.PrimaryScanNumber);
                sb.Append("\t"); 
                sb.Append(isosresult.IsotopicProfile.MonoPeakMZ.ToString("0.000"));
                sb.Append("\t"); 
                sb.Append(isosresult.DriftTime.ToString("0.000"));
                sb.Append(Environment.NewLine);
            }

            Console.Write(sb.ToString());
    
        }

        [Test]
        public void uimf_data_preLoaded()        // in this case, data is preloaded, preventing re-accessing the same drifttimes over and over
        {

            Run run = new UIMFRun(uimfFilepath);

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, 800, 801, 3, 1);
            fscc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 220, 270, 9, 1);
            sscc.Create();

            ((UIMFRun)run).GetFrameDataAllFrameSets();    // this is the key line... loads data necessary for drift time calc and correction



            UIMF_MSGenerator msgen = new UIMF_MSGenerator();
            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 6;
            peakDet.SigNoiseThreshold = 3;

            HornDeconvolutor decon = new HornDeconvolutor();
            UIMFDriftTimeExtractor driftTimeExtractor = new UIMFDriftTimeExtractor();

            StringBuilder sb = new StringBuilder();

            foreach (var frame in ((UIMFRun)run).FrameSetCollection.FrameSetList)
            {
                ((UIMFRun)run).CurrentFrameSet = frame;

                foreach (var scan in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scan;

                    msgen.Execute(run.ResultCollection);
                    peakDet.Execute(run.ResultCollection);
                    decon.Execute(run.ResultCollection);
                    driftTimeExtractor.Execute(run.ResultCollection);
                }

            }

            foreach (var item in run.ResultCollection.ResultList)
            {
                UIMFIsosResult isosresult = (UIMFIsosResult)item;

                sb.Append(isosresult.FrameSet.PrimaryFrame);
                sb.Append("\t");
                sb.Append(isosresult.ScanSet.PrimaryScanNumber);
                sb.Append("\t");
                sb.Append(isosresult.IsotopicProfile.MonoPeakMZ.ToString("0.000"));
                sb.Append("\t");
                sb.Append(isosresult.DriftTime.ToString("0.000"));
                sb.Append(Environment.NewLine);
            }

            Console.Write(sb.ToString());

        }


    }
}
