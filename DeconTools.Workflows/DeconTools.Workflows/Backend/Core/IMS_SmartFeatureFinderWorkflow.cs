﻿
//using CenterSpace.NMath.Matrix;
//using CenterSpace.NMath.Analysis;
//using CenterSpace.NMath.Core;


namespace DeconTools.Workflows.Backend.Core
{
    //public class IMS_SmartFeatureFinderWorkflow : WorkflowBase
    //{

    //    private string rawdataFile;   //@"D:\Data\UIMF\SmartSumming\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";

    //    int peaksInTheTreeCounter = 0;
    //    DeconToolsPeakDetector peakDetector;
    //    HornDeconvolutor hornConvolutor;
    //    const int TENPOWSIX = 1000000;
    //    UIMFRun thisUimfRun;

    //    private string outputFeatureFile;
    //    private string peakFile;
    //    private string logFile;


    //    #region Constructors
    //    public IMS_SmartFeatureFinderWorkflow(Run run, string masterPeakListFilePath)
    //    {
    //        this.Run = run;
    //        this.peakFile = masterPeakListFilePath;

    //        InitializeWorkflow();

    //    }

    //    private string GetlogFileName()
    //    {
    //        return Path.Combine(Path.GetDirectoryName(this.rawdataFile), Path.GetFileNameWithoutExtension(this.rawdataFile) + "_smartSumming_log.txt");

    //    }

    //    private string getOutputFileName()
    //    {
    //        return Path.Combine(Path.GetDirectoryName(this.rawdataFile), Path.GetFileNameWithoutExtension(this.rawdataFile) + "_smart_LCMSFeatures.txt");
    //    }
    //    #endregion

    //    #region Properties

    //    DeconToolsPeakDetector MasterPeakListPeakDetector { get; set; }

    //    public DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother ChromSmoother { get; set; }
    //    public ChromPeakDetector ChromPeakDetector { get; set; }

    //    public ChromatogramGenerator ChromGenerator { get; set; }


    //    public MSGenerator msgen { get; set; }

    //    public double DriftTimeProfileExtractionPPMTolerance { get; set; }

    //    public MSGenerator MSgen { get; set; }
    //    public DeconToolsPeakDetector MSPeakDetector { get; set; }
    //    public Deconvolutor Deconvolutor { get; set; }

    //    public int NumMSScansToSumWhenBuildingMasterPeakList { get; set; }

    //    #endregion

    //    #region Public Methods

    //    #endregion

    //    #region Private Methods

    //    #endregion

    //    #region IWorkflow Members

    //    //  public override WorkflowParameters WorkflowParameters
    //    //{
    //    //    get
    //    //    {
    //    //        throw new NotImplementedException();
    //    //    }
    //    //    set
    //    //    {
    //    //        throw new NotImplementedException();
    //    //    }
    //    //}

    //    public string Name { get; set; }

    //    public override void InitializeWorkflow()
    //    {

    //        NumMSScansToSumWhenBuildingMasterPeakList = 3;

    //        MasterPeakListPeakDetector = new DeconToolsPeakDetector();
    //        MasterPeakListPeakDetector.PeakBackgroundRatio = 4;
    //        MasterPeakListPeakDetector.SigNoiseThreshold = 3;
    //        MasterPeakListPeakDetector.IsDataThresholded = false;
    //        MasterPeakListPeakDetector.StorePeakData = true;


    //        msgen = MSGeneratorFactory.CreateMSGenerator(DeconTools.Backend.Globals.MSFileType.PNNL_UIMF);
    //        this.DriftTimeProfileExtractionPPMTolerance = 15;

    //        this.ChromSmoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother();
    //        this.ChromSmoother.LeftParam = 11;
    //        this.ChromSmoother.RightParam = 11;
    //        this.ChromSmoother.Order = 2;

    //        this.peakDetector = new DeconToolsPeakDetector(4, 3, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);


    //        this.ChromGenerator = new ChromatogramGenerator();

    //        hornConvolutor = new HornDeconvolutor();
    //    }

    //    private bool peakPresent(Peak peak, ScanSet scanSet, UIMFRun run)
    //    {
    //        bool found = false;
    //        //basically we have to check if the peak is present in the given scan set
    //        //so we have to detect the peak
    //        run.CurrentScanSet = scanSet;
    //        msgen.Execute(run.ResultCollection);
    //        List<Peak> peaks = peakDetector.FindPeaks(run.XYData, 0, 5000);

    //        if (peaks.Count > 0)
    //        {
    //            //binary search the peak mz within a certain tolerance in this list
    //            //never mind for now
    //            for (int i = 0; i < peaks.Count; i++)
    //            {
    //                if (Math.Abs(peak.XValue - peaks[i].XValue) * TENPOWSIX / peak.XValue <= 20)
    //                {
    //                    found = true;
    //                    break;
    //                }
    //            }
    //        }

    //        return found;
    //    }


    //    private Peak getMaxPeak(List<Peak> peakList)
    //    {
    //        Peak maxPeak = null;
    //        if (peakList.Count != 0)
    //        {
    //            maxPeak = peakList[0];
    //            for (int i = 0; i < peakList.Count; i++)
    //            {
    //                if (peakList[i].Height > maxPeak.Height)
    //                {
    //                    maxPeak = peakList[i];
    //                }

