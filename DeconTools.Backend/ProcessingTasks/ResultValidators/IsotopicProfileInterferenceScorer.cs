using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class IsotopicProfileInterferenceScorer : ResultValidator
    {
        InterferenceScorer m_scorer;

        #region Constructors
      
        public IsotopicProfileInterferenceScorer(double minRelIntensityForScore = 0.025,  bool usePeakBasedInterferenceValue=true)
        {
            m_scorer = new InterferenceScorer(minRelIntensityForScore);
            UsePeakBasedInterferenceValue = usePeakBasedInterferenceValue;
        }

        #endregion

        #region Properties
        public bool UsePeakBasedInterferenceValue { get; set; }
        #endregion

        #region Public Methods

        public override DeconTools.Backend.Core.IsosResult CurrentResult { get; set; }

        public override void ValidateResult(DeconTools.Backend.Core.ResultCollection resultColl, DeconTools.Backend.Core.IsosResult currentResult)
        {

            if (currentResult.IsotopicProfile == null) return;

            Check.Require(currentResult != null, String.Format("{0} failed. CurrentResult has not been defined.", this.Name));
            if(resultColl.Run.PeakList == null || resultColl.Run.PeakList.Count == 0)
            {
                currentResult.InterferenceScore = -1;
                return;
            }

            
            MSPeak monoPeak = currentResult.IsotopicProfile.getMonoPeak();
            MSPeak lastPeak = currentResult.IsotopicProfile.Peaklist[currentResult.IsotopicProfile.Peaklist.Count - 1];

            double leftMZBoundary = monoPeak.XValue - 1.1;
            double rightMZBoundary = lastPeak.XValue + lastPeak.Width / 2.35 * 2;      // 2 sigma

            double interferenceVal = -1;
            if (UsePeakBasedInterferenceValue)
            {
                List<MSPeak> scanPeaks = resultColl.Run.PeakList.Select<Peak, MSPeak>(i => (MSPeak)i).ToList();
                interferenceVal = m_scorer.GetInterferenceScore(scanPeaks, currentResult.IsotopicProfile.Peaklist, leftMZBoundary, rightMZBoundary);
            }
            else
            {
                int startIndexOfXYData = MathUtils.BinarySearchWithTolerance(resultColl.Run.XYData.Xvalues, monoPeak.XValue - 3, 0, (resultColl.Run.XYData.Xvalues.Length - 1), 2);
                if (startIndexOfXYData < 0)
                {
                    startIndexOfXYData = 0;
                }

                interferenceVal = m_scorer.GetInterferenceScore(resultColl.Run.XYData, currentResult.IsotopicProfile.Peaklist, leftMZBoundary, rightMZBoundary, startIndexOfXYData);

            }



            currentResult.InterferenceScore = interferenceVal;

        }

        #endregion

        #region Private Methods

        #endregion


        
    }
}
