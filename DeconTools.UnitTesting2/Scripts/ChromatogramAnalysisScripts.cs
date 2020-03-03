using System;
using System.IO;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using GWSGraphLibrary;
using NUnit.Framework;
using ZedGraph;

namespace DeconTools.UnitTesting2.Scripts
{
    [TestFixture]
    public class ChromatogramAnalysisScripts
    {





        [Test]
        public void ChromAnalysisForAllSelectedPrecursors()
        {
            var isDataSmoothed = true;

            var numPointsInSmoothing = 9;
            var smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.SavitzkyGolaySmoother(numPointsInSmoothing, 2);

            var graphGenerator = new BasicGraphControl();

            var thermoFile1 = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var run = new RunFactory().CreateRun(thermoFile1);

            var outputDirectoryForChromGraphs = @"\\protoapps\DataPkgs\Public\2012\684_DeconMSn_research1\ChromatogramImages";
            if (!Directory.Exists(outputDirectoryForChromGraphs)) Directory.CreateDirectory(outputDirectoryForChromGraphs);

            var scanSetCollection = new ScanSetCollection();
            var scanStart = run.MinLCScan;
            var scanStop = run.MaxLCScan;

            scanSetCollection.Create(run,scanStart,scanStop,1, 1, true);
            var sb = new StringBuilder();

            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");
            if (!File.Exists(expectedPeaksFile))
            {
                var peakCreatorParams = new PeakDetectAndExportWorkflowParameters();
                peakCreatorParams.PeakBR = 1.75;

                var peakCreator = new PeakDetectAndExportWorkflow(run, peakCreatorParams);
                peakCreator.Execute();
            }

            var peakImporter = new PeakImporterFromText(expectedPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            double ppmTol = 50;
            var peakChromatogramGenerator = new PeakChromatogramGenerator(ppmTol, Globals.ChromatogramGeneratorMode.MZ_BASED);
            var scansetList = scanSetCollection.ScanSetList;

            var scanCounter = 0;
            var currentBin = 0;
            foreach (var scanSet in scansetList)
            {
                run.CurrentScanSet = scanSet;

                var currentScanLevel = run.GetMSLevel(scanSet.PrimaryScanNumber);


                if (currentScanLevel > 1)
                {
                    scanCounter++;
                    var precursorInfo = run.GetPrecursorInfo(scanSet.PrimaryScanNumber);

                    var scanInfo = run.GetScanInfo(scanSet.PrimaryScanNumber);

                    var scanWindowSize = 400;
                    var startScan = scanSet.PrimaryScanNumber - scanWindowSize / 2;
                    var stopScan = scanSet.PrimaryScanNumber + scanWindowSize / 2;

                    run.XYData=   peakChromatogramGenerator.GenerateChromatogram(run, startScan, stopScan, precursorInfo.PrecursorMZ, ppmTol);

                    if (run.XYData == null)
                    {
                        run.XYData = new XYData();
                        run.XYData.Xvalues = new double[] { 0, 1, 2 };
                        run.XYData.Yvalues = new double[] { 0, 1, 2 };

                        Console.WriteLine(scanSet.PrimaryScanNumber + "\t" + precursorInfo.MSLevel + "\t" + precursorInfo.PrecursorMZ +
                          "\t" + precursorInfo.PrecursorScan + "--------- NO XYData!!! -------------");
                    }
                    else
                    {
                        if (isDataSmoothed)
                        {
                            run.XYData = smoother.Smooth(run.XYData);
                        }
                    }

                    Console.WriteLine(scanSet.PrimaryScanNumber + "\t" + precursorInfo.MSLevel + "\t" + precursorInfo.PrecursorMZ +
                      "\t" + precursorInfo.PrecursorScan);

                    graphGenerator.GenerateGraph(run.XYData.Xvalues, run.XYData.Yvalues);

                    var line = graphGenerator.GraphPane.CurveList[0] as LineItem;
                    line.Line.IsVisible = true;
                    line.Symbol.Size = 2;
                    line.Symbol.Type = SymbolType.Circle;

                    graphGenerator.GraphPane.XAxis.Title.Text = "scan";
                    graphGenerator.GraphPane.YAxis.Title.Text = "intensity";
                    graphGenerator.GraphPane.XAxis.Scale.MinAuto = true;
                    graphGenerator.GraphPane.YAxis.Scale.MinAuto = false;
                    graphGenerator.GraphPane.YAxis.Scale.Min = 0;
                    graphGenerator.GraphPane.XAxis.Scale.FontSpec.Size = 12;
                    graphGenerator.AddVerticalLineToGraph(scanSet.PrimaryScanNumber, 3);
                    graphGenerator.AddAnnotationRelativeAxis(scanInfo, 0.3, 0.1);

                    if (scanCounter > 500)
                    {
                        currentBin++;
                        scanCounter = 0;

                    }
                    var currentOutputDirectory = Path.Combine(outputDirectoryForChromGraphs, "bin" + currentBin);
                    if (!Directory.Exists(currentOutputDirectory)) Directory.CreateDirectory(currentOutputDirectory);


                    var baseFilename =  Path.Combine(currentOutputDirectory,
                                                     scanSet.PrimaryScanNumber.ToString().PadLeft(5, '0') + "_mz" + precursorInfo.PrecursorMZ);

                    string outputGraphFilename;

                    string outputXYData;

                    if (isDataSmoothed)
                    {
                        outputGraphFilename =  baseFilename + "_smoothed_chrom.png";
                        outputXYData = baseFilename + "_smoothed_xydata.txt";
                    }
                    else
                    {
                        outputXYData = baseFilename + "_xydata.txt";
                        outputGraphFilename =  baseFilename + "_chrom.png";
                    }


                    graphGenerator.SaveGraph(outputGraphFilename);
                    TestUtilities.WriteToFile(run.XYData, outputXYData);

                }
            }

            Console.WriteLine(sb.ToString());
        }


        [Test]
        public void ChromAnalysisForAllSelectedPrecursors_Smoothed()
        {

            var graphGenerator = new BasicGraphControl();

            var thermoFile1 = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            var run = new RunFactory().CreateRun(thermoFile1);

            var outputDirectoryForChromGraphs = @"\\protoapps\DataPkgs\Public\2012\684_DeconMSn_research1\ChromatogramImages";
            if (!Directory.Exists(outputDirectoryForChromGraphs)) Directory.CreateDirectory(outputDirectoryForChromGraphs);


            var scanSetCollection = new ScanSetCollection();
            scanSetCollection.Create(run, 1, 1, true);

            var sb = new StringBuilder();

            var expectedPeaksFile = Path.Combine(run.DatasetDirectoryPath, run.DatasetName + "_peaks.txt");


            if (!File.Exists(expectedPeaksFile))
            {
                var peakCreatorParams = new PeakDetectAndExportWorkflowParameters();
                peakCreatorParams.PeakBR = 1.75;

                var peakCreator = new PeakDetectAndExportWorkflow(run, peakCreatorParams);
                peakCreator.Execute();

            }

            var peakImporter = new PeakImporterFromText(expectedPeaksFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            double ppmTol = 50;
            var peakChromatogramGenerator = new PeakChromatogramGenerator(ppmTol, Globals.ChromatogramGeneratorMode.MZ_BASED);

            var numPointsInSmoothing=9;
            var smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.SavitzkyGolaySmoother(numPointsInSmoothing,2);



            //var scansetList =
            //    scanSetCollection.ScanSetList.Where(p => p.PrimaryScanNumber > 6000 && p.PrimaryScanNumber < 6013).
            //        ToList();

            var scansetList = scanSetCollection.ScanSetList;


            var scanCounter = 0;
            var currentBin = 0;
            foreach (var scanSet in scansetList)
            {
                run.CurrentScanSet = scanSet;

                var currentScanLevel = run.GetMSLevel(scanSet.PrimaryScanNumber);


                if (currentScanLevel > 1)
                {
                    scanCounter++;
                    var precursorInfo = run.GetPrecursorInfo(scanSet.PrimaryScanNumber);

                    var scanInfo = run.GetScanInfo(scanSet.PrimaryScanNumber);


                    var scanWindowSize = 400;
                    var startScan = scanSet.PrimaryScanNumber - scanWindowSize / 2;
                    var stopScan = scanSet.PrimaryScanNumber + scanWindowSize / 2;

                    run.XYData=   peakChromatogramGenerator.GenerateChromatogram(run, startScan, stopScan, precursorInfo.PrecursorMZ, ppmTol);



                    if (run.XYData == null)
                    {
                        run.XYData = new XYData();
                        run.XYData.Xvalues = new double[] { 0, 1, 2 };
                        run.XYData.Yvalues = new double[] { 0, 1, 2 };

                        Console.WriteLine(scanSet.PrimaryScanNumber + "\t" + precursorInfo.MSLevel + "\t" + precursorInfo.PrecursorMZ +
                          "\t" + precursorInfo.PrecursorScan + "--------- NO XYData!!! -------------");

                    }
                    else
                    {
                        run.XYData = smoother.Smooth(run.XYData);
                    }





                    Console.WriteLine(scanSet.PrimaryScanNumber + "\t" + precursorInfo.MSLevel + "\t" + precursorInfo.PrecursorMZ +
                      "\t" + precursorInfo.PrecursorScan);

                    graphGenerator.GenerateGraph(run.XYData.Xvalues, run.XYData.Yvalues);




                    var line = graphGenerator.GraphPane.CurveList[0] as LineItem;
                    line.Line.IsVisible = true;
                    line.Symbol.Size = 2;
                    line.Symbol.Type = SymbolType.Circle;

                    graphGenerator.GraphPane.XAxis.Title.Text = "scan";
                    graphGenerator.GraphPane.YAxis.Title.Text = "intensity";
                    graphGenerator.GraphPane.XAxis.Scale.MinAuto = true;
                    graphGenerator.GraphPane.YAxis.Scale.MinAuto = false;
                    graphGenerator.GraphPane.YAxis.Scale.Min = 0;

                    graphGenerator.GraphPane.XAxis.Scale.FontSpec.Size = 12;

                    graphGenerator.AddVerticalLineToGraph(scanSet.PrimaryScanNumber, 3);

                    graphGenerator.AddAnnotationRelativeAxis(scanInfo, 0.3, 0.1);

                    if (scanCounter > 500)
                    {
                        currentBin++;
                        scanCounter = 0;

                    }
                    var currentOutputDirectory = Path.Combine(outputDirectoryForChromGraphs, "bin" + currentBin);
                    if (!Directory.Exists(currentOutputDirectory)) Directory.CreateDirectory(currentOutputDirectory);


                    var outputGraphFilename = Path.Combine(currentOutputDirectory,
                                                           scanSet.PrimaryScanNumber.ToString().PadLeft(5, '0') + "_mz" + precursorInfo.PrecursorMZ + "_chrom_smoothed.png");

                    graphGenerator.SaveGraph(outputGraphFilename);








                }






            }

            Console.WriteLine(sb.ToString());








        }


    }
}
