using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class TopDownTargetedResultFromTextImporter:TargetedResultFromTextImporter
    {

        #region Constructors

        public TopDownTargetedResultFromTextImporter(string filename): base(filename){ }

        #endregion

   
        #region Public Methods
        protected override TargetedResultDTO ConvertTextToDataObject(List<string> processedData)
        {
            TargetedResultDTO result = new TopDownTargetedResultDTO();

            GetBasicResultDTOData(processedData, result);
            return result;
        }
        #endregion

      
     

      
    }
}