    //            }
    //        }

    //        return maxPeak;

    //    }

    //    private MSPeakResult parse(string peaksline)
    //    {
    //        MSPeakResult peakresult = new MSPeakResult();
    //        string[] processedLine = peaksline.Split('\t');
    //        if (processedLine.Length < 7)
    //        {
    //            throw new System.IO.IOException("Trying to import peak data into UIMF data object, but not enough columns are present in the source text file");
    //        }

    //        peakresult.PeakID = int.Parse(processedLine[0]);
    //        peakresult.Frame_num = int.Parse(processedLine[1]);
    //        peakresult.Scan_num = int.Parse(processedLine[2]);
    //        peakresult.MSPeak = new DeconTools.Backend.Core.MSPeak();
    //        peakresult.MSPeak.XValue = double.Parse(processedLine[3], CultureInfo.InvariantCulture);
    //        peakresult.MSPeak.Height = float.Parse(processedLine[4], CultureInfo.InvariantCulture);
    //        peakresult.MSPeak.Width = float.Parse(processedLine[5], CultureInfo.InvariantCulture);
    //        peakresult.MSPeak.SignalToNoise = float.Parse(processedLine[6], CultureInfo.InvariantCulture);

    //        if (processedLine.Length > 7)
    //        {
    //            peakresult.MSPeak.MSFeatureID = int.Parse(processedLine[7]);
    //        }

    //        return peakresult;


    //    }

    //    private void getAndUpdateFramePressures()
    //    {
    //        thisUimfRun.GetFrameDataAllFrameSets();
    //        thisUimfRun.SmoothFramePressuresInFrameSets();
    //        Console.WriteLine("Finished updating frame pressures from the other thread");
    //    }

    //    public override void Execute()
    //    {
    //        this.rawdataFile = this.Run.DatasetFileOrDirectoryPath;
    //        this.outputFeatureFile = getOutputFileName();
    //        this.logFile = GetlogFileName();



    //        Stopwatch st = new Stopwatch();
    //        st.Start();

    //        thisUimfRun = (UIMFRun)this.Run;

    //        Console.WriteLine("Writing features to file " + outputFeatureFile);
    //        Console.WriteLine("Writing logs to " + logFile);

    //        FrameSetCollectionCreator fscc;
    //        ScanSetCollectionCreator sscc;
    //        Dictionary<ushort, List<ushort>> frameAndScanNumbers = null;
    //        StringBuilder featureOutputStringBuilder = new StringBuilder();
    //        TextWriter featureFileWriter = null;
    //        TextWriter logFileWriter = null;
    //        int featureIndex = 0;
    //        int isosResultIndex = 0;
    //        ushort lcScanSpan = 80;
    //        ushort imsScanSpan = 30;
    //        int peaksProcessedCounter = 0;
    //        int totalLCFrames = thisUimfRun.GetNumFrames();
    //        int totalIMSScans = thisUimfRun.GetNumMSScans();

    //        ushort intensityCutoff = 500;
    //        double driftToleranceInMilliseconds = 0.5; //milliseconds

    //        ushort netToleranceInScans = (ushort)(0.02 * totalLCFrames);
    //        ushort driftToleranceInScans = (ushort)(driftToleranceInMilliseconds / 0.16); //here 0.16 stands for the 160 microseconds that it takes to capture 1 scan.
    //        ushort massToleranceInPPM = 30;
    //        UIMFDriftTimeExtractor driftTimeExtractorTask = new UIMFDriftTimeExtractor();
    //        StreamReader peaksFileReader = null;
    //        int peaksSkipped = 0;

    //        try
    //        {
    //            peaksFileReader = new StreamReader(peakFile);
    //            string peaksLine = peaksFileReader.ReadLine();
    //            //this is done to get the frame pressures loaded and corrected using a smoother.
    //            fscc = new FrameSetCollectionCreator(thisUimfRun, 1, totalLCFrames, 1, 1);
    //            fscc.Create();

    //            //Run this in a separate thread as we need the drift times only after a while
    //            //this adds avgTOFlength and framePressureBack to each frame's object data; 
    //            Thread oThread = new Thread(new ThreadStart(this.getAndUpdateFramePressures));

    //            // Start the thread
    //            oThread.Start();

    //            featureFileWriter = new StreamWriter(outputFeatureFile);

    //            //write the header
    //            featureFileWriter.WriteLine("Feature_Index	Original_Index	Monoisotopic_Mass	Average_Mono_Mass	UMC_MW_Min	UMC_MW_Max	Scan_Start	Scan_End	Scan	IMS_Scan_Start	IMS_Scan_End	IMS_Scan	Decon2ls_Fit_Score	UMC_Member_Count	Max_Abundance	Abundance	Class_Rep_MZ	Class_Rep_Charge	Charge_Max	Drift_Time");


    //            logFileWriter = new StreamWriter(logFile);
    //            //hornConvolutor.DeconEngineHornParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();


    //            //http://en.wikipedia.org/wiki/AVL_tree
    //            //I am using a tree based structure here to store the details of the peaks that are in a feature as well as peaks that didn't translate into a feature
    //            //the latter is used so that I don't keep trying to pull out features from the same region of the file. Makes the algorithm run a lot faster

