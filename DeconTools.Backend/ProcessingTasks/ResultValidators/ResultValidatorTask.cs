using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class ResultValidatorTask : Task
    {
        #region Constructors

        public ResultValidatorTask()
        {
            //create a default collection
            ResultValidatorColl = new List<ResultValidator>();
            ResultValidatorColl.Add(new LeftOfMonoPeakLooker());
            ResultValidatorColl.Add(new IsotopicProfileInterferenceScorer());


        }
        #endregion

        #region Properties

        IList<ResultValidator> ResultValidatorColl { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultColl)
        {
            if (resultColl.IsosResultBin == null || resultColl.IsosResultBin.Count == 0) return;

            //iterate over each ms feature
            foreach (var msFeature in resultColl.IsosResultBin)
            {

                //execute each validator
                foreach (var validator in ResultValidatorColl)
                {
                    validator.CurrentResult = msFeature;
                    validator.Execute(resultColl);

                }



            }


        }
    }
}
