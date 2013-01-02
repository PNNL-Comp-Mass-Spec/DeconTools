using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Runs;

namespace DeconTools.Workflows.Backend.Core
{
    public class IMS_WholisticFeatureFinderWorkflow : WorkflowBase
    {
        List<MSPeakResult> processedMSPeaks;


        #region Constructors
        public IMS_WholisticFeatureFinderWorkflow(Run run)
        {
            this.Run = run;
            InitializeWorkflow();
        }
        #endregion

        #region Properties

        DeconToolsPeakDetector MasterPeakListPeakDetector { get; set; }

        public SavitzkyGolaySmoother ChromSmoother { get; set; }
        public ChromPeakDetector ChromPeakDetector { get; set; }

        public ChromatogramGenerator ChromGenerator { get; set; }


        public MSGenerator msgen { get; set; }

        public double DriftTimeProfileExtractionPPMTolerance { get; set; }

        public MSGenerator MSgen { get; set; }
        public DeconToolsPeakDetector MSPeakDetector { get; set; }
        public Deconvolutor Deconvolutor { get; set; }

        public int NumMSScansToSumWhenBuildingMasterPeakList { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion


        #region IWorkflow Members

        public string Name { get; set; }

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

            NumMSScansToSumWhenBuildingMasterPeakList = 3;

            MasterPeakListPeakDetector = new DeconToolsPeakDetector();
            MasterPeakListPeakDetector.PeakToBackgroundRatio = 4;
            MasterPeakListPeakDetector.SignalToNoiseThreshold = 3;
            MasterPeakListPeakDetector.IsDataThresholded = false;
            MasterPeakListPeakDetector.PeaksAreStored = true;

            
            msgen = MSGeneratorFactory.CreateMSGenerator(DeconTools.Backend.Globals.MSFileType.PNNL_UIMF);



            this.DriftTimeProfileExtractionPPMTolerance = 15;

            this.ChromSmoother = new SavitzkyGolaySmoother(23,2);
            this.ChromPeakDetector = new ChromPeakDetector(0.5, 0.5);


            this.ChromGenerator = new ChromatogramGenerator();

            processedMSPeaks = new List<MSPeakResult>();
        }

        public override void Execute()
        {

            UIMFRun uimfRun = (UIMFRun)this.Run;

            //for each frame

            foreach (var frame in uimfRun.ScanSetCollection.ScanSetList)
            {
                uimfRun.CurrentScanSet = frame;


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
                    
                    //TODO: Fix this: update to use UIMF library and not DeconTools
                    //uimfRun.GetDriftTimeProfile  (frame.PrimaryFrame, this.Run.MinScan, this.Run.MaxScan, peak.MSPeak.XValue, driftTimeProfileMZTolerance);

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
                    Peak chromPeak = chrom.GetChromPeakForGivenSource(peak);
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

                    
                    chrom.ChromSourceData = (from n in masterPeakList
                                  where n.Scan_num >= minScanForChrom && n.Scan_num <= maxScanForChrom &&
                                      n.MSPeak.XValue >= minMZForChromFilter && n.MSPeak.XValue < maxMZForChromFilter
                                  select n).ToList();
                    
                    foreach (var item in chrom.ChromSourceData)
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

        private void addPeakToProcessedPeakList(MSPeakResult peak)
        {
            peak.ChromID = peak.PeakID;
            this.processedMSPeaks.Add(peak);
        }

        private List<MSPeakResult> getAllPeaksInFrame(UIMFRun uimfRun, int numIMSScansToSum)
        {
            if (uimfRun.ResultCollection.MSPeakResultList != null)
            {
                uimfRun.ResultCollection.MSPeakResultList.Clear();
            }

            uimfRun.ScanSetCollection.Create(uimfRun, numIMSScansToSum, 1);
            
            foreach (var scan in uimfRun.ScanSetCollection.ScanSetList)
            {
                uimfRun.CurrentScanSet = scan;
                msgen.Execute(uimfRun.ResultCollection);
                MasterPeakListPeakDetector.Execute(uimfRun.ResultCollection);

            }

            return uimfRun.ResultCollection.MSPeakResultList;

        }

        #endregion

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