    //            //AVLTree<MSResultPeakWithLocation> peaksInAFeatureTree = new AVLTree<MSResultPeakWithLocation>(); //this is my biggest memory hog :)
    //            //AVLTree<MSResultPeakWithLocation> peaksNotDeisotopedTree = new AVLTree<MSResultPeakWithLocation>();

    //            //release all the peaks that were loaded from the file, since we've pushed them onto the stack.

    //            //The original intension here was to try some image analysis algorithms and hence the name
    //            IntensityToImageConverter imgProcessor = new IntensityToImageConverter();
    //            while (peaksLine != null)
    //            {

    //                //start with the current most intense peak,
    //                MSPeakResult mostIntensePeak = null;
    //                try
    //                {
    //                    mostIntensePeak = parse(peaksLine);
    //                }
    //                catch (Exception x)
    //                {
    //                    logFileWriter.Write("Parsing exception. Probably a line with text");
    //                    peaksLine = peaksFileReader.ReadLine();
    //                    peaksLine = peaksFileReader.ReadLine();
    //                    continue;
    //                }


    //                //just so that we don't print a 10GB log file. 
    //                if (peaksProcessedCounter % 1000 == 0)
    //                {
    //                    Console.Write(".");

    //                    string progressString = DateTime.Now + "\tpeakNum= \t" + peaksProcessedCounter + "\tpeakMZ= \t"
    //                        + mostIntensePeak.XValue + "\tpeakIntensity=\t" + mostIntensePeak.Height + "\tframe=\t" + mostIntensePeak.Frame_num + "\tScan=\t" + mostIntensePeak.Scan_num
    //                        + "\ttreeHeight=\t" + peaksInAFeatureTree.GetHeight() + "\tpeaksInTree=\t" + peaksInTheTreeCounter + "\tFeaturesFound=\t" + featureIndex;


    //                    logFileWriter.WriteLine(progressString);


    //                    logFileWriter.Flush();

    //                    //break here after processing 1000 peaks for now.
    //                    /*if (peaksProcessedCounter == 2000)
    //                    {
    //                        logFileWriter.Write("Done!!");
    //                        logFileWriter.Flush();
    //                        break;
    //                    }*/
    //                }

    //                //we're looking at stopping when we hit the lower intensity limit. 
    //                if (mostIntensePeak.Height < intensityCutoff)
    //                {
    //                    peaksLine = peaksFileReader.ReadLine();
    //                    peaksProcessedCounter++;
    //                    break;
    //                }

    //                //check if this peak is already present in any of our existing features.
    //                //if this peak is not in the list of processed peak, then we need to work through this one to see if there's a feature at this position
    //                if (!peaksInAFeatureTree.FindPeakWithinFeatures(mostIntensePeak, (ushort)mostIntensePeak.Frame_num, (ushort)mostIntensePeak.Scan_num, massToleranceInPPM, netToleranceInScans, driftToleranceInScans))
    //                {
    //                    if (peaksNotDeisotopedTree.FindPeakWithinFeatures(mostIntensePeak, (ushort)mostIntensePeak.Frame_num, (ushort)mostIntensePeak.Scan_num, massToleranceInPPM, netToleranceInScans, driftToleranceInScans))
    //                    {

    //                        if (peaksProcessedCounter % 500 == 0)
    //                        {
    //                            //logFileWriter.WriteLine("It's within tolerance and range of another peak that we couldn't deisotope, Let's skip it for now");
    //                            //logFileWriter.Flush();
    //                        }
    //                        peaksLine = peaksFileReader.ReadLine();
    //                        peaksProcessedCounter++;
    //                        peaksSkipped++;
    //                        continue;
    //                    }

    //                    ushort startFrame = (ushort)mostIntensePeak.Frame_num;
    //                    ushort startScan = (ushort)mostIntensePeak.Scan_num;
    //                    ushort maxIntensityFrameInMap = 0;
    //                    ushort maxIntensityScanInMap = 0;

    //                    fscc = new FrameSetCollectionCreator(thisUimfRun, startFrame, startFrame, 1, 1);
    //                    fscc.Create();

    //                    sscc = new ScanSetCollectionCreator(thisUimfRun, startScan, startScan, 1, 1);
    //                    sscc.Create();

    //                    thisUimfRun.CurrentScanSet = thisUimfRun.FrameSetCollection.FrameSetList[0];
    //                    thisUimfRun.CurrentScanSet = thisUimfRun.ScanSetCollection.ScanSetList[0];

    //                    //now let's deconvolute this data within +/- 5 Mz.
    //                    hornConvolutor.MinMZ = mostIntensePeak.XValue - 5;
    //                    hornConvolutor.MaxMZ = mostIntensePeak.XValue + 5;
    //                    hornConvolutor.IsMZRangeUsed = true;

    //                    double toleranceInMZ = mostIntensePeak.XValue * massToleranceInPPM / 1e6;
    //                    int[][] intensityMap = null;
    //                    int startLc = startFrame - lcScanSpan;
    //                    int endLc = startFrame + lcScanSpan;
    //                    int startIms = startScan - imsScanSpan;
    //                    int endIms = startScan + imsScanSpan;

