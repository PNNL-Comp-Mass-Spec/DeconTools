using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;

namespace DeconTools.Backend.ProcessingTasks.TaskControllers
{
    public class N14N15_CustomWorkflow1
    {
        Run run;
        MassTag mt;

        Task msGenerator;
        TomTheorFeatureGenerator theorGenerator;
        ChromatogramGenerator chromGen;
        DeconToolsSavitzkyGolaySmoother smoother;
        ChromPeakDetector chromPeakDetector;
        ChromPeakSelector chromPeakSel;


        #region Constructors
        public N14N15_CustomWorkflow1(Run run, MassTag mt)
        {
            this.run = run;
            this.mt = mt;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            msGenerator = msgenFactory.CreateMSGenerator(run.MSFileType);

        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods


        public N14N15ResultObject GetN14N15Result()
        {
            double chromToleranceInPPM = 30;
            double theorProfileIntensityCutoff = 0.3;
            int smootherLeftParam = 11;
            int smootherRightParam = 11;
            int smootherOrder = 2;
           
            double chromPeakDetectorPeakBR = 0.5;
            double chromPeakDetectorSignNoise = 0.5;

            double chromPeakSelectorNETTolerance = 0.01;
            Globals.PeakSelectorMode chromPeakSelectorMode = Globals.PeakSelectorMode.CLOSEST_TO_TARGET;
            int chromPeakSelectorNumLCScansSummed = 1;

            //results are stored here:
            N14N15ResultObject n14N15Result = new N14N15ResultObject(this.run.DatasetName,this.mt);

            //instantiate algorithms
            theorGenerator = new TomTheorFeatureGenerator();
            chromGen = new ChromatogramGenerator();
            smoother = new DeconToolsSavitzkyGolaySmoother(smootherLeftParam, smootherRightParam, smootherOrder);
            chromPeakDetector = new ChromPeakDetector(chromPeakDetectorPeakBR, chromPeakDetectorSignNoise);
            chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(chromPeakSelectorNumLCScansSummed, chromPeakSelectorNETTolerance, chromPeakSelectorMode);

            //0. Get theor profile for unlabeled
            theorGenerator.GenerateTheorFeature(this.mt);
            List<MSPeak> topTheorPeaks = IsotopicProfileUtilities.GetTopMSPeaks(this.mt.IsotopicProfile.Peaklist, theorProfileIntensityCutoff);

            //1. Get chroms for unlabelled.  Use theor. most intense peaks. 
            foreach (var peak in topTheorPeaks)
            {
                XYData chrom = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, peak.XValue, chromToleranceInPPM);
                n14N15Result.ChromListUnlabeled.Add(chrom);
            }

            for (int i = 0; i < n14N15Result.ChromListUnlabeled.Count; i++)
            {
                //2. Smooth each unlabelled chrom
                n14N15Result.ChromListUnlabeled[i] = smoother.Smooth(n14N15Result.ChromListUnlabeled[i]);

                //3. Detect chrom peaks + add NET data (based on run's alignment info)
                List<IPeak> peakList = chromPeakDetector.FindPeaks(n14N15Result.ChromListUnlabeled[i], run.MinScan, run.MaxScan);

                addNETDataToChromPeaks(run, peakList);

                //4. Select best chrom peak
                IPeak selectedChromPeak = chromPeakSel.selectBestPeak(chromPeakSelectorMode, peakList, mt.NETVal, chromPeakSelectorNETTolerance);
                n14N15Result.ChromPeakSelectedUnlabeled.Add(selectedChromPeak);
            }


            //n14N15Result.DisplaySelectedChromPeaks(n14N15Result.ChromPeakSelectedUnlabeled);
            
            
            
            //6. get theor N15 profile
            IsotopicProfile theorN14N15Profile =   N15IsotopeProfileGenerator.GetN15IsotopicProfile(mt, 0.005);

            //7. Repeat steps 1-5 for labelled profile. (see below) 
            
            //7.1  Get chroms for labelled. Use n14N15 theor most intense peaks
            List<MSPeak> topN14N15TheorPeaks = IsotopicProfileUtilities.GetTopMSPeaks(theorN14N15Profile.Peaklist, theorProfileIntensityCutoff);
            foreach (var peak in topN14N15TheorPeaks)
            {
                XYData chrom = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, peak.XValue, chromToleranceInPPM);
                n14N15Result.ChromListLabeled.Add(chrom);
            }




            for (int i = 0; i < n14N15Result.ChromListLabeled.Count; i++)
            {
                //7.2 Smooth each labelled chrom
                n14N15Result.ChromListLabeled[i] = smoother.Smooth(n14N15Result.ChromListLabeled[i]);

                //7.3 Detect chrom peaks + add NET data (based on run's alignment info)
                List<IPeak> peakList = chromPeakDetector.FindPeaks(n14N15Result.ChromListLabeled[i], run.MinScan, run.MaxScan);

                addNETDataToChromPeaks(run, peakList);

                //7.4 Select best chrom peak
                IPeak selectedChromPeak = chromPeakSel.selectBestPeak(chromPeakSelectorMode, peakList, mt.NETVal, chromPeakSelectorNETTolerance);
                n14N15Result.ChromPeakSelectedLabeled.Add(selectedChromPeak);

            }

            //n14N15Result.DisplaySelectedChromPeaks(n14N15Result.ChromPeakSelectedLabeled);

            double scanWeightedAverageUnlabeled = getScanWeightedAverage(n14N15Result.ChromPeakSelectedUnlabeled);


            //8. Calculate N14/N15 ratio from 1) chrom data   2) MS data.
            //9. Flag possible overlapping distributions.  Have a way of looking within the isotopic profile m/z range for all peaks detected and determine the
            //percent contribution of the found N14/N15 isotopic profile. If this is above a certain level, use it. If not, flag it. 

            //10. store data somehow!  

            return n14N15Result;

        }

        private double getScanWeightedAverage(List<IPeak> selectedChromList)
        {
            throw new NotImplementedException();
        }

        private void addNETDataToChromPeaks(Run run, List<IPeak> peakList)
        {
            foreach (ChromPeak chrompeak in peakList)
            {
                chrompeak.NETValue = run.GetNETValueForScan((int)chrompeak.XValue);
                
            }
        }


  
        #endregion

        #region Private Methods
        #endregion
    }
}
