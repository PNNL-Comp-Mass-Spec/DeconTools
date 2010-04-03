using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Algorithms;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public abstract class ITargetedFeatureFinder : Task
    {
        #region Properties
        public virtual IsotopicProfile TheorFeature { get; set; }

        public virtual double ToleranceInPPM { get; set; }

        /// <summary>
        /// If true, then FeatureFinder must find the monoIsotopic peak or no feature is reported. (Useful for most peptides or small MassTags)
        /// </summary>
        public virtual bool NeedMonoIsotopicPeak { get; set; }

        #endregion

        #region Public Methods
        public virtual void FindFeature(ResultCollection resultColl)
        {
            MassTagResultBase result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);
            if (result == null)
            {
                result = resultColl.CreateMassTagResult(resultColl.Run.CurrentMassTag);
            }

            if (result.ScanSet == null)
            {
                result.ScanSet = resultColl.Run.CurrentScanSet;

            }


            BasicMSFeatureFinder featureFinder = new BasicMSFeatureFinder();
            result.IsotopicProfile = featureFinder.FindMSFeature(resultColl.Run.PeakList, this.TheorFeature, this.ToleranceInPPM, this.NeedMonoIsotopicPeak);



            addInfoToResult(result);



            return;


        }

        private void addInfoToResult(MassTagResultBase result)
        {
            if (result.IsotopicProfile != null)
            {
                result.IsotopicProfile.ChargeState = result.MassTag.ChargeState;
                result.IsotopicProfile.MonoIsotopicMass = (result.IsotopicProfile.GetMZ() - Globals.PROTON_MASS) * result.MassTag.ChargeState;
                result.IsotopicProfile.IntensityAggregate = result.IsotopicProfile.getMostIntensePeak().Height;     // may need to change this to sum the top n peaks. 
            }
        }


        private IPeak findMostIntensePeak(List<IPeak> peaksWithinTol, double targetMZ)
        {
            double maxIntensity = 0;
            IPeak mostIntensePeak = null;

            for (int i = 0; i < peaksWithinTol.Count; i++)
            {
                float obsIntensity = peaksWithinTol[i].Height;
                if (obsIntensity > maxIntensity)
                {
                    maxIntensity = obsIntensity;
                    mostIntensePeak = peaksWithinTol[i];
                }
            }
            return mostIntensePeak;
        }


        private IPeak findClosestToXValue(List<IPeak> peaksWithinTol, double targetVal)
        {
            double diff = double.MaxValue;
            IPeak closestPeak = null;

            for (int i = 0; i < peaksWithinTol.Count; i++)
            {

                double obsDiff = Math.Abs(peaksWithinTol[i].XValue - targetVal);

                if (obsDiff < diff)
                {
                    diff = obsDiff;
                    closestPeak = peaksWithinTol[i];
                }

            }

            return closestPeak;
        }



        public virtual void GenerateTheorFeature(MassTag massTag)
        {
            this.TheorFeature = massTag.IsotopicProfile;
        }



        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList != null && resultList.Run != null, String.Format("{0} failed. Run is empty.", this.Name));
            Check.Require(resultList.Run.CurrentMassTag != null, String.Format("{0} failed. CurrentMassTag hasn't been defined.", this.Name));


            // generate theor feature
            GenerateTheorFeature(resultList.Run.CurrentMassTag);


            //
            FindFeature(resultList);


        }


    }
}