    //                    if (startLc <= 0)
    //                    {
    //                        startLc = 1;
    //                    }

    //                    if (startIms < 0)
    //                    {
    //                        startIms = 0;
    //                    }

    //                    if (endLc >= totalLCFrames)
    //                    {
    //                        endLc = totalLCFrames - 1;
    //                    }

    //                    if (endIms >= totalIMSScans)
    //                    {
    //                        endIms = totalIMSScans - 1;
    //                    }

    //                    intensityMap = thisUimfRun.GetFramesAndScanIntensitiesForAGivenMz(startLc, endLc, 0, startIms, endIms, mostIntensePeak.XValue, toleranceInMZ);

    //                    int maxIntensity = getMax(intensityMap, out maxIntensityFrameInMap, out maxIntensityScanInMap);
    //                    ushort minimumScanNumber = 0;
    //                    ushort maximumScanNumber = 0;
    //                    ushort totalSummed = 0;
    //                    List<MSPeak> isotopicPeakList = null;
    //                    try
    //                    {
    //                        frameAndScanNumbers = new Dictionary<ushort, List<ushort>>(lcScanSpan * 2);
    //                        List<MSPeakResult> peaksForCurveFitting = imgProcessor.getFrameAndScanNumberListFromIntensityMap(intensityMap, maxIntensity, 0.1f, (ushort)maxIntensityFrameInMap, (ushort)maxIntensityScanInMap, startFrame, startScan, frameAndScanNumbers, out minimumScanNumber, out maximumScanNumber, out totalSummed);
    //                        if (frameAndScanNumbers.Keys.Count != 0)
    //                        {
    //                            thisUimfRun.GetMassSpectrum(frameAndScanNumbers, hornConvolutor.MinMZ, hornConvolutor.MaxMZ);
    //                            peakDetector.Execute(thisUimfRun.ResultCollection);
    //                            hornConvolutor.Execute(thisUimfRun.ResultCollection);
    //                            driftTimeExtractorTask.Execute(thisUimfRun.ResultCollection);
    //                            isotopicPeakList = findPeakListClosestIsotopicDistribution(mostIntensePeak, thisUimfRun.ResultCollection.IsosResultBin, out isosResultIndex);
    //                        }
    //                    }
    //                    catch (IndexOutOfRangeException rangeException)
    //                    {
    //                        logFileWriter.WriteLine("Had a rangeException at " + rangeException.Message);
    //                        logFileWriter.WriteLine(mostIntensePeak.XValue + "\t" + mostIntensePeak.Frame_num.ToString() + " \t" + mostIntensePeak.Scan_num.ToString());
    //                        //logFileWriter.Flush();
    //                    }

    //                    if (isotopicPeakList != null)
    //                    {
    //                        int minimumFrameNumber = 0;
    //                        int maximumFrameNumber = 0;
    //                        getMinMax(frameAndScanNumbers.Keys.ToList<ushort>(), out minimumFrameNumber, out maximumFrameNumber);

    //                        UIMFIsosResult matchingIsosResult = (UIMFIsosResult)thisUimfRun.ResultCollection.IsosResultBin[isosResultIndex];

    //                        if (!peaksInAFeatureTree.FindPeakWithinFeatures(isotopicPeakList[0], startFrame, startScan, massToleranceInPPM, netToleranceInScans, driftToleranceInScans))
    //                        {
    //                            featureIndex++;
    //                            //there may be a better way than starting a new thread everytime. Maybe a thread pool. but we'll attack that later.

    //                           writeFeatureToFile(featureIndex, matchingIsosResult, minimumFrameNumber, maximumFrameNumber, minimumScanNumber, maximumScanNumber, startFrame, startScan, featureFileWriter, totalSummed);

    //                            //oThread = new Thread(() => writeFeatureToFile(featureIndex, matchingIsosResult, minimumFrameNumber, maximumFrameNumber, minimumScanNumber, maximumScanNumber, startFrame, startScan, featureFileWriter, totalSummed));
    //                            //// Start the thread to write the data to the output file. 
    //                            //oThread.Start();
    //                            ////you want to process this list starting from the middle and moving left and right, 
    //                            //this is to ensure that the binary tree insertions are balanced since the peaks in an 
    //                            //isotopic profile are going to be in increasing order of mzs. 
    //                            int midPoint = isotopicPeakList.Count / 2;

    //                            for (int peakIndex = midPoint; peakIndex >= 0; peakIndex--)
    //                            {
    //                                addPeakToFeatureTree(isotopicPeakList[peakIndex], frameAndScanNumbers, peaksInAFeatureTree, startFrame, startScan);
    //                            }

