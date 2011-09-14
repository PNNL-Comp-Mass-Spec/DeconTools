using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;

namespace DeconTools.Backend.ProcessingTasks.TaskControllers
{
    public class N14N15_ChromatogramApproachTaskController:TaskController
    {
        MassTagCollection m_massTagCollection;

        #region Constructors

        public N14N15_ChromatogramApproachTaskController(MassTagCollection mtc)
        {
            this.m_massTagCollection = mtc;

                
        }


        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion


        public override void Execute(Run run)
        {
            LabeledMultiPeakChromGeneratorTask chromGenerator = new LabeledMultiPeakChromGeneratorTask(3, 25);



            ISmoother smoother = new Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);

            IPeakDetector peakDetector = new ChromPeakDetector(0.5, 0.5);

            ChromPeakSelector chromPeakSelector = new ChromPeakSelector(1, 0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);



            run.ResultCollection.ResultType = Globals.ResultType.N14N15_TARGETED_RESULT;
            List<MSPeakResult> masterMSPeakList = run.ResultCollection.MSPeakResultList;


            //get alignment info


            foreach (var mt in m_massTagCollection.MassTagList)
            {
                run.CurrentMassTag = mt;


                TomTheorFeatureGenerator featureGenerator = new TomTheorFeatureGenerator();
                featureGenerator.GenerateTheorFeature(mt);   //generate theor profile for unlabeled feature

                IsotopicProfile labeledProfile = N15IsotopeProfileGenerator.GetN15IsotopicProfile(mt, 0.005);

                IsotopicProfileChromData theorIsopeakChromData = new IsotopicProfileChromData();
                IsotopicProfileChromData isopeakChromData = new IsotopicProfileChromData();

                IsotopicProfileMultiChromatogramExtractor chromExtractor = new IsotopicProfileMultiChromatogramExtractor(3, 25);

                theorIsopeakChromData.ChromXYData = chromExtractor.GetChromatogramsForIsotopicProfilePeaks(masterMSPeakList, mt.IsotopicProfile);
                isopeakChromData.ChromXYData = chromExtractor.GetChromatogramsForIsotopicProfilePeaks(masterMSPeakList, labeledProfile);

                smoothChromatograms(theorIsopeakChromData.ChromXYData, smoother);
                smoothChromatograms(isopeakChromData.ChromXYData, smoother);


                theorIsopeakChromData.ChromPeakData = detectChromPeaks(theorIsopeakChromData.ChromXYData, peakDetector);
                isopeakChromData.ChromPeakData = detectChromPeaks(isopeakChromData.ChromXYData, peakDetector);


                getNETValues(theorIsopeakChromData.ChromPeakData, run);
                getNETValues(isopeakChromData.ChromPeakData, run);


                theorIsopeakChromData.ChromBestPeakData = selectBestChromPeaks(theorIsopeakChromData.ChromPeakData, chromPeakSelector, mt.NormalizedElutionTime, 0.1);
                isopeakChromData.ChromBestPeakData = selectBestChromPeaks(isopeakChromData.ChromPeakData, chromPeakSelector, mt.NormalizedElutionTime, 0.1);







            }
        }

        public override void Execute(List<Run> runCollection)
        {
            throw new NotImplementedException();

    
        }

        private void getNETValues(Dictionary<MSPeak, List<IPeak>> isoChromPeakData, Run run)
        {
            foreach (MSPeak msPeak in isoChromPeakData.Keys.ToList())
            {
                List<IPeak> chromPeaks = isoChromPeakData[msPeak];

                foreach (ChromPeak chrompeak in chromPeaks)
                {
                    chrompeak.NETValue =(double)run.GetNETValueForScan((int)chrompeak.XValue);
                    
                }


            }


        }

        private Dictionary<MSPeak, List<IPeak>> detectChromPeaks(Dictionary<MSPeak, XYData> isoChromXYData, IPeakDetector peakDetector)
        {
            //each MSPeak (from the isotopic profile) has chromatographic XY data associated with it.  Now will detect peaks within this data

            Dictionary<MSPeak, List<IPeak>> isoChromPeakData = new Dictionary<MSPeak, List<IPeak>>();

            foreach (MSPeak peak in isoChromXYData.Keys.ToList())
            {
                XYData xydata = isoChromXYData[peak];
                List<IPeak>chromPeakList = peakDetector.FindPeaks(xydata, 0, 0);

                isoChromPeakData.Add(peak, chromPeakList);
            }
            return isoChromPeakData;
        }

    

        private Dictionary<MSPeak,IPeak> selectBestChromPeaks(Dictionary<MSPeak,List<IPeak>>isoChromPeakData, ChromPeakSelector chromPeakSelector, float targetNET, double netTolerance)
        {
            Dictionary<MSPeak, IPeak> isoSelectedChromPeakData = new Dictionary<MSPeak, IPeak>();

            foreach (MSPeak peak in isoChromPeakData.Keys.ToList())
            {
                List<IPeak> peakData = isoChromPeakData[peak];
                IPeak bestPeak = chromPeakSelector.selectBestPeak(Globals.PeakSelectorMode.CLOSEST_TO_TARGET,peakData,targetNET,netTolerance);

                isoSelectedChromPeakData.Add(peak,bestPeak);
            }
            return isoSelectedChromPeakData;
        }

    
        private void smoothChromatograms(Dictionary<MSPeak,XYData>isoChromatograms, ISmoother smoother)
        {
            foreach (MSPeak peak in isoChromatograms.Keys.ToList())
            {
                XYData xydata = isoChromatograms[peak];
                if (xydata != null)
                {
                    isoChromatograms[peak] = smoother.Smooth(xydata);
                }
                else
                {

                }
            }

   

        }

    }
}
