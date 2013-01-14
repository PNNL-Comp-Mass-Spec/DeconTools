using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class DeuteratedTargetedResultFromTextImporter : TargetedResultFromTextImporter
    {

        string[] intensityI2Headers = { "intensityI2", "i2" };
        string[] intensityI4Headers = { "intensityI4", "i4" };
        string[] intensityTheorIOHeaders = { "intensityTheorI0", "i0theor" };
        string[] intensityTheorI2Headers = { "intensityTheorI2", "i2theor" };
        string[] intensityTheorI4Headers = { "intensityTheorI4", "i4theor" };
        string[] intensityI4AdjustedHeaders = { "intensityI4Adjusted" };
        string[] ratioHeaders = {"ratio"};


        #region Constructors
        public DeuteratedTargetedResultFromTextImporter(string filename) : base(filename) { }
        #endregion

        protected override TargetedResultDTO ConvertTextToDataObject(List<string> processedData)
        {
            O16O18TargetedResultDTO result = new O16O18TargetedResultDTO();

            result.DatasetName = LookupData(processedData, datasetHeaders);

            if (result.DatasetName == "-1")
            {
                result.DatasetName = TryGetDatasetNameFromFileName();
            }

            GetBasicResultDTOData(processedData, result);

            result.IntensityI2 = ParseFloatField(LookupData(processedData, intensityI2Headers));
            result.IntensityI4 = ParseFloatField(LookupData(processedData, intensityI4Headers));
            result.IntensityTheorI0 = ParseFloatField(LookupData(processedData, intensityTheorIOHeaders));
            result.IntensityTheorI2 = ParseFloatField(LookupData(processedData, intensityTheorI2Headers));
            result.IntensityTheorI4 = ParseFloatField(LookupData(processedData, intensityTheorI4Headers));
            result.IntensityI4Adjusted = ParseFloatField(LookupData(processedData, intensityI4AdjustedHeaders));
            result.Ratio = ParseFloatField(LookupData(processedData, ratioHeaders));

            return result;
        }
    }
}