    //                            for (int peakIndex = midPoint + 1; peakIndex < isotopicPeakList.Count; peakIndex++)
    //                            {
    //                                addPeakToFeatureTree(isotopicPeakList[peakIndex], frameAndScanNumbers, peaksInAFeatureTree, startFrame, startScan);
    //                            }
    //                        }
    //                        else
    //                        {
    //                            //this is else part if the feature is found,we can simply ignore it. 
    //                        }
    //                    }
    //                    else
    //                    {
    //                        //I want to add it to my other tree, for faster processing
    //                        MSResultPeakWithLocation tempPeak = new MSResultPeakWithLocation(mostIntensePeak);
    //                        peaksNotDeisotopedTree.Add(tempPeak);
    //                    }
    //                }
    //                else
    //                {
    //                    //logFileWriter.WriteLine("Peak was processed before.");
    //                    peaksSkipped++;
    //                }
    //                peaksProcessedCounter++;
    //                //if (peaksProcessedCounter % 100 == 0)
    //                //{
    //                //    logFileWriter.WriteLine(DateTime.Now + "\tprocessing peak number " + peaksProcessedCounter);
    //                //    logFileWriter.WriteLine("Peak mz = " + mostIntensePeak.XValue + "; intensity = " + mostIntensePeak.Height);
    //                //    logFileWriter.WriteLine("Peak was found at frame = " + mostIntensePeak.Frame_num.ToString() + " and scan = " + mostIntensePeak.Scan_num.ToString());
    //                //    logFileWriter.Flush();


    //                //    logFileWriter.WriteLine("Height of binary tree = " + peaksInAFeatureTree.GetHeight().ToString());
    //                //    logFileWriter.WriteLine("Features found= " + featureIndex.ToString() + "; Peaks in Tree= " + peaksInTheTreeCounter.ToString());
    //                //    logFileWriter.WriteLine("Peaks skipped = " + peaksSkipped);
    //                //    logFileWriter.WriteLine("**********************************************");
    //                //    //logFileWriter.Flush();
    //                //}

    //                peaksLine = peaksFileReader.ReadLine();
    //            }
    //        }
    //        finally
    //        {

    //            if (featureFileWriter != null)
    //            {
    //                featureFileWriter.Flush();
    //                featureFileWriter.Close();
    //            }
    //            thisUimfRun.Close();

    //            if (logFileWriter != null)
    //            {
    //                st.Stop();
    //                TimeSpan ts = st.Elapsed;
    //                logFileWriter.WriteLine("**********************************************");
    //                logFileWriter.Write("Time taken to complete the deisotoping on a large file " +
    //                    ts.Hours.ToString() + ":" + ts.Minutes.ToString() + ":" + ts.Seconds.ToString());
    //                logFileWriter.WriteLine("\n**********************************************");

    //                logFileWriter.Flush();
    //                logFileWriter.Close();
    //            }
    //            if (peaksFileReader != null)
    //            {
    //                peaksFileReader.Close();
    //            }
    //        }
    //    }

