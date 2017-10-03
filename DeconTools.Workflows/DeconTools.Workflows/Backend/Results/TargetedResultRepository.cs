using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Results
{
    public class TargetedResultRepository
    {

        #region Constructors
        public TargetedResultRepository()
        {
            this.Results = new List<TargetedResultDTO>();
        }
        #endregion

        #region Properties
        public List<TargetedResultDTO> Results { get; set; }


        #endregion

        public void AddResult(TargetedResultBase resultToConvert)
        {
            var result = ResultDTOFactory.CreateTargetedResult(resultToConvert);
            this.Results.Add(result);
        }



        public void AddResults(List<TargetedResultDTO> featuresToAlign)
        {
            this.Results.AddRange(featuresToAlign);
        }

        public void AddResults(List<TargetedResultBase> resultsToConvert)
        {
            foreach (var item in resultsToConvert)
            {
                var result = ResultDTOFactory.CreateTargetedResult(item);
                this.Results.Add(result);
            }
        }


        public void Clear()
        {
            this.Results.Clear();
        }



        public bool HasResults => (Results != null && Results.Count > 0);
    }
}
