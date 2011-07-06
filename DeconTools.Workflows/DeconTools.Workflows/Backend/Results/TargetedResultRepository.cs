using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Results
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



        public void AddResults(List<TargetedResult> featuresToAlign)
        {
            this.Results.AddRange(featuresToAlign);
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


        public void Clear()
        {
            this.Results.Clear();
        }



        public bool HasResults
        {
            get
            { return (this.Results != null && this.Results.Count > 0); }
        }
    }
}