    //    private void writeFeatureToFile(int featureIndex, UIMFIsosResult matchingIsosResult, int minFrameNum, int maxFrameNum, int minScanNum, int maxScanNum, int startFrame, int startScan, TextWriter featureFile, ushort totalSummed)
    //    {
    //        float averageIntensity = (float)matchingIsosResult.IsotopicProfile.IntensityAggregate / totalSummed;
    //        StringBuilder featureOutputStringBuilder = new StringBuilder();
    //        //umc.OriginalIndex = isotopicPeakList[0].DataIndex
    //        featureOutputStringBuilder.Append(featureIndex + "\t");
    //        featureOutputStringBuilder.Append(featureIndex + "\t");
    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.MonoIsotopicMass.ToString("0.00000") + "\t");
    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.AverageMass.ToString("0.00000") + "\t");

    //        //this is to write place holders for the average UMC Min and UMC max mass
    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.MonoIsotopicMass.ToString("0.00000") + "\t");
    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.MonoIsotopicMass.ToString("0.00000") + "\t");

    //        featureOutputStringBuilder.Append(minFrameNum.ToString() + "\t");
    //        featureOutputStringBuilder.Append(maxFrameNum.ToString() + "\t");
    //        featureOutputStringBuilder.Append(startFrame + "\t");


    //        featureOutputStringBuilder.Append(minScanNum.ToString() + "\t");
    //        featureOutputStringBuilder.Append(maxScanNum.ToString() + "\t");
    //        featureOutputStringBuilder.Append(startScan + "\t");

    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.Score.ToString("0.00000") + "\t");
    //        featureOutputStringBuilder.Append(totalSummed + "\t"); //this represents UMC member count for now

    //        featureOutputStringBuilder.Append(averageIntensity.ToString("0.00000") + "\t");  // comparable to 'max_abundance'
    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.IntensityAggregate.ToString("0.00000") + "\t");   //comparable to 'sum_abundance'

    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.MonoPeakMZ.ToString("0.00000") + "\t");   //comparable to 'class_rep_mz' in traditional UMC output
    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.ChargeState.ToString("0.00000") + "\t");
    //        featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.ChargeState.ToString("0.00000") + "\t");  // comparable to 'charge_max'

    //        featureOutputStringBuilder.Append(matchingIsosResult.DriftTime.ToString("0.0000"));

    //        featureOutputStringBuilder.Append(Environment.NewLine);

    //        //featureOutputStringBuilder.Append("0\n");
    //        /*featureOutputStringBuilder.Append("LCFitScore\t");
    //        featureOutputStringBuilder.Append("AverFit\t");
    //        featureOutputStringBuilder.Append("MemPerc\t");
    //        featureOutputStringBuilder.Append("CombScore\n");*/

    //        featureFile.Write(featureOutputStringBuilder.ToString());
    //        featureFile.Flush();

    //    }

    //    //private void addPeakToFeatureTree(MSPeak msPeakInIsotopicProfile, Dictionary<ushort, List<ushort>> frameScanLoc, AVLTree<MSResultPeakWithLocation> peaksInAFeatureTree, int startFrame, int startScan)
    //    //{
    //    //    BinaryTreeNode<MSResultPeakWithLocation> featureNode = peaksInAFeatureTree.FindFeatureWithGivenMass(msPeakInIsotopicProfile.XValue, 20);
    //    //    if (featureNode != null)
    //    //    {
    //    //        //that means we found something that is exactly the same mass. let's simply update the frame scans map for this peak
    //    //        featureNode.Value.UpdateFrameScansRange(frameScanLoc);
    //    //    }
    //    //    else
    //    //    {
    //    //        //there's nothing with a mass close to ours and we have to insert this into the free, based on its mass.
    //    //        peaksInTheTreeCounter++;
    //    //        peaksInAFeatureTree.Add(new MSResultPeakWithLocation(msPeakInIsotopicProfile, frameScanLoc, startFrame, startScan));
    //    //        //logFileWriter.Write(msPeakInIsotopicProfile.XValue.ToString() + ";");
    //    //    }
    //    //}

    //    private void getMinMax(List<ushort> frameNumbers, out int minimum, out int maximum)
    //    {
    //        maximum = frameNumbers[0];
    //        minimum = frameNumbers[0];

    //        for (int i = 1; i < frameNumbers.Count; i++)
    //        {
    //            if (frameNumbers[i] < minimum)
    //            {
    //                minimum = frameNumbers[i];
    //            }
    //            else if (frameNumbers[i] > maximum)
    //            {
    //                maximum = frameNumbers[i];
    //            }
    //        }


    //    }
    //    private List<MSPeak> findPeakListClosestIsotopicDistribution(Peak mostIntensePeak, IList<IsosResult> isosResults, out int isosResultIndex)
    //    {
    //        double minMzDistance = Double.MaxValue;
    //        int index = 0;
    //        isosResultIndex = -1;

    //        foreach (IsosResult item in isosResults)
    //        {
    //            foreach (MSPeak msPeak in item.IsotopicProfile.Peaklist)
    //            {
    //                double absDiff = Math.Abs(mostIntensePeak.XValue - msPeak.XValue);

    //                if (absDiff <= minMzDistance)
    //                {
    //                    isosResultIndex = index;
    //                    minMzDistance = absDiff;
    //                }
    //            }
    //            index++;
    //        }

    //        if (isosResultIndex != -1)
    //        {
    //            return isosResults[isosResultIndex].IsotopicProfile.Peaklist;
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }

    //    //private void addPeakListToTree(BinaryTree<Peak> peakTree, List<Peak> peakList)
    //    //{
    //    //    for (int i = 0; i < peakList.Count; i++)
    //    //    {
    //    //        peakTree.Add(peakList[i]);
    //    //    }
    //    //}


    //    public void ExecuteWorkflowGordon(DeconTools.Backend.Core.Run run)
    //    {

    //        UIMFRun uimfRun = (UIMFRun)run;

    //        //for each frame

    //        foreach (var frame in uimfRun.FrameSetCollection.FrameSetList)
    //        {
    //            uimfRun.CurrentScanSet = frame;


    //            // detect all peaks in frame
    //            List<MSPeakResult> masterPeakList = getAllPeaksInFrame(uimfRun, NumMSScansToSumWhenBuildingMasterPeakList);

    //            // sort peaks
    //            masterPeakList.Sort(delegate(MSPeakResult peak1, MSPeakResult peak2)
    //            {
    //                return peak2.MSPeak.Height.CompareTo(peak1.MSPeak.Height);
    //            });


    //            // for each peak
    //            int peakCounter = 0;
    //            int peaksThatGenerateAChromatogram = 0;
    //            foreach (var peak in masterPeakList)
    //            {
    //                peakCounter++;

    //                //if (peakCounter > 500) break;
    //                if (peak.MSPeak.Height < 1000) break;


    //                string peakFate = "Undefined";

    //                bool peakResultAlreadyIncludedInChromatogram = (peak.ChromID != -1);
    //                if (peakResultAlreadyIncludedInChromatogram)
    //                {
    //                    peakFate = "Chrom_Already";

    //                    displayPeakInfoAndFate(peak, peakFate);


    //                    continue;
    //                }
    //                else
    //                {
    //                    peakFate = "CHROM";

    //                    //bool peakResultAlreadyFoundInAnMSFeature = findPeakWithinMSFeatureResults(run.ResultCollection.ResultList, peakResult, scanTolerance);
    //                    //if (peakResultAlreadyFoundInAnMSFeature)
    //                    //{
    //                    //    peakFate = "MSFeature_Already";
    //                    //}
    //                    //else
    //                    //{
    //                    //    peakFate = "CHROM";
    //                    //}



    //                }

    //                peaksThatGenerateAChromatogram++;
    //                PeakChrom chrom = new BasicPeakChrom();

    //                // create drift profile from raw data
    //                double driftTimeProfileMZTolerance = this.DriftTimeProfileExtractionPPMTolerance * peak.MSPeak.XValue / 1e6;
    //                uimfRun.GetDriftTimeProfile(frame.PrimaryFrame, run.MinScan, run.MaxScan, peak.MSPeak.XValue, driftTimeProfileMZTolerance);

    //                bool driftTimeProfileIsEmpty = (uimfRun.XYData.Xvalues == null);
    //                if (driftTimeProfileIsEmpty)
    //                {
    //                    addPeakToProcessedPeakList(peak);
    //                    peakFate = peakFate + " DriftProfileEmpty";
    //                    displayPeakInfoAndFate(peak, peakFate);

    //                    continue;
    //                }

    //                chrom.XYData = uimfRun.XYData;


    //                // smooth drift profile
    //                chrom.XYData = ChromSmoother.Smooth(uimfRun.XYData);

    //                // detect peaks in chromatogram
    //                chrom.PeakList = this.ChromPeakDetector.FindPeaks(chrom.XYData, 0, 0);

    //                if (chrom.PeakDataIsNullOrEmpty)
    //                {
    //                    addPeakToProcessedPeakList(peak);
    //                    peakFate = peakFate + " NoChromPeaksDetected";
    //                    displayPeakInfoAndFate(peak, peakFate);


    //                    continue;
    //                }

    //                // find which drift profile peak,  if any, the source peak is a member of
    //                Peak chromPeak = chrom.GetChromPeakForGivenSource(peak);
    //                if (chromPeak == null)
    //                {
    //                    addPeakToProcessedPeakList(peak);
    //                    peakFate = peakFate + " TargetChromPeakNotFound";
    //                    displayPeakInfoAndFate(peak, peakFate);


    //                    continue;
    //                }


    //                // find other peaks in the master peaklist that are members of the found drift profile peak
    //                // tag these peaks with the source peak's ID
    //                double peakWidthSigma = chromPeak.Width / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)

    //                int minScanForChrom = (int)Math.Floor(chromPeak.XValue - peakWidthSigma * 4);
    //                int maxScanForChrom = (int)Math.Floor(chromPeak.XValue + peakWidthSigma * 4);

    //                double peakToleranceInMZ = driftTimeProfileMZTolerance;
    //                double minMZForChromFilter = peak.MSPeak.XValue - peakToleranceInMZ;
    //                double maxMZForChromFilter = peak.MSPeak.XValue + peakToleranceInMZ;


    //                chrom.ChromSourceData = (from n in masterPeakList
    //                              where n.Scan_num >= minScanForChrom && n.Scan_num <= maxScanForChrom &&
    //                                  n.MSPeak.XValue >= minMZForChromFilter && n.MSPeak.XValue < maxMZForChromFilter
    //                              select n).ToList();

    //                foreach (var item in chrom.ChromSourceData)
    //                {
    //                    item.ChromID = peak.PeakID;
    //                }

    //                displayPeakInfoAndFate(peak, peakFate);



    //            }

    //            Console.WriteLine("peaksProcessed = " + peakCounter);
    //            Console.WriteLine("peaks generating a chrom = " + peaksThatGenerateAChromatogram);



    //        }











    //        // generate MS by integrating over drift profile peak

    //        // find MS peaks within range

    //        // find MS Features. 

    //        // find MS Feature for which the source peak is a member of.  


    //        // if found, add it. 
    //        // And, for each MS peaks of the found MS Feature,  mark all peaks of the masterpeak list that correspond to the found drift time peak and m/z

    //    }

    //    private void displayPeakInfoAndFate(MSPeakResult peak, string peakFate)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        sb.Append(peak.PeakID);
    //        sb.Append('\t');
    //        sb.Append(peak.Scan_num);
    //        sb.Append('\t');
    //        sb.Append(peak.MSPeak.XValue);
    //        sb.Append('\t');
    //        sb.Append(peak.MSPeak.Height);
    //        sb.Append('\t');
    //        sb.Append(peakFate);
    //        sb.Append('\t');
    //        sb.Append(peak.ChromID);
    //        Console.WriteLine(sb.ToString());

    //    }


    //    private void printAsAMatrix(int[][] intensityVals)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        for (int i = 0; i < intensityVals.Length; i++)
    //        {

    //            for (int j = 0; j < intensityVals[i].Length; j++)
    //            {
    //                sb.Append(intensityVals[i][j].ToString() + ",");
    //            }

    //            sb.Append("\n");


    //        }

    //        Console.WriteLine(sb.ToString());


    //    }

    //    private void addPeakToProcessedPeakList(MSPeakResult peak)
    //    {
    //        peak.ChromID = peak.PeakID;
    //        //this.processedMSPeaks.Add(peak);
    //    }

    //    private List<MSPeakResult> getAllPeaksInFrame(UIMFRun uimfRun, int numIMSScansToSum)
    //    {
    //        if (uimfRun.ResultCollection.MSPeakResultList != null)
    //        {
    //            uimfRun.ResultCollection.MSPeakResultList.Clear();
    //        }

    //        ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(uimfRun, numIMSScansToSum, 1);
    //        sscc.Create();

    //        foreach (var scan in uimfRun.ScanSetCollection.ScanSetList)
    //        {
    //            uimfRun.CurrentScanSet = scan;
    //            msgen.Execute(uimfRun.ResultCollection);
    //            MasterPeakListPeakDetector.Execute(uimfRun.ResultCollection);

    //        }

    //        return uimfRun.ResultCollection.MSPeakResultList;

    //    }

    //    private int getMax(int[][] values, out ushort x, out ushort y)
    //    {
    //        int max = 0;
    //        x = 0;
    //        y = 0;
    //        for (int i = 0; i < values.Length; i++)
    //        {
    //            for (int j = 0; j < values[i].Length; j++)
    //            {
    //                if (values[i][j] >= max)
    //                {
    //                    max = values[i][j];
    //                    x = (ushort)i;
    //                    y = (ushort)j;
    //                }
    //            }

    //        }

    //        return max;
    //    }

    //    #endregion

    //    public override void InitializeRunRelatedTasks()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override WorkflowParameters WorkflowParameters
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //        set
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
    //}
}
/*


int frameNum = 1;
int frameSetIndex = 0;
UIMFRun uimfRun = (UIMFRun)run;
ScanSet startScanSet;
FrameSet endFrameSet;



uimfRun.CurrentScanSet = uimfRun.FrameSetCollection.FrameSetList[frameSetIndex];
ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(uimfRun, 1, 1);
sscc.Create();
peakDetector = new DeconToolsPeakDetector(4, 3, Globals.PeakFitType.QUADRATIC, true);
            
int startScan = 0;
int startFrame = uimfRun.CurrentScanSet.PrimaryScanNumber;
int endScan = 0;
int endFrame = 0;

for (int i = 0; i < uimfRun.ScanSetCollection.ScanSetList.Count; i++)
{
             
    //start with an individual scan
    uimfRun.CurrentScanSet = uimfRun.ScanSetCollection.ScanSetList[i];

    //Console.WriteLine(uimfRun.CurrentScanSet.PrimaryScanNumber);

    msgen.Execute(uimfRun.ResultCollection);
   // uimfRun.GetMassSpectrum(uimfRun.CurrentScanSet);
    List<Peak> peaks = peakDetector.FindPeaks(uimfRun.XYData, 0, 5000);
    
    //let's say we need at least 3 peaks in a spectrum to even consider summing
    //or deisotooping it
    if ( peaks.Count > 3 ){
        startScan = uimfRun.CurrentScanSet.PrimaryScanNumber;
        startScanSet = uimfRun.CurrentScanSet;
        endScan = startScan;
        Console.WriteLine("Peaks were found in this scan " + uimfRun.CurrentScanSet.PrimaryScanNumber.ToString());

        //start with the highest peak in the spectrum
        //first sort the data based on intensity
        //peaks.Sort(delegate(DeconTools.Backend.Core.Peak peak1, DeconTools.Backend.Core.Peak peak2)
        //{
          //  return peak2.Height.CompareTo(peak1.Height);
       // });


        //we might have to iterate through all the peaks here. TBD
        Peak mzPeak = peaks[0];
                    
        //advance the scan set till we don't see this peak, 
        if (i < uimfRun.ScanSetCollection.ScanSetList.Count - 1)
        {
            uimfRun.CurrentScanSet = uimfRun.ScanSetCollection.ScanSetList[i + 1];


            // you also want to store the results of the peak finding on consequent scans, so that 
            // we don't have to do this again. But we'll figure out a mechanism for that piece later.

            while (peakPresent(mzPeak, uimfRun.CurrentScanSet, uimfRun))
            {
                uimfRun.CurrentScanSet = uimfRun.ScanSetCollection.ScanSetList[i + 1];
                endScan = i + 1;
            }

            if (endScan - startScan > 1)
            {
                //this means it's found it more than 1 scans,
                //so we need to figure a way to sum

            }
            else
            {
                //it's not present in 3 consecutive IMS scans
                //and as a result we don't have to deisotope that peak

                //we should also keep track of how many peaks to deisotope
                //within a given scan.

                //that way if we don't have 3 peaks to form the cluster
                //we can completely skip that scan

            }
            //so now we've found the end scan for this feature
            //endScan = i;

            //we need to do this same step along the LC dimension to find the endFrame and the endFrame
            uimfRun.CurrentScanSet = uimfRun.FrameSetCollection.FrameSetList[
                ++frameSetIndex];
            uimfRun.CurrentScanSet = startScanSet;
            while (peakPresent(mzPeak, uimfRun.CurrentScanSet, uimfRun)){
                uimfRun.CurrentScanSet = uimfRun.FrameSetCollection.FrameSetList[
                ++frameSetIndex];
            }

            endFrame = uimfRun.CurrentScanSet.PrimaryScanNumber - 1;


            //now we've got the endFrame for this feature

            //Let's generate a mass spectrum for this feature, across the scans and frames
            //and the expected mz with a tolerance 


            //and now lets' deconvolute this newly generated spectra which is a sum of
            //arbitrary scans and frames both across IMS and LC dimensions


            //Once again we'll generate a peak list first and capture it, 


            //we also need to now remove the peaks from this deisotoped data and reduce 
            //the peak lists that were generated from the other scans. so that no duplicate
            //processing is done and a peak does not go into two different features.


            //now continue this for the next intense peak that was not removed
            //in the previous steps.


        }


    }
    else{
        Console.WriteLine("No peaks were found in this scan " );
    }

}
 * */
