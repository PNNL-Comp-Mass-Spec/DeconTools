using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
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
        [Ignore("Local testing only")]
        public void BasePeakChromatogramTests()
        {
            var filename = @"D:\Data\UIMF\Sarc_P13_C10_1186_23Sep11_Cheetah_11-09-06.uimf";
            var run = new RunFactory().CreateRun(filename);

            var scanSetCollection = new ScanSetCollection();
            //scanSetCollection.Create(run, 500, 550,1,1);
            scanSetCollection.Create(run, run.MinLCScan, run.MaxLCScan, 1, 1);

            var imsScanSetCollection = new IMSScanSetCollection();
            imsScanSetCollection.Create(run, 60, 250, 1, 1);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDet = new DeconToolsPeakDetectorV2
            {
                PeakToBackgroundRatio = 4,
                SignalToNoiseThreshold = 2,
                IsDataThresholded = true
            };

            var outputFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2013\Saturation_Correction\chromOuput.txt";

            using (var writer = new StreamWriter(outputFile))
            {

                for (var lcscan = 0; lcscan < scanSetCollection.ScanSetList.Count; lcscan++)
                {
                    Console.WriteLine("lcscan= " + lcscan);

                    var scanSet = scanSetCollection.ScanSetList[lcscan];
                    run.CurrentScanSet = scanSet;

                    var sb = new StringBuilder();

                    var numImsScans = imsScanSetCollection.ScanSetList.Count;

                    for (var imsScan = 0; imsScan < numImsScans; imsScan++)
                    {
                        var imsscanSet = (IMSScanSet)imsScanSetCollection.ScanSetList[imsScan];
                        ((UIMFRun)run).CurrentIMSScanSet = imsscanSet;

                        var basePeakIntensity = 0;

                        try
                        {
                            msgen.Execute(run.ResultCollection);
                            peakDet.Execute(run.ResultCollection);

                            basePeakIntensity = GetMaxPeak(run.PeakList);


                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine("skipping lcscan " + lcscan + " --- it is empty (exception " + ex.Message + ")");


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
        [Ignore("Local testing only")]
        public void BPISaturationCorrectedTest1()
        {

            var filename = @"D:\Data\UIMF\Sarc_P13_C10_1186_23Sep11_Cheetah_11-09-06.uimf";
            var run = new RunFactory().CreateRun(filename);

            run.ScanSetCollection = new ScanSetCollection();
            //scanSetCollection.Create(run, 500, 550,1,1);

            var startFrame = 477;
            var stopFrame = 477;

            //startFrame = run.MinLCScan;
            //stopFrame = run.MaxLCScan;


            run.ScanSetCollection.Create(run, startFrame, stopFrame, 1, 1);




            ((UIMFRun)run).IMSScanSetCollection = new IMSScanSetCollection();
            ((UIMFRun)run).IMSScanSetCollection.Create(run, 122, 122, 1, 1);

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDet = new DeconToolsPeakDetectorV2
            {
                PeakToBackgroundRatio = 2,
                SignalToNoiseThreshold = 2,
                IsDataThresholded = true
            };


            var _zeroFiller = new DeconToolsZeroFiller();

            var _deconvolutor = new ThrashDeconvolutorV2
            {
                UseAutoCorrelationChargeDetermination = true,
                Parameters = {MaxFit = 0.8}
            };


            var saturationWorkflow = new SaturationIMSScanBasedWorkflow(new DeconToolsParameters(), run);



            var outputFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2013\Saturation_Correction\chromOuputSatCorrected.txt";

            using (var writer = new StreamWriter(outputFile))
            {

                var uimfRun = (UIMFRun)run;

                foreach (var lcScanSet in run.ScanSetCollection.ScanSetList)
                {
                    Console.WriteLine("Scanset= " + lcScanSet);


                    uimfRun.ResultCollection.MSPeakResultList.Clear();
                    var unsummedMSFeatures = new List<IsosResult>();

                    var unsummedFrameSet = new ScanSet(lcScanSet.PrimaryScanNumber);
                    //get saturated MSFeatures for unsummed data
                    uimfRun.CurrentScanSet = unsummedFrameSet;


                    var sb = new StringBuilder();

                    var errorInFrame = false;
                    var numIMSScans = uimfRun.IMSScanSetCollection.ScanSetList.Count;
                    for (var imsScanNum = 0; imsScanNum < numIMSScans; imsScanNum++)
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
                        catch (Exception)
                        {
                            errorInFrame = true;
                        }

                        unsummedMSFeatures.AddRange(run.ResultCollection.IsosResultBin);


                        var basePeakIntensity = 0;
                        //iterate over unsummed MSFeatures and check for saturation
                        foreach (var isosResult in uimfRun.ResultCollection.IsosResultBin)
                        {
                            var isPossiblySaturated = isosResult.IntensityAggregate > 1e7;

                            if (isPossiblySaturated)
                            {
                                var theorIso = new IsotopicProfile();

                                saturationWorkflow.RebuildSaturatedIsotopicProfile(run.XYData, isosResult, uimfRun.PeakList, out theorIso);
                                saturationWorkflow.AdjustSaturatedIsotopicProfile(isosResult.IsotopicProfile, theorIso);

                            }


                            TestUtilities.DisplayIsotopicProfileData(isosResult.IsotopicProfile);


                            var height = (int)isosResult.IsotopicProfile.getMostIntensePeak().Height;
                            if (height > basePeakIntensity)
                            {
                                basePeakIntensity = height;
                            }
                        }


                        sb.Append(basePeakIntensity);

                        var isNotLastIMSScan = imsScanNum != numIMSScans - 1;
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
        [Ignore("Local testing only")]
        public void Generate3dBpiFromIsosFile1()
        {
            var outputOrigIntensity = false;
            var outputToConsole = true;

            var isosFile =
                @"D:\Data\UIMF\Sarc_P13_C10_1186_23Sep11_Cheetah_11-09-06_isos.csv";

            var outputFile = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2013\Saturation_Correction\chromOuputSatCorrected.txt";

            var lcScanStart = 450;
            var lcScanStop = 550;


            var isosResultUtilities = new IsosResultUtilities();
            var filteredIsos = isosResultUtilities.getUIMFResults(isosFile, lcScanStart, lcScanStop);


            var outputIsosFilename = isosFile.Replace("_isos.csv", "_frame450-550_isos.csv");

            var indexFrameCol = 1;
            isosResultUtilities.FilterAndOutputIsos(isosFile, indexFrameCol, lcScanStart, lcScanStop, outputIsosFilename);


            var imsScanStart = 60;
            var imsScanStop = 250;

            using (var writer = new StreamWriter(outputFile))
            {
                for (var scan = lcScanStart; scan <= lcScanStop; scan++)
                {
                    //Console.WriteLine("Frame = " + scan);

                    var lcScanResults = (from n in filteredIsos where n.ScanSet.PrimaryScanNumber == scan select n).ToList();

                    var sb = new StringBuilder();
                    for (var imsScan = imsScanStart; imsScan <= imsScanStop; imsScan++)
                    {

                        var imsScanResults =
                            (from n in lcScanResults
                             where ((UIMFIsosResult)n).IMSScanSet.PrimaryScanNumber == imsScan
                             orderby ((UIMFIsosResult)n).IntensityAggregate descending
                             select n).ToList();

                        var basePeakIntensity = 0;
                        if (imsScanResults.Count == 0)
                        {

                        }
                        else
                        {
                            if (outputOrigIntensity)
                            {
                                basePeakIntensity = (int) imsScanResults.First().IsotopicProfile.getMostIntensePeak().Height;
                            }
                            else
                            {
                                basePeakIntensity = (int)imsScanResults.First().IntensityAggregate;
                            }


                        }

                        sb.Append(basePeakIntensity);

                        if (imsScan != imsScanStop)
                        {
                            sb.Append("\t");
                        }
                    }

                    writer.WriteLine(sb.ToString());
                    if (outputToConsole)
                    {
                        Console.WriteLine(sb.ToString());
                    }

                }
            }


        }



        private int GetMaxPeak(List<Peak> peakList)
        {
            var maxIntensity = 0;
            for (var i = 0; i < peakList.Count; i++)
            {
                var currentIntensity = (int)peakList[i].Height;
                if (currentIntensity > maxIntensity)
                {
                    maxIntensity = currentIntensity;
                }
            }
            return maxIntensity;
        }
    }
}
