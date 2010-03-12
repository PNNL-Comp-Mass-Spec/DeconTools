using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
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

            MSPeak monoPeak = currentResult.IsotopicProfile.getMonoPeak();


            MSPeak peakToTheLeft = lookforPeakToTheLeftOfMonoPeak(monoPeak, currentResult.IsotopicProfile.ChargeState, resultColl.Run.PeakList);
            if (peakToTheLeft == null) return;  // no peak found... so no problem.

            if (peakToTheLeft.Height > monoPeak.Height * MinRatioToGiveFlag)    //if peak-to-the-left exceeds min Ratio, then flag it
            {
                currentResult.Flags.Add(new PeakToTheLeftResultFlag());
            }


        }

        #endregion

        #region Private Methods
        private MSPeak lookforPeakToTheLeftOfMonoPeak(MSPeak monoPeak, int chargeState, List<IPeak> peakList)
        {
            double mzTol = monoPeak.Width;

            double targetMZ = monoPeak.XValue - (1.003 / (double)chargeState);

            foreach (MSPeak peak in peakList)
            {
                if (Math.Abs(peak.XValue - targetMZ) < mzTol)
                {
                    return peak;
                }

            }
            return null;



        }
        #endregion


    }
}
