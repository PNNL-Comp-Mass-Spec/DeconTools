using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class ResultValidatorTask : Task
    {
        #region Constructors

        public ResultValidatorTask(double minRelIntensityForScore = 0.025, bool usePeakBasedInterferenceValue = true)
        {
            //create a default collection
            ResultValidatorColl = new List<ResultValidator>
            {
                new LeftOfMonoPeakLooker(),
                new IsotopicProfileInterferenceScorer(minRelIntensityForScore, usePeakBasedInterferenceValue)
            };
        }
        #endregion

        #region Properties

        IList<ResultValidator> ResultValidatorColl { get; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public override void Execute(ResultCollection resultList)
        {
            if (resultList.IsosResultBin == null || resultList.IsosResultBin.Count == 0) return;

            //iterate over each ms feature
            foreach (var msFeature in resultList.IsosResultBin)
            {
                //execute each validator
                foreach (var validator in ResultValidatorColl)
                {
                    validator.CurrentResult = msFeature;
                    validator.Execute(resultList);
                }
            }
        }
    }
}
