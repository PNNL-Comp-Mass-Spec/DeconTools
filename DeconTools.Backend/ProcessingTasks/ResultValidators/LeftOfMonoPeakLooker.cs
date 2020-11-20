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
            Check.Require(currentResult != null, string.Format("{0} failed. CurrentResult has not been defined.", this.Name));

            if (currentResult.IsotopicProfile == null) return;
            var monoPeak = currentResult.IsotopicProfile.getMonoPeak();

            var peakToTheLeft = LookforPeakToTheLeftOfMonoPeak(monoPeak, currentResult.IsotopicProfile.ChargeState, resultColl.Run.PeakList);
            if (peakToTheLeft == null) return;  // no peak found... so no problem.

            if (peakToTheLeft.Height > monoPeak.Height * MinRatioToGiveFlag)    //if peak-to-the-left exceeds min Ratio, then flag it
            {
                currentResult.Flags.Add(new PeakToTheLeftResultFlag());
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a 'peak-to-the-left' of the monoisotopic peak if: 1) it exists and 2) it is above the user provided relative intensity
        /// </summary>
        /// <param name="monoPeak"></param>
        /// <param name="chargeState"></param>
        /// <param name="peakList"></param>
        /// <param name="minRelIntensityForFlag"></param>
        /// <returns></returns>
        public MSPeak LookforPeakToTheLeftOfMonoPeak(MSPeak monoPeak, int chargeState, List<Peak> peakList, double minRelIntensityForFlag)
        {
            double mzTol = monoPeak.Width;

            var targetMZ = monoPeak.XValue - (1.003 / (double)chargeState);

            var foundLeftOfMonoPeaks = PeakUtilities.GetPeaksWithinTolerance(peakList, targetMZ, mzTol);

            //if found a peak to the left, will return that peak. If

            if (foundLeftOfMonoPeaks.Count == 0)
            {
                return null;
            }

            var peakToTheLeft = foundLeftOfMonoPeaks.OrderByDescending(p => p.Height).First() as MSPeak;

            if (peakToTheLeft == null) return null;

            if (peakToTheLeft.Height > monoPeak.Height * MinRatioToGiveFlag)
            {
                return peakToTheLeft;
            }

            return null;
        }

        public MSPeak LookforPeakToTheLeftOfMonoPeak(MSPeak monoPeak, int chargeState, List<Peak> peakList)
        {
            return LookforPeakToTheLeftOfMonoPeak(monoPeak, chargeState, peakList, this.MinRatioToGiveFlag);
        }
        #endregion

    }
}
