using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Data;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Scripts
{
    [TestFixture]
    public class UIMFChromatogramScripts
    {
        [Test]
        public void BasePeakChromatogramTests()
        {
            string filename = @"D:\Data\UIMF\Sarc_P13_C10_1186_23Sep11_Cheetah_11-09-06.uimf";
            Run run = new RunFactory().CreateRun(filename);

            ScanSetCollection scanSetCollection = new ScanSetCollection();
            //scanSetCollection.Create(run, 500, 550,1,1);
            scanSetCollection.Create(run, run.MinLCScan, run.MaxLCScan, 1, 1);

            IMSScanSetCollection imsScanSetCollection = new IMSScanSetCollection();
            imsScanSetCollection.Create(run, 60, 250, 1, 1);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDet = new DeconToolsPeakDetectorV2();
            peakDet.PeakToBackgroundRatio = 4;
            peakDet.SignalToNoiseThreshold = 2;
            peakDet.IsDataThresholded = true;

            string outputFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2013\Saturation_Correction\chromOuput.txt";

            using (StreamWriter writer = new StreamWriter(outputFile))
            {

                for (int lcscan = 0; lcscan < scanSetCollection.ScanSetList.Count; lcscan++)
                {
                    Console.WriteLine("lcscan= " + lcscan);

                    var scanSet = scanSetCollection.ScanSetList[lcscan];
                    run.CurrentScanSet = scanSet;

                    StringBuilder sb = new StringBuilder();

                    int numImsScans = imsScanSetCollection.ScanSetList.Count;

                    for (int imsScan = 0; imsScan < numImsScans; imsScan++)
                    {
                        IMSScanSet imsscanSet = (IMSScanSet)imsScanSetCollection.ScanSetList[imsScan];
                        ((UIMFRun)run).CurrentIMSScanSet = imsscanSet;

                        int basePeakIntensity = 0;

                        try
                        {
                            msgen.Execute(run.ResultCollection);
                            peakDet.Execute(run.ResultCollection);

                            basePeakIntensity = GetMaxPeak(run.PeakList);


                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine("skipping lcscan " + lcscan + " --- it is empty");


                        }


                        sb.Append(basePeakIntensity);

                        if (imsScan != numImsScans - 1) // if not the last value, add delimiter
                        {
                            sb.Append("\t");
                        }


                    }
                    writer.WriteLine(sb.ToString());


                }
            }
        }


        [Test]
        public void BPISaturationCorrectedTest1()
        {

            string filename = @"D:\Data\UIMF\Sarc_P13_C10_1186_23Sep11_Cheetah_11-09-06.uimf";
            Run run = new RunFactory().CreateRun(filename);

            run.ScanSetCollection = new ScanSetCollection();
            //scanSetCollection.Create(run, 500, 550,1,1);

            int startFrame = 477;
            int stopFrame = 477;

            //startFrame = run.MinLCScan;
            //stopFrame = run.MaxLCScan;


            run.ScanSetCollection.Create(run, startFrame, stopFrame, 1, 1);




            ((UIMFRun)run).IMSScanSetCollection = new IMSScanSetCollection();
            ((UIMFRun)run).IMSScanSetCollection.Create(run, 122, 122, 1, 1);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDet = new DeconToolsPeakDetectorV2();
            peakDet.PeakToBackgroundRatio = 2;
            peakDet.SignalToNoiseThreshold = 2;
            peakDet.IsDataThresholded = true;


            var _zeroFiller = new DeconToolsZeroFiller();

            var _deconvolutor = new ThrashDeconvolutorV2();
            _deconvolutor.UseAutocorrelationChargeDetermination = true;
            _deconvolutor.Parameters.MaxFit = 0.8;
            

            var saturationWorkflow = new SaturationIMSScanBasedWorkflow(new DeconToolsParameters(), run);



            string outputFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2013\Saturation_Correction\chromOuputSatCorrected.txt";

            using (StreamWriter writer = new StreamWriter(outputFile))
            {

                UIMFRun uimfRun = (UIMFRun)run;


                var _unsummedMSFeatures = new List<IsosResult>();

                foreach (var lcScanSet in run.ScanSetCollection.ScanSetList)
                {
                    Console.WriteLine("Scanset= " + lcScanSet);


                    uimfRun.ResultCollection.MSPeakResultList.Clear();
                    _unsummedMSFeatures.Clear();


                    ScanSet unsummedFrameSet = new ScanSet(lcScanSet.PrimaryScanNumber);
                    //get saturated MSFeatures for unsummed data
                    uimfRun.CurrentScanSet = unsummedFrameSet;


                    StringBuilder sb = new StringBuilder();

                    bool errorInFrame = false;
                    int numIMSScans = uimfRun.IMSScanSetCollection.ScanSetList.Count;
                    for (int imsScanNum = 0; imsScanNum < numIMSScans; imsScanNum++)
                    {


                        var imsScanSet = uimfRun.IMSScanSetCollection.ScanSetList[imsScanNum];
                        uimfRun.ResultCollection.IsosResultBin.Clear(); //clear any previous MSFeatures

                        var unsummedIMSScanset = new IMSScanSet(imsScanSet.PrimaryScanNumber);
                        uimfRun.CurrentIMSScanSet = unsummedIMSScanset;

                        try
                        {
                            msgen.Execute(run.ResultCollection);

                            _zeroFiller.Execute(run.ResultCollection);

                            peakDet.Execute(run.ResultCollection);

                            _deconvolutor.Deconvolute(uimfRun.ResultCollection); //adds to IsosResultBin
                        }
                        catch (Exception ex)
                        {
                            errorInFrame = true;
                        }

                        _unsummedMSFeatures.AddRange(run.ResultCollection.IsosResultBin);


                        int basePeakIntensity = 0;
                        //iterate over unsummed MSFeatures and check for saturation
                        foreach (var isosResult in uimfRun.ResultCollection.IsosResultBin)
                        {
                            bool isPossiblySaturated = isosResult.IntensityAggregate > 1e7;

                            if (isPossiblySaturated)
                            {
                                var theorIso = new IsotopicProfile();

                                saturationWorkflow.RebuildSaturatedIsotopicProfile(isosResult, uimfRun.PeakList, out theorIso);
                                saturationWorkflow.AdjustSaturatedIsotopicProfile(isosResult.IsotopicProfile, theorIso, true, true);

                            }


                            TestUtilities.DisplayIsotopicProfileData(isosResult.IsotopicProfile);


                            int height = (int)isosResult.IsotopicProfile.getMostIntensePeak().Height;
                            if (height > basePeakIntensity)
                            {
                                basePeakIntensity = height;
                            }
                        }


                        sb.Append(basePeakIntensity);

                        bool isNotLastIMSScan = imsScanNum != numIMSScans - 1;
                        if (isNotLastIMSScan)
                        {
                            sb.Append("\t");
                        }


                    }

                    if (errorInFrame)
                    {


                    }
                    else
                    {
                        writer.WriteLine(sb.ToString());

                    }



                }







            }












        }




        [Test]
        public void Generate3dBpiFromIsosFile1()
        {

            string isosFile =
                @"D:\Data\UIMF\Sarc_P13_C10_1186_23Sep11_Cheetah_11-09-06_isos.csv";

            string outputFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2013\Saturation_Correction\chromOuputSatCorrected.txt";

            int lcScanStart = 450;
            int lcScanStop = 550;


            IsosResultUtilities isosResultUtilities = new IsosResultUtilities();
            var filteredIsos = isosResultUtilities.getUIMFResults(isosFile, lcScanStart, lcScanStop);


            string outputIsosFilename = isosFile.Replace("_isos.csv", "_frame450-550_isos.csv");

            int indexFrameCol = 1;
            isosResultUtilities.FilterAndOutputIsos(isosFile, indexFrameCol, lcScanStart, lcScanStop, outputIsosFilename);


            int imsScanStart = 60;
            int imsScanStop = 250;


            int startFrameIndex = 0;

            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                for (int scan = lcScanStart; scan <= lcScanStop; scan++)
                {
                    Console.WriteLine("Frame = " + scan);

                    var lcScanResults = (from n in filteredIsos where n.ScanSet.PrimaryScanNumber == scan select n).ToList();

                    StringBuilder sb = new StringBuilder();
                    for (int imsScan = imsScanStart; imsScan <= imsScanStop; imsScan++)
                    {

                        var imsScanResults =
                            (from n in lcScanResults
                             where ((UIMFIsosResult)n).IMSScanSet.PrimaryScanNumber == imsScan
                             orderby n.IntensityAggregate descending
                             select n).ToList();

                        int basePeakIntensity = 0;
                        if (imsScanResults.Count == 0)
                        {

                        }
                        else
                        {
                            basePeakIntensity = (int)imsScanResults.First().IntensityAggregate;
                        }

                        sb.Append(basePeakIntensity);

                        if (imsScan != imsScanStop)
                        {
                            sb.Append("\t");
                        }
                    }

                    writer.WriteLine(sb.ToString());
                }
            }


        }



        private int GetMaxPeak(List<Peak> peakList)
        {
            int maxIntensity = 0;
            for (int i = 0; i < peakList.Count; i++)
            {
                int currentIntensity = (int)peakList[i].Height;
                if (currentIntensity > maxIntensity)
                {
                    maxIntensity = currentIntensity;
                }
            }
            return maxIntensity;
        }
    }
}
