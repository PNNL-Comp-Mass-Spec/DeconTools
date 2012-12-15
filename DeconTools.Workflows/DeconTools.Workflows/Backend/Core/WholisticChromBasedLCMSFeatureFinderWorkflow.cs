using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class WholisticChromBasedLCMSFeatureFinderWorkflow : WorkflowBase
    {
        string m_peakOutputFileName;
        string m_isosResultFileName;
        string m_logFileName;

        string m_baseOutputPath;
        int m_msFeatureCounter;

        #region Constructors

        public WholisticChromBasedLCMSFeatureFinderWorkflow(Run run)
        {
            this.Run = run;
            this.Name = this.ToString();

            this.m_baseOutputPath = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2011\2011_02_10_SmartAveraging_OrbitrapData";
            this.m_isosResultFileName = m_baseOutputPath + Path.DirectorySeparatorChar + "MSFeaturesOutput.csv";
            this.m_peakOutputFileName = m_baseOutputPath + Path.DirectorySeparatorChar + "peakOutput.txt";
            this.m_logFileName = m_baseOutputPath + Path.DirectorySeparatorChar + "log.txt";

            InitializeWorkflow();

        }

        public WholisticChromBasedLCMSFeatureFinderWorkflow(Run run, string outputPeakFilename, string outputIsosResultFileName)
            : this(run)
        {
            m_peakOutputFileName = outputPeakFilename;
            m_isosResultFileName = outputIsosResultFileName;
        }


        #endregion

        #region Properties

        public int ChromGenToleranceInPPM { get; set; }


        public ChromatogramGenerator ChromGenerator { get; set; }
        public DeconTools.Backend.ProcessingTasks.Smoothers.SavitzkyGolaySmoother ChromSmoother { get; set; }
        public ChromPeakDetector ChromPeakDetector { get; set; }


        public MSGenerator MSgen { get; set; }
        public DeconToolsPeakDetector MSPeakDetector { get; set; }
        public Deconvolutor Deconvolutor { get; set; }


        DeconTools.Backend.FileIO.MSFeatureToTextFileExporterBasic isosExporter;


        #endregion

        #region Public Methods



        #endregion

        #region Private Methods
        #endregion
        #region IWorkflow Members

        public string Name { get; set; }
        public int MinScan { get; set; }
        public int MaxScan { get; set; }

        //public override WorkflowParameters WorkflowParameters
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public override void InitializeWorkflow()
        {
            this.ChromGenToleranceInPPM = 20;

            this.ChromGenerator = new ChromatogramGenerator();

            this.ChromSmoother = new DeconTools.Backend.ProcessingTasks.Smoothers.SavitzkyGolaySmoother(23, 2);

            this.ChromPeakDetector = new ChromPeakDetector(0.5, 0.5);

            this.MSPeakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            this.Deconvolutor = new RapidDeconvolutor();

            Validator = new ResultValidatorTask();


            isosExporter = new DeconTools.Backend.FileIO.MSFeatureToTextFileExporterBasic(m_isosResultFileName);


        }

        public override void Execute()
        {
            double scanTolerance = 100;


            Check.Require(this.Run != null, String.Format("{0} failed. Run not defined.", this.Name));
            Check.Require(this.Run.ResultCollection != null && this.Run.ResultCollection.MSPeakResultList != null && this.Run.ResultCollection.MSPeakResultList.Count > 0,
                String.Format("{0} failed. Workflow requires MSPeakResults, but these were not defined.", this.Name));

            List<MSPeakResult> sortedMSPeakResultList = this.Run.ResultCollection.MSPeakResultList.OrderByDescending(p => p.MSPeak.Height).ToList();

            bool msGeneratorNeedsInitializing = (this.MSgen == null);
            if (msGeneratorNeedsInitializing)
            {
                this.MSgen = MSGeneratorFactory.CreateMSGenerator(this.Run.MSFileType);
            }



            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            List<long> deconTimes = new List<long>();

            StringBuilder sb = new StringBuilder();

            int totalPeaks = sortedMSPeakResultList.Count;

            int counter = -1;




            Dictionary<int, string> whatPeakWentWhere = new Dictionary<int, string>();

            MSPeakResult lastPeakResult = sortedMSPeakResultList.Last();

            int chromPeaksCounter = 0;

            foreach (var peakResult in sortedMSPeakResultList)
            {
                counter++;

                //if (counter > 10000)
                //{
                //    break;
                //}


                if (counter % 1000 == 0)
                {


                    string logEntry = DateTime.Now + "\tWorking on peak " + counter + " of " + totalPeaks + "\tMSFeaturesCount =\t" + m_msFeatureCounter + "\tChomPeaks =\t" + chromPeaksCounter;
                    Logger.Instance.AddEntry(logEntry, m_logFileName);
                    Console.WriteLine(logEntry);
                    chromPeaksCounter = 0;
                }

                //if (peakResult.PeakID == 396293)
                //{
                //    Console.WriteLine(DateTime.Now + "\tWorking on peak " + peakResult.PeakID);
                //}


                string peakFate = "Undefined";

                bool peakResultAlreadyIncludedInChromatogram = (peakResult.ChromID != -1);
                if (peakResultAlreadyIncludedInChromatogram)
                {
                    peakFate = "Chrom_Already";
                }

                if (!peakResultAlreadyIncludedInChromatogram)
                {
                    bool peakResultAlreadyFoundInAnMSFeature = findPeakWithinMSFeatureResults(this.Run.ResultCollection.ResultList, peakResult, scanTolerance);
                    if (peakResultAlreadyFoundInAnMSFeature)
                    {
                        peakFate = "MSFeature_Already";
                    }
                    else
                    {
                        peakFate = "CHROM";
                    }
                }

                whatPeakWentWhere.Add(peakResult.PeakID, peakFate);

                try
                {
                    if (peakFate == "CHROM")
                    {
                        //generate chromatogram & tag MSPeakResults
                        XYData chromatogram = this.ChromGenerator.GenerateChromatogram(this.Run.ResultCollection.MSPeakResultList, this.Run.MinLCScan, this.Run.MaxLCScan, peakResult.MSPeak.XValue, this.ChromGenToleranceInPPM, peakResult.PeakID);

                        if (chromatogram == null) continue;


                        //remove points from chromatogram due to MS/MS level data
                        if (this.Run.ContainsMSMSData)
                        {
                            chromatogram = filterOutMSMSRelatedPoints(this.Run, chromatogram);
                        }

                        //smooth the chromatogram
                        chromatogram = this.ChromSmoother.Smooth(chromatogram);

                        //detect peaks in chromatogram
                        List<Peak> chromPeakList = this.ChromPeakDetector.FindPeaks(chromatogram, 0, 0);

                        //sort chrompeak list so it is decending.
                        chromPeakList = chromPeakList.OrderByDescending(p => p.Height).ToList();

                        //this is the temporary results that are collected for MSFeatures found over each chromPeak of the ChromPeakList
                        List<IsosResult> tempChromPeakMSFeatures = new List<IsosResult>();

                        //store each chrom peak (which, here, is an ElutingPeak)

                        foreach (var chromPeak in chromPeakList)
                        {
                            int diffBetweenSourcePeakAndChromPeak = Math.Abs(peakResult.Scan_num - (int)chromPeak.XValue);

                            //TODO: examine whether or not we should iterate over the chromPeakList or not....
                            chromPeaksCounter++;
                            //Console.WriteLine("source peak = " + peakResult.PeakID + "; scan = " + peakResult.Scan_num + "; diff = " + diffBetweenSourcePeakAndChromPeak);

                            try
                            {

                                this.Run.CurrentScanSet = createScanSetFromChromatogramPeak(this.Run, chromPeak);

                                MSgen.Execute(this.Run.ResultCollection);

                                //trim the XYData to help the peak detector and Deconvolutor work faster
                                this.Run.XYData = this.Run.XYData.TrimData(peakResult.MSPeak.XValue - 2, peakResult.MSPeak.XValue + 2);

                                this.MSPeakDetector.Execute(this.Run.ResultCollection);

                                //HACK:  calling 'deconvolute' will write results to 'isosResultBin' but not to 'ResultList';  I will manually add what I want to the official 'ResultList'
                                this.Run.ResultCollection.IsosResultBin.Clear();
                                this.Deconvolutor.Deconvolute(this.Run.ResultCollection);

                                this.Validator.Execute(this.Run.ResultCollection);

                                //Need to find the target peak within the MSFeature.  Then mark other peaks of the MSFeature as being found, so that we don't bother generating a MS and deisotoping
                                findTargetPeakAddResultsToCollectionAndMarkAssociatedPeaks(tempChromPeakMSFeatures, peakResult, this.Run, scanTolerance);
                            }
                            catch (Exception ex)
                            {

                                Logger.Instance.AddEntry("ERROR:  peakID = " + peakResult.PeakID + "\t" + ex.Message + ";\t" + ex.StackTrace, m_logFileName);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.AddEntry("ERROR:  peakID = " + peakResult.PeakID + "\t" + ex.Message + ";\t" + ex.StackTrace, m_logFileName);
                }



                int triggerToExport = 10;
                if (this.Run.ResultCollection.ResultList.Count > triggerToExport || peakResult == lastPeakResult)
                {


                    isosExporter.ExportResults(this.Run.ResultCollection.ResultList);
                    this.Run.ResultCollection.ResultList.Clear();



                    exportPeakData(this.Run, m_peakOutputFileName, whatPeakWentWhere);
                    whatPeakWentWhere.Clear();

                }





            }

            //foreach (var item in deconTimes)
            //{
            //    Console.WriteLine(item);
            //}

            //Console.WriteLine("Average = " + deconTimes.Average());
            //Console.WriteLine("Top 50 = " + deconTimes.Take(50).Average());
            //Console.WriteLine("Next 50 = " + deconTimes.Skip(50).Take(50).Average());







            //Console.WriteLine(sb.ToString());



        }

        public void ExecuteWorkflow2(DeconTools.Backend.Core.Run run)
        {
            double scanTolerance = 100;


            Check.Require(run != null, String.Format("{0} failed. Run not defined.", this.Name));
            Check.Require(run.ResultCollection != null && run.ResultCollection.MSPeakResultList != null && run.ResultCollection.MSPeakResultList.Count > 0,
                String.Format("{0} failed. Workflow requires MSPeakResults, but these were not defined.", this.Name));

            List<MSPeakResult> sortedMSPeakResultList = run.ResultCollection.MSPeakResultList.OrderByDescending(p => p.MSPeak.Height).ToList();

            bool msGeneratorNeedsInitializing = (this.MSgen == null);
            if (msGeneratorNeedsInitializing)
            {
                this.MSgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            }



            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            List<long> deconTimes = new List<long>();

            StringBuilder sb = new StringBuilder();

            int totalPeaks = sortedMSPeakResultList.Count;

            int counter = -1;




            Dictionary<int, string> whatPeakWentWhere = new Dictionary<int, string>();

            MSPeakResult lastPeakResult = sortedMSPeakResultList.Last();

            int chromPeaksCounter = 0;
            

            foreach (var peakResult in sortedMSPeakResultList)
            {
                counter++;

                //if (counter > 10000)
                //{
                //    break;
                //}


                if (counter % 1000 == 0)
                {


                    string logEntry = DateTime.Now + "\tWorking on peak " + counter + " of " + totalPeaks + "\tMSFeaturesCount =\t" + m_msFeatureCounter + "\tChomPeaks =\t" + chromPeaksCounter;
                    Logger.Instance.AddEntry(logEntry, m_logFileName);
                    Console.WriteLine(logEntry);
                    chromPeaksCounter = 0;
                }

                //if (peakResult.PeakID == 396293)
                //{
                //    Console.WriteLine(DateTime.Now + "\tWorking on peak " + peakResult.PeakID);
                //}


                string peakFate = "Undefined";

                bool peakResultAlreadyIncludedInChromatogram = (peakResult.ChromID != -1);
                if (peakResultAlreadyIncludedInChromatogram)
                {
                    peakFate = "Chrom_Already";
                }
                else
                {
                    bool peakResultAlreadyFoundInAnMSFeature = findPeakWithinMSFeatureResults(run.ResultCollection.ResultList, peakResult, scanTolerance);
                    if (peakResultAlreadyFoundInAnMSFeature)
                    {
                        peakFate = "MSFeature_Already";
                    }
                    else
                    {
                        peakFate = "CHROM";
                    }

                }

                whatPeakWentWhere.Add(peakResult.PeakID, peakFate);

                try
                {
                    if (peakFate == "CHROM")
                    {
                        //generate chromatogram & tag MSPeakResults

                        int minScanForChrom = peakResult.Scan_num - (int)scanTolerance;
                        if (minScanForChrom < run.MinLCScan)
                        {
                            minScanForChrom = run.MinLCScan;
                        }

                        int maxScanForChrom = peakResult.Scan_num + (int)scanTolerance;
                        if (maxScanForChrom > run.MaxLCScan)
                        {
                            maxScanForChrom = run.MaxLCScan;
                        }

                        PeakChrom chrom = new BasicPeakChrom();
                        chrom.ChromSourceData = this.ChromGenerator.GeneratePeakChromatogram(run.ResultCollection.MSPeakResultList, minScanForChrom, maxScanForChrom,
                            peakResult.MSPeak.XValue, this.ChromGenToleranceInPPM);

                        if (chrom.IsNullOrEmpty) continue;

                        chrom.XYData = chrom.GetXYDataFromChromPeakData(minScanForChrom, maxScanForChrom);



                        //remove points from chromatogram due to MS/MS level data
                        if (run.ContainsMSMSData)
                        {
                            chrom.XYData = filterOutMSMSRelatedPoints(run, chrom.XYData);
                        }

                        //smooth the chromatogram
                        chrom.XYData = this.ChromSmoother.Smooth(chrom.XYData);

                        //detect peaks in chromatogram
                        chrom.PeakList = this.ChromPeakDetector.FindPeaks(chrom.XYData, 0, 0);

                        //Console.WriteLine("source peak -> scan= " + peakResult.Scan_num + "; m/z= " + peakResult.MSPeak.XValue);
                        //chrom.XYData.Display();



                        if (!chrom.PeakDataIsNullOrEmpty)
                        {
                            Peak chromPeak = chrom.GetChromPeakForGivenSource(peakResult);
                            if (chromPeak == null)
                            {
                                continue;
                            }

                            double peakWidthSigma = chromPeak.Width / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)

                            //now mark all peakResults that are members of this chromPeak
                            chrom.GetMSPeakMembersForGivenChromPeakAndAssignChromID(chromPeak, peakWidthSigma * 4, peakResult.PeakID);

                            run.CurrentScanSet = createScanSetFromChromatogramPeak(run, chromPeak);

                            List<IsosResult> tempChromPeakMSFeatures = new List<IsosResult>();

                            MSgen.Execute(run.ResultCollection);

                            //trim the XYData to help the peak detector and Deconvolutor work faster

                            if (run.XYData == null)
                            {
                                continue;
                            }

                            run.XYData = run.XYData.TrimData(peakResult.MSPeak.XValue - 2, peakResult.MSPeak.XValue + 2);

                            if (run.XYData == null || run.XYData.Xvalues == null || run.XYData.Xvalues.Length == 0)
                            {
                                continue;
                            }

                            this.MSPeakDetector.Execute(run.ResultCollection);

                            //HACK:  calling 'deconvolute' will write results to 'isosResultBin' but not to 'ResultList';  I will manually add what I want to the official 'ResultList'
                            run.ResultCollection.IsosResultBin.Clear();
                            this.Deconvolutor.Deconvolute(run.ResultCollection);

                            this.Validator.Execute(run.ResultCollection);

                            //now, look in the isosResultBin and see what IsosResult (if any) the source peak is a member of
                            IsosResult msfeature = getMSFeatureForCurrentSourcePeak(peakResult, run);

                            //Console.WriteLine("source peak -> peakID= " + peakResult.PeakID + ";scan= " + peakResult.Scan_num + "; m/z= " + peakResult.MSPeak.XValue);

                            if (msfeature == null)  // didn't find a feature.  Source peak might be a 'lone wolf'
                            {
                                //Console.WriteLine("No MSFeature found!");
                            }
                            else
                            {
                                //Console.WriteLine("!!!!!!! MSFeature found!");

                                double toleranceInMZ = peakResult.MSPeak.Width / 2;

                                bool msFeatureAlreadyExists = checkIfMSFeatureAlreadyExists(msfeature, run.ResultCollection.ResultList, toleranceInMZ, peakWidthSigma);

                                if (msFeatureAlreadyExists)
                                {
                                    //Console.WriteLine("---- but MSFeature was already present");

                                }
                                else
                                {

                                    //generate chromatograms and tag other peak members of the isotopic profile...
                                    foreach (var isoPeak in msfeature.IsotopicProfile.Peaklist)
                                    {
                                        PeakChrom isoPeakChrom = new BasicPeakChrom();

                                        isoPeakChrom.ChromSourceData = this.ChromGenerator.GeneratePeakChromatogram(run.ResultCollection.MSPeakResultList, minScanForChrom, maxScanForChrom,
                                            isoPeak.XValue, this.ChromGenToleranceInPPM);
                                        if (!isoPeakChrom.IsNullOrEmpty)
                                        {
                                            isoPeakChrom.GetMSPeakMembersForGivenChromPeakAndAssignChromID(chromPeak, peakWidthSigma * 4, peakResult.PeakID);
                                        }

                                    }

                                    run.ResultCollection.ResultList.Add(msfeature);
                                    m_msFeatureCounter++;
                                }

                            }

                            //Console.WriteLine();


                        }



                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.AddEntry("ERROR:  peakID = " + peakResult.PeakID + "\t" + ex.Message + ";\t" + ex.StackTrace, m_logFileName);
                }



                int triggerToExport = 10;
                if (run.ResultCollection.ResultList.Count > triggerToExport)
                {
                    isosExporter.ExportResults(run.ResultCollection.ResultList);
                    run.ResultCollection.ResultList.Clear();

                    exportPeakData(run, m_peakOutputFileName, whatPeakWentWhere);
                    whatPeakWentWhere.Clear();
                }


            }


            //needs clean up....   sometimes there might be a case where the above loop is broken and we need the last few results written out. 
            isosExporter.ExportResults(run.ResultCollection.ResultList);
            run.ResultCollection.ResultList.Clear();

            exportPeakData(run, m_peakOutputFileName, whatPeakWentWhere);
            whatPeakWentWhere.Clear();



            //foreach (var item in deconTimes)
            //{
            //    Console.WriteLine(item);
            //}

            //Console.WriteLine("Average = " + deconTimes.Average());
            //Console.WriteLine("Top 50 = " + deconTimes.Take(50).Average());
            //Console.WriteLine("Next 50 = " + deconTimes.Skip(50).Take(50).Average());







            //Console.WriteLine(sb.ToString());



        }

        private bool checkIfMSFeatureAlreadyExists(IsosResult msfeature, List<IsosResult> list, double toleranceInMZ, double scanTolerance)
        {



            var query = (from n in list
                         where (Math.Abs(n.IsotopicProfile.MonoPeakMZ - msfeature.IsotopicProfile.MonoPeakMZ) <= toleranceInMZ) &&
                         n.IsotopicProfile.ChargeState == msfeature.IsotopicProfile.ChargeState &&
                         (Math.Abs(n.ScanSet.PrimaryScanNumber - msfeature.ScanSet.PrimaryScanNumber) <= scanTolerance)
                         select n);


            if (query.Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        private IsosResult getMSFeatureForCurrentSourcePeak(MSPeakResult peakResult, Run run)
        {
            if (run.ResultCollection.IsosResultBin == null || run.ResultCollection.IsosResultBin.Count == 0)
            {
                return null;
            }

            Dictionary<IsosResult, double> isosResultPossiblyContainingSourcePeak = new Dictionary<IsosResult, double>();    //store possible isosResult, along with it's difference with the peakResult

            for (int i = 0; i < run.ResultCollection.IsosResultBin.Count; i++)
            {
                IsosResult msfeature = run.ResultCollection.IsosResultBin[i];

                double toleranceInMZ = peakResult.MSPeak.Width / 2;


                List<MSPeak> peaksWithinTolerance = PeakUtilities.GetMSPeaksWithinTolerance(msfeature.IsotopicProfile.Peaklist, peakResult.MSPeak.XValue, toleranceInMZ);
                if (peaksWithinTolerance == null || peaksWithinTolerance.Count == 0)
                {

                }
                else
                {
                    double diff = Math.Abs(peaksWithinTolerance[0].XValue - peakResult.MSPeak.XValue);
                    isosResultPossiblyContainingSourcePeak.Add(msfeature, diff);
                }
            }

            if (isosResultPossiblyContainingSourcePeak.Count == 0)
            {
                return null;
            }
            else if (isosResultPossiblyContainingSourcePeak.Count == 1)
            {
                return isosResultPossiblyContainingSourcePeak.First().Key;

            }
            else
            {
                return isosResultPossiblyContainingSourcePeak.Keys.OrderByDescending(p => p.IntensityAggregate).First();
            }




        }




        private void exportPeakData(Run run, string outputFilename, Dictionary<int, string> whatPeakWentWhere)
        {
            StringBuilder peaksb = new StringBuilder();

            using (StreamWriter outputStream = new StreamWriter(new System.IO.FileStream(outputFilename, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read)))
            {
                foreach (var item in whatPeakWentWhere)
                {
                    peaksb = new StringBuilder();
                    peaksb.Append(item.Key);
                    peaksb.Append("\t");

                    MSPeakResult peak = run.ResultCollection.MSPeakResultList.Find(p => p.PeakID == item.Key);

                    peaksb.Append(peak.Scan_num);
                    peaksb.Append("\t");
                    peaksb.Append(peak.ChromID);
                    peaksb.Append("\t");
                    peaksb.Append(peak.MSPeak.XValue);
                    peaksb.Append("\t");
                    peaksb.Append(peak.MSPeak.Height);
                    peaksb.Append("\t");
                    peaksb.Append(item.Value);

                    outputStream.WriteLine(peaksb.ToString());

                }
            }
        }


        private bool findPeakWithinMSFeatureResults(List<IsosResult> msFeatureList, MSPeakResult peakResult, double scanTolerance)
        {
            double toleranceInPPM = 5;
            double toleranceInMZ = toleranceInPPM / 1e6 * peakResult.MSPeak.XValue;

            foreach (var msfeature in msFeatureList)
            {
                if (msfeature.IsotopicProfile == null || msfeature.IsotopicProfile.Peaklist == null || msfeature.IsotopicProfile.Peaklist.Count == 0) continue;

                //check target peak is within an allowable scan tolerance
                bool targetPeakIsWithinScanTol = Math.Abs(msfeature.ScanSet.PrimaryScanNumber - peakResult.Scan_num) <= scanTolerance;
                if (!targetPeakIsWithinScanTol) continue;

                List<MSPeak> peaksWithinTol = PeakUtilities.GetMSPeaksWithinTolerance(msfeature.IsotopicProfile.Peaklist, peakResult.MSPeak.XValue, toleranceInMZ);

                if (peaksWithinTol.Count == 0)
                {
                    continue;
                }
                else
                {
                    return true;
                }



            }

            return false;

        }

        private void findTargetPeakAddResultsToCollectionAndMarkAssociatedPeaks(List<IsosResult> chromPeakBasedMSFeatures, MSPeakResult peakResult, Run run, double scanTolerance)
        {
            double toleranceInPPM = 5;

            double toleranceInMZ = toleranceInPPM / 1e6 * peakResult.MSPeak.XValue;
            bool foundPeakWithinMSFeature = false;

            IList<IsosResult> msFeatureList = run.ResultCollection.IsosResultBin;    //this is the small list if features found within a small m/z range, based on the targeted peak. 


            for (int i = 0; i < msFeatureList.Count; i++)
            {
                IsosResult msfeature = msFeatureList[i];
                if (msfeature.IsotopicProfile == null || msfeature.IsotopicProfile.Peaklist == null || msfeature.IsotopicProfile.Peaklist.Count == 0) continue;

                List<MSPeak> peaksWithinTol = DeconTools.Backend.Utilities.PeakUtilities.GetMSPeaksWithinTolerance(msfeature.IsotopicProfile.Peaklist, peakResult.MSPeak.XValue, toleranceInMZ);

                if (peaksWithinTol.Count == 0)
                {
                    foundPeakWithinMSFeature = false;
                }
                else if (peaksWithinTol.Count == 1)
                {
                    foundPeakWithinMSFeature = true;

                    bool peakResultAlreadyFoundInAnMSFeature = findPeakWithinMSFeatureResults(chromPeakBasedMSFeatures, peakResult, scanTolerance);
                    if (!peakResultAlreadyFoundInAnMSFeature)
                    {
                        run.ResultCollection.ResultList.Add(msfeature);   // add it to the big long list of MSFeatures found
                        m_msFeatureCounter++;
                    }

                    chromPeakBasedMSFeatures.Add(msfeature);       //also add it to a temporary list.  This list is much smaller and can be searched more quickly. 


                }
                else
                {
                    Console.WriteLine("Not sure what to do with this case!");
                }

                if (foundPeakWithinMSFeature) break;
            }
        }

        private ScanSet createScanSetFromChromatogramPeak(Run run, Peak chromPeak)
        {
            double sigma = chromPeak.Width / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)
            double centerXVal = chromPeak.XValue;

            int leftMostScan = (int)Math.Floor(centerXVal - sigma);
            int rightMostScan = (int)Math.Ceiling(centerXVal + sigma);
            int centerScan = (int)Math.Round(centerXVal);

            centerScan = run.GetClosestMSScan(centerScan, DeconTools.Backend.Globals.ScanSelectionMode.CLOSEST);
            leftMostScan = run.GetClosestMSScan(leftMostScan, DeconTools.Backend.Globals.ScanSelectionMode.DESCENDING);
            rightMostScan = run.GetClosestMSScan(rightMostScan, DeconTools.Backend.Globals.ScanSelectionMode.ASCENDING);

            List<int> scanIndexList = new List<int>();


            //the idea is to start at the center scan and add scans to the left and right.  It is easier to set a maximum number of scans to add this way.

            int maxScansToAddToTheLeft = 0;
            int maxScansToAddToTheRight = 0;

            //add center scan
            scanIndexList.Add(centerScan);

            //add to the left of center
            int numScansAdded = 0;
            for (int i = (centerScan - 1); i >= leftMostScan; i--)
            {
                if (run.GetMSLevel(i) == 1)
                {
                    scanIndexList.Add(i);

                    numScansAdded++;
                    if (numScansAdded >= maxScansToAddToTheLeft)
                    {
                        break;
                    }
                }



            }

            //add to the right of center
            numScansAdded = 0;
            for (int i = (centerScan + 1); i <= rightMostScan; i++)
            {
                if (run.GetMSLevel(i) == 1)
                {
                    scanIndexList.Add(i);

                    numScansAdded++;
                    if (numScansAdded >= maxScansToAddToTheRight)
                    {
                        break;
                    }

                }



            }

            scanIndexList.Sort();

            //ScanSet scanSet = new ScanSet(centerScan, scanIndexList.ToArray());
            ScanSet scanSet = new ScanSet(centerScan);

            return scanSet;

        }

        private XYData filterOutMSMSRelatedPoints(Run run, XYData chromatogram)
        {
            if (chromatogram == null) return null;

            XYData output = new XYData();
            Dictionary<int, double> filteredChromVals = new Dictionary<int, double>();



            for (int i = 0; i < chromatogram.Xvalues.Length; i++)
            {
                int currentScanVal = (int)chromatogram.Xvalues[i];
                if (currentScanVal > run.MaxLCScan)
                {
                    break;
                }

                try
                {

                    if (run.GetMSLevel(currentScanVal) == 1)
                    {
                        filteredChromVals.Add(currentScanVal, chromatogram.Yvalues[i]);
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message + "---------------- scan " + currentScanVal);
                }
            }

            output.Xvalues = XYData.ConvertIntsToDouble(filteredChromVals.Keys.ToArray());
            output.Yvalues = filteredChromVals.Values.ToArray();

            return output;
        }

        #endregion





        public ResultValidatorTask Validator { get; set; }

        public override void InitializeRunRelatedTasks()
        {
            throw new NotImplementedException();
        }

        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
