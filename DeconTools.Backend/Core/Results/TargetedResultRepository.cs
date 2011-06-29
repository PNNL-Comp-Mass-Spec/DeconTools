using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core.Results
{
    public class TargetedResultRepository
    {

        #region Constructors
        public TargetedResultRepository()
        {
            this.Results = new List<TargetedResult>();
        }
        #endregion

        #region Properties
        public List<TargetedResult> Results { get; set; }


        #endregion

        public void AddResult(MassTagResultBase resultToConvert)
        {
            TargetedResult result = ResultFactory.CreateTargetedResult(resultToConvert);
            this.Results.Add(result);
        }

        public void AddResults(List<MassTagResultBase> resultsToConvert)
        {
            foreach (var item in resultsToConvert)
            {
                TargetedResult result = ResultFactory.CreateTargetedResult(item);
                this.Results.Add(result);
            }
        }



        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
