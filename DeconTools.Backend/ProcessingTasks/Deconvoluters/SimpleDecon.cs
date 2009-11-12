using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters
{
    public class SimpleDecon : IDeconvolutor
    {
        private double mzVar;

        public double MzVar
        {
            get { return mzVar; }
            set { mzVar = value; }
        }

        public SimpleDecon()
            : this(0.0005)
        {

        }

        public SimpleDecon(double mzVar)
        {
            this.mzVar = mzVar;
        }



        public override void deconvolute(ResultCollection resultList)
        {
            Task test = Project.getInstance().TaskCollection.GetTask(typeof(I_MSGenerator));

            List<MSPeak> localPeaklist = new List<MSPeak>(resultList.Run.MSPeakList);

            while (localPeaklist.Count > 0)
            {
                MSPeak currentPeak = findMostIntensePeak(localPeaklist);
                MSPeak plusOnePeak = findPlusOnePeak(currentPeak.MZ + 1.003, localPeaklist, this.mzVar);

                bool foundPlusOnePeak = (plusOnePeak != null);

                //if (foundPlusOnePeak)   //Julia Laskin requested that all peaks be outputed (Sept 21, 2009). So I'm removing this feature. 
                //{
                //    //TODO:  check for plusTwoPeak and reject if plusTwoPeak is present

                //    IsosResult result = resultList.CreateIsosResult();
                //    result.IsotopicProfile = new IsotopicProfile();
                //    result.IsotopicProfile.Peaklist.Add(currentPeak);
                //    result.IsotopicProfile.Peaklist.Add(plusOnePeak);
                //    result.IsotopicProfile.IntensityAggregate = currentPeak.Intensity;
                //    result.IsotopicProfile.ChargeState = 1;
                //    result.IsotopicProfile.AverageMass = 0;   //not calculating this...
                //    result.IsotopicProfile.MonoIsotopicMass = currentPeak.MZ - 1.00727649;   //  -proton mass 
                //    result.IsotopicProfile.MostAbundantIsotopeMass = result.IsotopicProfile.MonoIsotopicMass;
                //    result.IsotopicProfile.Score = 1;     //Score of '1' means found second isotopic peak
                   
                //    resultList.ResultList.Add(result);
                //    resultList.Run.CurrentScanSet.NumIsotopicProfiles++;

                //    localPeaklist.Remove(currentPeak);
                //    localPeaklist.Remove(plusOnePeak);
                //}
                //else    // didn't find +1 peak, so add the currentPeak to the results, but give it a score of 0 (unmatched)
                //{
                //    IsosResult result = resultList.CreateIsosResult();
                //    result.IsotopicProfile = new IsotopicProfile();
                //    result.IsotopicProfile.Peaklist.Add(currentPeak);
                //    result.IsotopicProfile.IntensityAggregate = currentPeak.Intensity;
                //    result.IsotopicProfile.ChargeState = 1;
                //    result.IsotopicProfile.AverageMass = 0;   //not calculating this...
                //    result.IsotopicProfile.MonoIsotopicMass = currentPeak.MZ - 1.00727649;   //  -proton mass 
                //    result.IsotopicProfile.MostAbundantIsotopeMass = result.IsotopicProfile.MonoIsotopicMass;
                //    result.IsotopicProfile.Score = 0;     //Score of '0' means the algorithm found only one peak.

                //    resultList.ResultList.Add(result);
                //    resultList.Run.CurrentScanSet.NumIsotopicProfiles++;

                //    localPeaklist.Remove(currentPeak);
                //}


                IsosResult result = resultList.CreateIsosResult();
                result.IsotopicProfile = new IsotopicProfile();
                result.IsotopicProfile.Peaklist.Add(currentPeak);
                result.IsotopicProfile.IntensityAggregate = currentPeak.Intensity;
                result.IsotopicProfile.ChargeState = 1;
                result.IsotopicProfile.AverageMass = 0;   //not calculating this...
                result.IsotopicProfile.MonoIsotopicMass = currentPeak.MZ - 1.00727649;   //  -proton mass 
                result.IsotopicProfile.MostAbundantIsotopeMass = result.IsotopicProfile.MonoIsotopicMass;
                result.IsotopicProfile.Score = 0;     //Score of '0' means the algorithm found only one peak.

                resultList.ResultList.Add(result);
                resultList.Run.CurrentScanSet.NumIsotopicProfiles++;

                localPeaklist.Remove(currentPeak);

            }
        }

        private MSPeak findPlusOnePeak(double targetMZ, List<MSPeak> localPeaklist, double mzVar)
        {
            MSPeak plusOnePeak = null;
            for (int i = 0; i < localPeaklist.Count; i++)
            {
                bool isPeakWithinMZVar = (Math.Abs(localPeaklist[i].MZ - targetMZ) <= mzVar);
                if (isPeakWithinMZVar)
                {
                    if (plusOnePeak == null)
                    {
                        plusOnePeak = localPeaklist[i];
                    }
                    else      //if there is another peak at +1 (rare occurance), pick the one closest to the target mz
                    {
                        double mzdiffCurrentPeak = Math.Abs(localPeaklist[i].MZ - targetMZ);
                        double mzdiffBestPlusOnePeak = Math.Abs(plusOnePeak.MZ - targetMZ);

                        
                        bool isPeakMZCloser = (mzdiffCurrentPeak < mzdiffBestPlusOnePeak);
                        if (isPeakMZCloser)
                        {
                            plusOnePeak = localPeaklist[i];
                        }
                        else
                        {
                            // do nothing
                        }
                    }
                }
            }
            return plusOnePeak;

        }

        private MSPeak findMostIntensePeak(List<MSPeak> localPeaklist)
        {
            MSPeak mostIntensePeak = new MSPeak();

            for (int i = 0; i < localPeaklist.Count; i++)
            {
                if (localPeaklist[i].Intensity > mostIntensePeak.Intensity)
                {
                    mostIntensePeak = localPeaklist[i];
                }

            }
            return mostIntensePeak;
        }


   
    }
}
