using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class IsotopicProfileInterferenceScorer : ResultValidator
    {
        InterferenceScorer m_scorer;

        #region Constructors
        public IsotopicProfileInterferenceScorer()
        {
            m_scorer = new InterferenceScorer();
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion


        public override DeconTools.Backend.Core.IsosResult CurrentResult { get; set; }

        public override void ValidateResult(DeconTools.Backend.Core.ResultCollection resultColl, DeconTools.Backend.Core.IsosResult currentResult)
        {
            Check.Require(currentResult != null, String.Format("{0} failed. CurrentResult has not been defined.", this.Name));

            if (currentResult.IsotopicProfile == null) return;
            MSPeak monoPeak = currentResult.IsotopicProfile.getMonoPeak();
            MSPeak lastPeak = currentResult.IsotopicProfile.Peaklist[currentResult.IsotopicProfile.Peaklist.Count - 1];

            double leftMZBoundary = monoPeak.XValue - 1.1;
            double rightMZBoundary = lastPeak.XValue + lastPeak.Width / 2.35 * 2;      // 2 sigma

            int startIndexOfXYData = MathUtils.BinarySearchWithTolerance(resultColl.Run.XYData.Xvalues, monoPeak.XValue - 3, 0, (resultColl.Run.XYData.Xvalues.Length - 1), 2);
            if (startIndexOfXYData < 0)
            {
                startIndexOfXYData = 0;
            }

            double interferenceVal = m_scorer.GetInterferenceScore(resultColl.Run.XYData, currentResult.IsotopicProfile.Peaklist, leftMZBoundary, rightMZBoundary, startIndexOfXYData);

            currentResult.InterferenceScore = interferenceVal;

        }
    }
}
