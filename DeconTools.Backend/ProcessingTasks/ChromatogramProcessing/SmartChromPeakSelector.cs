using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class SmartChromPeakSelector : Task
    {
        private const double DEFAULT_MSPEAKDETECTOR_PEAKBR = 1.3;
        private const double DEFAULT_MSPEAKDETECTOR_SIGNOISERATIO = 2;
        private const double DEFAULT_TARGETEDMSFEATUREFINDERTOLERANCE_PPM = 20;

        private DeconTools.Backend.ProcessingTasks.I_MSGenerator msgen;
        private DeconTools.Backend.ProcessingTasks.ResultValidators.ResultValidatorTask resultValidator;
        private MassTagFitScoreCalculator fitScoreCalc;

        internal class PeakQualityData
        {

            internal PeakQualityData(ChromPeak peak)
            {
                this.i_score = 1;     // worst possible
                this.fitScore = 1;   // worst possible
                this.abundance = 0;
                this.peak = peak;
                this.isotopicProfileFound = false;
            }

            internal ChromPeak peak;
            internal bool isotopicProfileFound;
            internal double fitScore;
            internal double i_score;
            internal double abundance;


            internal void Display()
            {
                Console.WriteLine(peak.XValue.ToString("0.00") + "\t" + peak.NETValue.ToString("0.0000") + "\t" + abundance + "\t" + fitScore.ToString("0.0000") + "\t" + i_score.ToString("0.000"));
            }
        }

        #region Constructors
        public SmartChromPeakSelector()
        {
            MSPeakDetector = new DeconToolsPeakDetector(DEFAULT_MSPEAKDETECTOR_PEAKBR, DEFAULT_MSPEAKDETECTOR_SIGNOISERATIO, Globals.PeakFitType.QUADRATIC, true);
            TargetedMSFeatureFinder = new TargetedFeatureFinders.BasicTFF(DEFAULT_TARGETEDMSFEATUREFINDERTOLERANCE_PPM);
            resultValidator = new ResultValidators.ResultValidatorTask();
            fitScoreCalc = new MassTagFitScoreCalculator();

            this.NETTolerance = 0.025f;
            this.NumScansToSum = 1;

        }


        public SmartChromPeakSelector(float netTolerance, int numScansToSum)
            : this()
        {
            this.NETTolerance = netTolerance;
            this.NumScansToSum = numScansToSum;

        }


        #endregion

        #region Properties
        public float NETTolerance { get; set; }

        public int NumScansToSum { get; set; }

        public DeconTools.Backend.ProcessingTasks.DeconToolsPeakDetector MSPeakDetector { get; set; }
        public DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders.BasicTFF TargetedMSFeatureFinder { get; set; }

        #endregion

        #region Public Methods
        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl.Run.CurrentMassTag != null, this.Name + " failed. MassTag was not defined.");

            if (msgen == null)
            {
                MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
                msgen = msgenFactory.CreateMSGenerator(resultColl.Run.MSFileType);

            }

            MassTag mt = resultColl.Run.CurrentMassTag;

            //collect Chrom peaks that fall within the NET tolerance
            List<ChromPeak> peaksWithinTol = new List<ChromPeak>(); // 
            foreach (ChromPeak peak in resultColl.Run.PeakList)
            {
                if (Math.Abs(peak.NETValue - mt.NETVal) <= NETTolerance)     //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    peaksWithinTol.Add(peak);
                }
            }


            List<PeakQualityData> peakQualityList = new List<PeakQualityData>();

            MassTagResultBase currentResult = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);

            //iterate over peaks within tolerance and score each peak according to MSFeature quality
            foreach (var peak in peaksWithinTol)
            {
                ScanSet scanset = createSummedScanSet(peak, resultColl.Run);
                PeakQualityData pq = new PeakQualityData(peak);
                peakQualityList.Add(pq);

                resultColl.Run.CurrentScanSet = scanset;

                //generate a mass spectrum
                msgen.Execute(resultColl);

                //detect peaks
                MSPeakDetector.Execute(resultColl);

                //find isotopic profile
                TargetedMSFeatureFinder.Execute(resultColl);

                //get fit score
                fitScoreCalc.Execute(resultColl);

                //get i_score
                resultValidator.Execute(resultColl);

                //collect the results together
                addScoresToPeakQualityData(pq, currentResult);

                pq.Display();

            }


            //run a algorithm that decides, based on fit score mostly. 
            ChromPeak bestChromPeak = determineBestChromPeak(peakQualityList);

            ScanSet bestScanset = createSummedScanSet(bestChromPeak, resultColl.Run);
            resultColl.Run.CurrentScanSet = bestScanset;   // maybe good to set this here so that the MSGenerator can operate on it...  

            currentResult.AddSelectedChromPeakAndScanSet(bestChromPeak, bestScanset);

            Check.Ensure(currentResult.ChromPeakSelected != null && currentResult.ChromPeakSelected.XValue != 0, "ChromPeakSelector failed. No chromatographic peak found within tolerances.");




        }

        //helper method
        public void SetDefaultMSPeakDetectorSettings(double peakBR, double signoiseRatio, Globals.PeakFitType peakFitType, bool isThresholded)
        {
            MSPeakDetector.PeakBackgroundRatio = peakBR;
            MSPeakDetector.SigNoiseThreshold = signoiseRatio;
            MSPeakDetector.PeakFitType = peakFitType;
            MSPeakDetector.IsDataThresholded = isThresholded;
        }

        //helper method
        public void SetDefaultTargetedFeatureFinderSettings(double toleranceInPPM)
        {
            TargetedMSFeatureFinder.ToleranceInPPM = toleranceInPPM;
        }

        #endregion

        #region Private Methods
        private ChromPeak determineBestChromPeak(List<PeakQualityData> peakQualityList)
        {
            var filteredList1 = (from n in peakQualityList where n.isotopicProfileFound == true && n.fitScore < 1 && n.i_score < 1 select n).ToList();

            ChromPeak bestpeak;

            bool allChromPeaksAreBad = filteredList1.Count == 0;
            if (filteredList1.Count == 0)
            {
                bestpeak = null;
            }
            else if (filteredList1.Count == 1)
            {
                bestpeak = filteredList1[0].peak;
            }
            else
            {
                //for now, simply select the peak with the lowest fit score
                filteredList1 = filteredList1.OrderBy(p => p.fitScore).ToList();
                bestpeak = filteredList1[0].peak;
            }

            return bestpeak;


        }

        private void addScoresToPeakQualityData(PeakQualityData pq, MassTagResultBase currentResult)
        {
            if (currentResult.IsotopicProfile == null)
            {
                pq.isotopicProfileFound = false;
                return;
            }
            else
            {
                pq.isotopicProfileFound = true;
                pq.abundance = currentResult.IsotopicProfile.IntensityAggregate;
                pq.fitScore = currentResult.Score;
                pq.i_score = currentResult.InterferenceScore;
            }
        }

        private ScanSet createSummedScanSet(ChromPeak chromPeak, Run run)
        {
            if (chromPeak == null || chromPeak.XValue == 0) return null;

            int bestScan = (int)chromPeak.XValue;
            bestScan = run.GetClosestMSScan(bestScan, Globals.ScanSelectionMode.CLOSEST);

            return new ScanSetFactory().CreateScanSet(run, bestScan, NumScansToSum);
        }
        #endregion

    }
}
