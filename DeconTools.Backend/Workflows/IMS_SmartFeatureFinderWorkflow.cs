using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Data.Structures;
using System.Data.SQLite;
using DeconTools.Backend.Utilities.Converters;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Threading;

//using CenterSpace.NMath.Matrix;
//using CenterSpace.NMath.Analysis;
//using CenterSpace.NMath.Core;


namespace DeconTools.Backend.Workflows
{
    public class IMS_SmartFeatureFinderWorkflow : IWorkflow
    {

        public static string sarcUIMFFile1 = "C:\\ProteomicsSoftwareTools\\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";

        int peaksInTheTreeCounter = 0;
        DeconToolsPeakDetector peakDetector;
        HornDeconvolutor hornConvolutor;
        const int TENPOWSIX = 1000000;
        UIMFRun thisUimfRun;


        #region Constructors
        public IMS_SmartFeatureFinderWorkflow()
        {
            InitializeWorkflow();
        }
        #endregion

        #region Properties

        DeconToolsPeakDetector MasterPeakListPeakDetector { get; set; }

        public DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother ChromSmoother { get; set; }
        public ChromPeakDetector ChromPeakDetector { get; set; }

        public ChromatogramGenerator ChromGenerator { get; set; }


        public I_MSGenerator msgen { get; set; }

        public double DriftTimeProfileExtractionPPMTolerance { get; set; }

        public I_MSGenerator MSgen { get; set; }
        public DeconToolsPeakDetector MSPeakDetector { get; set; }
        public IDeconvolutor Deconvolutor { get; set; }

