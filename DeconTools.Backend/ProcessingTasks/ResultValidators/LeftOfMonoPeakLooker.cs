using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class LeftOfMonoPeakLooker : ResultValidator
    {
        #region Constructors
        public LeftOfMonoPeakLooker()
            : this(0.5)
        {

        }

        public LeftOfMonoPeakLooker(double minRatioToGiveFlag)
        {
            this.Name = "LeftOfMonoPeakLooker Validator";
            this.MinRatioToGiveFlag = minRatioToGiveFlag;
        }
        #endregion

        #region Properties
        public override IsosResult CurrentResult { get; set; }
        public double MinRatioToGiveFlag { get; set; }
        #endregion

        #region Public Methods
        public override void ValidateResult(DeconTools.Backend.Core.ResultCollection resultColl, IsosResult currentResult)
        {
            Check.Require(currentResult != null, String.Format("{0} failed. CurrentResult has not been defined.", this.Name));

            if (currentResult.IsotopicProfile == null) return;
            MSPeak monoPeak = currentResult.IsotopicProfile.getMonoPeak();

            MSPeak peakToTheLeft = LookforPeakToTheLeftOfMonoPeak(monoPeak, currentResult.IsotopicProfile.ChargeState, resultColl.Run.PeakList);
            if (peakToTheLeft == null) return;  // no peak found... so no problem.

            if (peakToTheLeft.Height > monoPeak.Height * MinRatioToGiveFlag)    //if peak-to-the-left exceeds min Ratio, then flag it
            {
                currentResult.Flags.Add(new PeakToTheLeftResultFlag());
            }


        }

        #endregion

        #region Private Methods
        private MSPeak LookforPeakToTheLeftOfMonoPeak(MSPeak monoPeak, int chargeState, List<Peak> peakList)
        {
            double mzTol = monoPeak.Width;

            double targetMZ = monoPeak.XValue - (1.003 / (double)chargeState);


            var foundLeftOfMonoPeaks=  PeakUtilities.GetPeaksWithinTolerance(peakList, targetMZ, mzTol);

            //if found a peak to the left, will return that peak. If 
           
            if (foundLeftOfMonoPeaks.Count==0)
            {
                return null;
            }
            
            if (foundLeftOfMonoPeaks.Count==1)
            {
                return (MSPeak) foundLeftOfMonoPeaks.First();
            }
            
            return (MSPeak) foundLeftOfMonoPeaks.OrderByDescending(p => p.Height).First();
        }
        #endregion


    }
}