        public int NumMSScansToSumWhenBuildingMasterPeakList { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region IWorkflow Members

        public string Name { get; set; }

        public void InitializeWorkflow()
        {

            NumMSScansToSumWhenBuildingMasterPeakList = 3;

            MasterPeakListPeakDetector = new DeconToolsPeakDetector();
            MasterPeakListPeakDetector.PeakBackgroundRatio = 4;
            MasterPeakListPeakDetector.SigNoiseThreshold = 3;
            MasterPeakListPeakDetector.IsDataThresholded = false;
            MasterPeakListPeakDetector.StorePeakData = true;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            msgen = msgenFactory.CreateMSGenerator(Globals.MSFileType.PNNL_UIMF);
            this.DriftTimeProfileExtractionPPMTolerance = 15;

            this.ChromSmoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother();
            this.ChromSmoother.LeftParam = 11;
            this.ChromSmoother.RightParam = 11;
            this.ChromSmoother.Order = 2;

            this.peakDetector = new DeconToolsPeakDetector(4,3, Globals.PeakFitType.QUADRATIC, true);


            this.ChromGenerator = new ChromatogramGenerator();

               hornConvolutor = new HornDeconvolutor();
        }

        private bool peakPresent(IPeak peak, ScanSet scanSet,UIMFRun run)
        {
            bool found = false;
            //basically we have to check if the peak is present in the given scan set
            //so we have to detect the peak
            run.CurrentScanSet = scanSet;
            msgen.Execute(run.ResultCollection);
            List<IPeak> peaks = peakDetector.FindPeaks(run.XYData, 0, 5000);
            
            if (peaks.Count > 0)
            {
                //binary search the peak mz within a certain tolerance in this list
                //never mind for now
                for (int i = 0; i < peaks.Count; i++)
                {
                    if (Math.Abs(peak.XValue - peaks[i].XValue)* TENPOWSIX/peak.XValue <= 20)
                    {
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }

        
        private IPeak getMaxPeak(List<IPeak> peakList)
        {
            IPeak maxPeak = null;
            if (peakList.Count != 0)
            {
                maxPeak = peakList[0];
                for (int i = 0; i < peakList.Count; i++)
                {
                    if (peakList[i].Height > maxPeak.Height)
                    {
                        maxPeak = peakList[i];
                    }

                }
            }

            return maxPeak;

        }

        private MSPeakResult parse(string peaksline)
        {
            MSPeakResult peakresult = new MSPeakResult();
            string [] processedLine = peaksline.Split('\t');
            if (processedLine.Length < 7)
            {
                throw new System.IO.IOException("Trying to import peak data into UIMF data object, but not enough columns are present in the source text file");
            }

            peakresult.PeakID = Convert.ToInt32(processedLine[0]);
            peakresult.Frame_num = Convert.ToInt32(processedLine[1]);
            peakresult.Scan_num = Convert.ToInt32(processedLine[2]);
            peakresult.MSPeak = new DeconTools.Backend.Core.MSPeak();
            peakresult.MSPeak.XValue = Convert.ToDouble(processedLine[3]);
            peakresult.MSPeak.Height = Convert.ToSingle(processedLine[4]);
            peakresult.MSPeak.Width = Convert.ToSingle(processedLine[5]);
            peakresult.MSPeak.SN = Convert.ToSingle(processedLine[6]);

            if (processedLine.Length > 7)
            {
                peakresult.MSPeak.MSFeatureID = Convert.ToInt32(processedLine[7]);
            }

            return peakresult;


        }

        private void getAndUpdateFramePressures ( )
        {
            thisUimfRun.GetFrameDataAllFrameSets();
            thisUimfRun.SmoothFramePressuresInFrameSets();
            Console.WriteLine("Finished updating frame pressures from the other thread");
        }

        public void ExecuteWorkflow(DeconTools.Backend.Core.Run run)
        {
            Stopwatch st = new Stopwatch();
            st.Start();

            thisUimfRun = (UIMFRun)run;
            string outputFeatureFile = @"\\protoapps\UserData\Shah\TestFiles\BSA_0pt01_2_20Sep10_Cheetah_10-08-05_0000_LCMSFeatures.txt";
            string testPeakFile = @"\\protoapps\UserData\Shah\TestFiles\BSA_0pt01_2_20Sep10_Cheetah_10-08-05_0000_peaks.txt";
            string logFile = @"\\protoapps\UserData\Shah\TestFiles\BSA_0pt01_2_20Sep10_Cheetah_10-08-05_0000_log.txt";
            FrameSetCollectionCreator fscc;
            ScanSetCollectionCreator sscc;
            Dictionary<ushort, List<ushort>> frameAndScanNumbers = null;
            StringBuilder featureOutputStringBuilder = new StringBuilder();
            TextWriter featureFileWriter = null;
            TextWriter logFileWriter = null;
            int featureIndex = 0;
            int isosResultIndex = 0;
            ushort lcScanSpan = 80;
            ushort imsScanSpan = 40;
            int peaksProcessedCounter = 0;
            int totalLCFrames = thisUimfRun.GetNumFrames();
            int totalIMSScans = thisUimfRun.GetNumScansPerFrame();

            ushort intensityCutoff = 500;

            double driftToleranceInMilliseconds = 0.6 ; //milliseconds

            ushort netToleranceInScans = (ushort) (0.02 * totalLCFrames);
            ushort driftToleranceInScans = (ushort) (driftToleranceInMilliseconds / 0.16); //here 0.16 stands for the 160 microseconds that it takes to capture 1 scan.
            ushort massToleranceInPPM = 30;
            UIMFDriftTimeExtractor driftTimeExtractorTask = new UIMFDriftTimeExtractor();
            StreamReader peaksFileReader = null;

            try
            {
                peaksFileReader = new StreamReader(testPeakFile);
                string peaksLine = peaksFileReader.ReadLine();
                //this is done to get the frame pressures loaded and corrected using a smoother.
                fscc = new FrameSetCollectionCreator(thisUimfRun, 1, totalLCFrames, 1, 1);
                fscc.Create();

                //Run this in a separate thread as we need the drift times only after a while
                //this adds avgTOFlength and framePressureBack to each frame's object data; 
                Thread oThread = new Thread(new ThreadStart(this.getAndUpdateFramePressures));

                // Start the thread
                oThread.Start();
                
                featureFileWriter = new StreamWriter(outputFeatureFile);
                logFileWriter = new StreamWriter(logFile);
                hornConvolutor.DeconEngineHornParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();


                //http://en.wikipedia.org/wiki/AVL_tree
                //I am using a tree based structure here to store the details of the peaks that are in a feature as well as peaks that didn't translate into a feature
                //the latter is used so that I don't keep trying to pull out features from the same region of the file. Makes the algorithm run a lot faster

                AVLTree<MSResultPeakWithLocation> peaksInAFeatureTree = new AVLTree<MSResultPeakWithLocation>(); //this is my biggest memory hog :)
                AVLTree<MSResultPeakWithLocation> peaksNotDeisotopedTree = new AVLTree<MSResultPeakWithLocation>();

                //release all the peaks that were loaded from the file, since we've pushed them onto the stack.

                //The original intension here was to try some image analysis algorithms and hence the name
                IntensityToImageConverter imgProcessor = new IntensityToImageConverter();
                while ( peaksLine != null)
                {

                    //start with the current most intense peak,
                    MSPeakResult mostIntensePeak = null;
                    try
                    {
                        mostIntensePeak = parse(peaksLine); 
                    }
                    catch(Exception x){
                        logFileWriter.Write("Parsing exception. Probably a line with text");
                        peaksLine = peaksFileReader.ReadLine();
                        peaksLine = peaksFileReader.ReadLine();
                        continue;
                    }


                    //just so that we don't print a 10GB log file. 
                    if (peaksProcessedCounter % 3000 == 0)
                    {
                        logFileWriter.WriteLine("processing peak number " + peaksProcessedCounter);
                        logFileWriter.WriteLine("Peak mz = " + mostIntensePeak.XValue + "; intensity = " + mostIntensePeak.Height);
                        logFileWriter.WriteLine("Peak was found at frame = " + mostIntensePeak.Frame_num.ToString() + " and scan = " + mostIntensePeak.Scan_num.ToString());
                    }

                    //we're looking at stopping when we hit the lower intensity limit. 
                    if (mostIntensePeak.Height < intensityCutoff)
                    {
                        peaksLine = peaksFileReader.ReadLine();
                        peaksProcessedCounter++;
                        break;
                    }

                    //check if this peak is already present in any of our existing features.
                    //if this peak is not in the list of processed peak, then we need to work through this one to see if there's a feature at this position
                    if (!peaksInAFeatureTree.FindPeakWithinFeatures(mostIntensePeak, (ushort) mostIntensePeak.Frame_num, (ushort) mostIntensePeak.Scan_num, massToleranceInPPM, netToleranceInScans, driftToleranceInScans))
                    {

                        if (peaksNotDeisotopedTree.FindPeakWithinFeatures(mostIntensePeak, (ushort)mostIntensePeak.Frame_num, (ushort)mostIntensePeak.Scan_num, massToleranceInPPM, netToleranceInScans, driftToleranceInScans))
                        {
                            logFileWriter.WriteLine("It's within tolerance and range of another peak that we couldn't deisotope, Let's skip it for now");
                            peaksLine = peaksFileReader.ReadLine();
                            peaksProcessedCounter++;
                            continue;
                        }

                        ushort startFrame = (ushort) mostIntensePeak.Frame_num;
                        ushort startScan = (ushort) mostIntensePeak.Scan_num;
                        ushort maxIntensityFrameInMap = 0;
                        ushort maxIntensityScanInMap = 0;

                        fscc = new FrameSetCollectionCreator(thisUimfRun, startFrame, startFrame, 1, 1);
                        fscc.Create();

                        sscc = new ScanSetCollectionCreator(thisUimfRun, startScan, startScan, 1, 1);
                        sscc.Create();

                        thisUimfRun.CurrentFrameSet = thisUimfRun.FrameSetCollection.FrameSetList[0];
                        thisUimfRun.CurrentScanSet = thisUimfRun.ScanSetCollection.ScanSetList[0];

                        //now let's deconvolute this data within +/- 5 Mz.
                        hornConvolutor.DeconEngineHornParameters.MinMZ = mostIntensePeak.XValue - 5;
                        hornConvolutor.DeconEngineHornParameters.MaxMZ = mostIntensePeak.XValue + 5;
                        hornConvolutor.DeconEngineHornParameters.UseMZRange = true;

                        double toleranceInMZ = mostIntensePeak.XValue * massToleranceInPPM / 1e6;
                        int[][] intensityMap = null;
                        int startLc = startFrame - lcScanSpan;
                        int endLc = startFrame + lcScanSpan;
                        int startIms = startScan - imsScanSpan;
                        int endIms = startScan + imsScanSpan;

                        if (startLc <= 0)
                        {
                            startLc = 1;
                        }

                        if (startIms < 0)
                        {
                            startIms = 0;
                        }

                        if (endLc >= totalLCFrames )
                        {
                            endLc = totalLCFrames-1;
                        }

                        if (endIms >= totalIMSScans)
                        {
                            endIms = totalIMSScans-1;
                        }

                        intensityMap = thisUimfRun.GetFramesAndScanIntensitiesForAGivenMz(startLc, endLc, 0, startIms, endIms, mostIntensePeak.XValue, toleranceInMZ);

                        int maxIntensity = getMax(intensityMap, out maxIntensityFrameInMap, out maxIntensityScanInMap);
                        ushort minimumScanNumber = 0;
                        ushort maximumScanNumber = 0;
                        ushort totalSummed = 0;
                        List<MSPeak> isotopicPeakList = null;
                        try
                        {
                            frameAndScanNumbers = new Dictionary<ushort, List<ushort>>(lcScanSpan*2);
                            List<MSPeakResult> peaksForCurveFitting = imgProcessor.getFrameAndScanNumberListFromIntensityMap(intensityMap, maxIntensity, 0.1f, (ushort) maxIntensityFrameInMap, (ushort)maxIntensityScanInMap, startFrame, startScan, frameAndScanNumbers, out minimumScanNumber, out maximumScanNumber, out totalSummed);
                            if ( frameAndScanNumbers.Keys.Count != 0)
                            {
                                thisUimfRun.GetMassSpectrum(frameAndScanNumbers, hornConvolutor.DeconEngineHornParameters.MinMZ, hornConvolutor.DeconEngineHornParameters.MaxMZ);
                                peakDetector.Execute(thisUimfRun.ResultCollection);
                                hornConvolutor.Execute(thisUimfRun.ResultCollection);
                                driftTimeExtractorTask.Execute(thisUimfRun.ResultCollection);
                                isotopicPeakList = findPeakListClosestIsotopicDistribution(mostIntensePeak, thisUimfRun.ResultCollection.IsosResultBin, out isosResultIndex);
                            }

                        }
                        catch (IndexOutOfRangeException rangeException)
                        {
                            logFileWriter.WriteLine ( "Had a rangeException at " + rangeException.Message);
                            logFileWriter.WriteLine ( mostIntensePeak.XValue + "\t" + mostIntensePeak.Frame_num.ToString() + " \t" + mostIntensePeak.Scan_num.ToString());
                            logFileWriter.Flush();
                        }

                        if (isotopicPeakList != null)
                        {
                            int minimumFrameNumber = 0;
                            int maximumFrameNumber = 0; 
                            getMinMax(frameAndScanNumbers.Keys.ToList<ushort>(), out minimumFrameNumber, out maximumFrameNumber);

                            UIMFIsosResult matchingIsosResult = (UIMFIsosResult)thisUimfRun.ResultCollection.IsosResultBin[isosResultIndex];

                            if (!peaksInAFeatureTree.FindPeakWithinFeatures(isotopicPeakList[0], startFrame, startScan, massToleranceInPPM, netToleranceInScans, driftToleranceInScans))
                            {
                                featureIndex++;
                                //there may be a better way than starting a new thread everytime. Maybe a thread pool. but we'll attack that later.
                                oThread = new Thread(() => writeFeatureToFile(featureIndex, matchingIsosResult, minimumFrameNumber, maximumFrameNumber, minimumScanNumber, maximumScanNumber, startFrame, startScan, featureFileWriter, totalSummed));
                                // Start the thread to write the data to the output file. 
                                oThread.Start();
                                //you want to process this list starting from the middle and moving left and right, 
                                //this is to ensure that the binary tree insertions are balanced since the peaks in an 
                                //isotopic profile are going to be in increasing order of mzs. 
                                int midPoint = isotopicPeakList.Count / 2;

                                for (int peakIndex = midPoint; peakIndex >= 0; peakIndex--)
                                {
                                    addPeakToFeatureTree(isotopicPeakList[peakIndex], frameAndScanNumbers, peaksInAFeatureTree, startFrame, startScan);
                                }

                                for (int peakIndex = midPoint + 1; peakIndex < isotopicPeakList.Count; peakIndex++)
                                {
                                    addPeakToFeatureTree(isotopicPeakList[peakIndex], frameAndScanNumbers, peaksInAFeatureTree, startFrame, startScan);
                                }
                            }
                            else
                            {
                                //this is else part if the feature is found,we can simply ignore it. 
                            }
                        }
                        else
                        {
                            //I want to add it to my other tree, for faster processing
                            MSResultPeakWithLocation tempPeak = new MSResultPeakWithLocation(mostIntensePeak);
                            peaksNotDeisotopedTree.Add(tempPeak);
                        }
                    }
                  /*else
                    {
                        logFileWriter.WriteLine("Peak was processed before.");
                    }*/
                    peaksProcessedCounter++;
                    if (peaksProcessedCounter % 2000 == 0)
                    {
                        logFileWriter.WriteLine("Height of binary tree = " + peaksInAFeatureTree.GetHeight().ToString());
                        logFileWriter.WriteLine("Features found= " + featureIndex.ToString() + "; Peaks in Tree= "+ peaksInTheTreeCounter.ToString());
                        logFileWriter.WriteLine("**********************************************");
                    }
                    
                    peaksLine = peaksFileReader.ReadLine();
                   }
            }
            finally
            {
                st.Stop();
                TimeSpan ts = st.Elapsed;
                logFileWriter.WriteLine("**********************************************");
                logFileWriter.Write("Time taken to complete the deisotoping on a large file " + 
                    ts.Hours.ToString() + ":" + ts.Minutes.ToString() + ":" + ts.Seconds.ToString());
                logFileWriter.WriteLine("**********************************************");

                if (featureFileWriter != null)
                {
                    featureFileWriter.Close();
                }
                thisUimfRun.Close();

                if (logFileWriter != null)
                {
                    logFileWriter.Close();
                }
                if ( peaksFileReader != null){
                    peaksFileReader.Close();
                }
                
                
            }

        }

        private void writeFeatureToFile(int featureIndex, UIMFIsosResult matchingIsosResult, int minFrameNum, int maxFrameNum, int minScanNum, int maxScanNum, int startFrame, int startScan, TextWriter featureFile, ushort totalSummed)
        {
            float averageIntensity = (float) matchingIsosResult.IsotopicProfile.IntensityAggregate / totalSummed;
            StringBuilder featureOutputStringBuilder = new StringBuilder();
            //umc.OriginalIndex = isotopicPeakList[0].DataIndex
            featureOutputStringBuilder.Append(featureIndex + "\t");
            featureOutputStringBuilder.Append(featureIndex + "\t");
            featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.MonoIsotopicMass.ToString("0.00000") + "\t");
            featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.AverageMass.ToString("0.00000") + "\t");

            //this is to write place holders for the average UMC Min and UMC max mass
            featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.MonoIsotopicMass.ToString("0.00000") + "\t");
            featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.MonoIsotopicMass.ToString("0.00000") + "\t");

            featureOutputStringBuilder.Append(minFrameNum.ToString() + "\t");
            featureOutputStringBuilder.Append(maxFrameNum.ToString() + "\t");
            featureOutputStringBuilder.Append(startFrame + "\t");


            featureOutputStringBuilder.Append(minScanNum.ToString() + "\t");
            featureOutputStringBuilder.Append(maxScanNum.ToString() + "\t");
            featureOutputStringBuilder.Append(startScan + "\t");

            featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.Score.ToString("0.00000") + "\t");
            featureOutputStringBuilder.Append("1\t"); //this represents UMC member count for now
            featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.IntensityAggregate.ToString("0.00000") + "\t");
            featureOutputStringBuilder.Append(averageIntensity.ToString("0.00000") + "\t");

            featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.MonoPeakMZ.ToString("0.00000") + "\t");
            featureOutputStringBuilder.Append(matchingIsosResult.IsotopicProfile.ChargeState.ToString("0.00000") + "\t");
            featureOutputStringBuilder.Append(matchingIsosResult.DriftTime.ToString("0.0000") + "\t");

            featureOutputStringBuilder.Append("ConfScore\n");
            /*featureOutputStringBuilder.Append("LCFitScore\t");
            featureOutputStringBuilder.Append("AverFit\t");
            featureOutputStringBuilder.Append("MemPerc\t");
            featureOutputStringBuilder.Append("CombScore\n");*/

            featureFile.Write(featureOutputStringBuilder.ToString());
            featureFile.Flush();

        }

        private void addPeakToFeatureTree(MSPeak msPeakInIsotopicProfile, Dictionary<ushort, List<ushort>> frameScanLoc, AVLTree<MSResultPeakWithLocation> peaksInAFeatureTree, int startFrame, int startScan)
        {
                BinaryTreeNode<MSResultPeakWithLocation> featureNode = peaksInAFeatureTree.FindFeatureWithGivenMass(msPeakInIsotopicProfile.XValue, 20);
                if (featureNode != null)
                {
                    //that means we found something that is exactly the same mass. let's simply update the frame scans map for this peak
                    featureNode.Value.updateFrameScansRange(frameScanLoc);
                }
                else
                {
                    //there's nothing with a mass close to ours and we have to insert this into the free, based on its mass.
                    peaksInTheTreeCounter++;
                    peaksInAFeatureTree.Add(new MSResultPeakWithLocation(msPeakInIsotopicProfile, frameScanLoc, startFrame, startScan));
                    //logFileWriter.Write(msPeakInIsotopicProfile.XValue.ToString() + ";");
                }
        }

        private void getMinMax(List<ushort> frameNumbers, out int minimum, out int maximum)
        {
            maximum = frameNumbers[0];
            minimum = frameNumbers[0];

            for (int i = 1; i < frameNumbers.Count; i++)
            {
                if (frameNumbers[i] < minimum)
                {
                    minimum = frameNumbers[i];
                }
                else if (frameNumbers[i] > maximum)
                {
                    maximum = frameNumbers[i];
                }
            }

                
        }
        private List<MSPeak> findPeakListClosestIsotopicDistribution(IPeak mostIntensePeak, IList<IsosResult> isosResults, out int isosResultIndex)
        {
            double minMzDistance = Double.MaxValue;
            int index = 0;
            isosResultIndex = -1;

            foreach (IsosResult item in isosResults)
            {
                foreach (MSPeak msPeak in item.IsotopicProfile.Peaklist)
                {
                    double absDiff = Math.Abs(mostIntensePeak.XValue - msPeak.XValue);

                    if (absDiff <= minMzDistance)
                    {
                        isosResultIndex = index;
                        minMzDistance = absDiff;
                    }
                }
                index++;
            }

            if (isosResultIndex != -1)
            {
                return isosResults[isosResultIndex].IsotopicProfile.Peaklist;
            }
            else
            {
                return null;
            }
        }

        private void addPeakListToTree(BinaryTree<IPeak> peakTree, List<IPeak> peakList)
        {
            for (int i = 0; i < peakList.Count; i++)
            {
                peakTree.Add(peakList[i]);
            }
        }


        public void ExecuteWorkflowGordon(DeconTools.Backend.Core.Run run)
        {

            UIMFRun uimfRun = (UIMFRun)run;

            //for each frame

            foreach (var frame in uimfRun.FrameSetCollection.FrameSetList)
            {
                uimfRun.CurrentFrameSet = frame;


                // detect all peaks in frame
                List<MSPeakResult> masterPeakList = getAllPeaksInFrame(uimfRun, NumMSScansToSumWhenBuildingMasterPeakList);

                // sort peaks
                masterPeakList.Sort(delegate(MSPeakResult peak1, MSPeakResult peak2)
                {
                    return peak2.MSPeak.Height.CompareTo(peak1.MSPeak.Height);
                });


                // for each peak
                int peakCounter = 0;
                int peaksThatGenerateAChromatogram = 0;
                foreach (var peak in masterPeakList)
                {
                    peakCounter++;

                    //if (peakCounter > 500) break;
                    if (peak.MSPeak.Height < 1000) break;


                    string peakFate = "Undefined";

                    bool peakResultAlreadyIncludedInChromatogram = (peak.ChromID != -1);
                    if (peakResultAlreadyIncludedInChromatogram)
                    {
                        peakFate = "Chrom_Already";

                        displayPeakInfoAndFate(peak, peakFate);


                        continue;
                    }
                    else
                    {
                        peakFate = "CHROM";

                        //bool peakResultAlreadyFoundInAnMSFeature = findPeakWithinMSFeatureResults(run.ResultCollection.ResultList, peakResult, scanTolerance);
                        //if (peakResultAlreadyFoundInAnMSFeature)
                        //{
                        //    peakFate = "MSFeature_Already";
                        //}
                        //else
                        //{
                        //    peakFate = "CHROM";
                        //}



                    }

                    peaksThatGenerateAChromatogram++;
                    PeakChrom chrom = new BasicPeakChrom();

                    // create drift profile from raw data
                    double driftTimeProfileMZTolerance = this.DriftTimeProfileExtractionPPMTolerance * peak.MSPeak.XValue / 1e6;
                    uimfRun.GetDriftTimeProfile(frame.PrimaryFrame, run.MinScan, run.MaxScan, peak.MSPeak.XValue, driftTimeProfileMZTolerance);

                    bool driftTimeProfileIsEmpty = (uimfRun.XYData.Xvalues == null);
                    if (driftTimeProfileIsEmpty)
                    {
                        addPeakToProcessedPeakList(peak);
                        peakFate = peakFate + " DriftProfileEmpty";
                        displayPeakInfoAndFate(peak, peakFate);

                        continue;
                    }

                    chrom.XYData = uimfRun.XYData;


                    // smooth drift profile
                    chrom.XYData = ChromSmoother.Smooth(uimfRun.XYData);

                    // detect peaks in chromatogram
                    chrom.PeakList = this.ChromPeakDetector.FindPeaks(chrom.XYData, 0, 0);

                    if (chrom.PeakDataIsNullOrEmpty)
                    {
                        addPeakToProcessedPeakList(peak);
                        peakFate = peakFate + " NoChromPeaksDetected";
                        displayPeakInfoAndFate(peak, peakFate);


                        continue;
                    }

                    // find which drift profile peak,  if any, the source peak is a member of
                    IPeak chromPeak = chrom.GetChromPeakForGivenSource(peak);
                    if (chromPeak == null)
                    {
                        addPeakToProcessedPeakList(peak);
                        peakFate = peakFate + " TargetChromPeakNotFound";
                        displayPeakInfoAndFate(peak, peakFate);


                        continue;
                    }

                 
                    // find other peaks in the master peaklist that are members of the found drift profile peak
                    // tag these peaks with the source peak's ID
                    double peakWidthSigma = chromPeak.Width / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)

                    int minScanForChrom = (int)Math.Floor(chromPeak.XValue - peakWidthSigma * 4);
                    int maxScanForChrom = (int)Math.Floor(chromPeak.XValue + peakWidthSigma * 4);

                    double peakToleranceInMZ = driftTimeProfileMZTolerance;
                    double minMZForChromFilter = peak.MSPeak.XValue - peakToleranceInMZ;
                    double maxMZForChromFilter = peak.MSPeak.XValue + peakToleranceInMZ;

                    
                    chrom.Data = (from n in masterPeakList
                                  where n.Scan_num >= minScanForChrom && n.Scan_num <= maxScanForChrom &&
                                      n.MSPeak.XValue >= minMZForChromFilter && n.MSPeak.XValue < maxMZForChromFilter
                                  select n).ToList();
                    
                    foreach (var item in chrom.Data)
                    {
                        item.ChromID = peak.PeakID;
                    }

                    displayPeakInfoAndFate(peak, peakFate);



                }

                Console.WriteLine("peaksProcessed = " + peakCounter);
                Console.WriteLine("peaks generating a chrom = " + peaksThatGenerateAChromatogram);



            }











            // generate MS by integrating over drift profile peak

            // find MS peaks within range

            // find MS Features. 

            // find MS Feature for which the source peak is a member of.  


            // if found, add it. 
            // And, for each MS peaks of the found MS Feature,  mark all peaks of the masterpeak list that correspond to the found drift time peak and m/z

        }

        private void displayPeakInfoAndFate(MSPeakResult peak, string peakFate)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(peak.PeakID);
            sb.Append('\t');
            sb.Append(peak.Scan_num);
            sb.Append('\t');
            sb.Append(peak.MSPeak.XValue);
            sb.Append('\t');
            sb.Append(peak.MSPeak.Height);
            sb.Append('\t');
            sb.Append(peakFate);
            sb.Append('\t');
            sb.Append(peak.ChromID);
            Console.WriteLine(sb.ToString());

        }


        private void printAsAMatrix(int[][] intensityVals)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < intensityVals.Length; i++)
            {

                for (int j = 0; j < intensityVals[i].Length; j++)
                {
                    sb.Append(intensityVals[i][j].ToString() + ",");
                }

                sb.Append("\n");
                

            }

            Console.WriteLine(sb.ToString());


        }

        private void addPeakToProcessedPeakList(MSPeakResult peak)
        {
            peak.ChromID = peak.PeakID;
            //this.processedMSPeaks.Add(peak);
        }

        private List<MSPeakResult> getAllPeaksInFrame(UIMFRun uimfRun, int numIMSScansToSum)
        {
            if (uimfRun.ResultCollection.MSPeakResultList != null)
            {
                uimfRun.ResultCollection.MSPeakResultList.Clear();
            }

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(uimfRun, numIMSScansToSum, 1);
            sscc.Create();

            foreach (var scan in uimfRun.ScanSetCollection.ScanSetList)
            {
                uimfRun.CurrentScanSet = scan;
                msgen.Execute(uimfRun.ResultCollection);
                MasterPeakListPeakDetector.Execute(uimfRun.ResultCollection);

            }

            return uimfRun.ResultCollection.MSPeakResultList;

        }

        private int getMax(int[][] values, out ushort x, out ushort y )
        {
            int max = 0;
            x = 0;
            y = 0;
            for (int i = 0; i < values.Length; i++)
            {
                for (int j = 0; j < values[i].Length; j++)
                {
                    if (values[i][j] >= max)
                    {
                        max = values[i][j];
                        x = (ushort)i;
                        y = (ushort)j;
                    }
                }
                
            }

            return max;
        }

        #endregion
    }
}
/*


int frameNum = 1;
int frameSetIndex = 0;
UIMFRun uimfRun = (UIMFRun)run;
ScanSet startScanSet;
FrameSet endFrameSet;



uimfRun.CurrentFrameSet = uimfRun.FrameSetCollection.FrameSetList[frameSetIndex];
ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(uimfRun, 1, 1);
sscc.Create();
peakDetector = new DeconToolsPeakDetector(4, 3, Globals.PeakFitType.QUADRATIC, true);
            
int startScan = 0;
int startFrame = uimfRun.CurrentFrameSet.PrimaryFrame;
int endScan = 0;
int endFrame = 0;

for (int i = 0; i < uimfRun.ScanSetCollection.ScanSetList.Count; i++)
{
			 
    //start with an individual scan
    uimfRun.CurrentScanSet = uimfRun.ScanSetCollection.ScanSetList[i];

    //Console.WriteLine(uimfRun.CurrentScanSet.PrimaryScanNumber);

    msgen.Execute(uimfRun.ResultCollection);
   // uimfRun.GetMassSpectrum(uimfRun.CurrentScanSet);
    List<IPeak> peaks = peakDetector.FindPeaks(uimfRun.XYData, 0, 5000);
    
    //let's say we need at least 3 peaks in a spectrum to even consider summing
    //or deisotooping it
    if ( peaks.Count > 3 ){
        startScan = uimfRun.CurrentScanSet.PrimaryScanNumber;
        startScanSet = uimfRun.CurrentScanSet;
        endScan = startScan;
        Console.WriteLine("Peaks were found in this scan " + uimfRun.CurrentScanSet.PrimaryScanNumber.ToString());

        //start with the highest peak in the spectrum
        //first sort the data based on intensity
        //peaks.Sort(delegate(DeconTools.Backend.Core.IPeak peak1, DeconTools.Backend.Core.IPeak peak2)
        //{
          //  return peak2.Height.CompareTo(peak1.Height);
       // });


        //we might have to iterate through all the peaks here. TBD
        IPeak mzPeak = peaks[0];
                    
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
            uimfRun.CurrentFrameSet = uimfRun.FrameSetCollection.FrameSetList[
                ++frameSetIndex];
            uimfRun.CurrentScanSet = startScanSet;
            while (peakPresent(mzPeak, uimfRun.CurrentScanSet, uimfRun)){
                uimfRun.CurrentFrameSet = uimfRun.FrameSetCollection.FrameSetList[
                ++frameSetIndex];
            }

            endFrame = uimfRun.CurrentFrameSet.PrimaryFrame - 1;


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
