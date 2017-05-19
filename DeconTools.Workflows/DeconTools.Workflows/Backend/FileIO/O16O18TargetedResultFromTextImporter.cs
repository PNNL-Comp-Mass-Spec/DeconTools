using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class O16O18TargetedResultFromTextImporter : TargetedResultFromTextImporter
    {
        readonly string[] _intensityI2Headers = { "intensityI2", "i2" };
        readonly string[] _intensityI4Headers = { "intensityI4", "i4" };
        readonly string[] _intensityTheorIoHeaders = { "intensityTheorI0", "i0theor" };
        readonly string[] _intensityTheorI2Headers = { "intensityTheorI2", "i2theor" };
        readonly string[] _intensityTheorI4Headers = { "intensityTheorI4", "i4theor" };
        readonly string[] _intensityI4AdjustedHeaders = { "intensityI4Adjusted" };
        readonly string[] _ratioHeaders = {"ratio"};
        readonly string[] _chromCorrO16O18SingleLabelHeaders = {"ChromCorrO16O18SingleLabel"};
        readonly string[] _chromCorrO16O18DoubleLabelHeaders = { "ChromCorrO16O18DoubleLabel" };
        readonly string[] _ratioChromCorrHeaders = {"RatioFromChromCorr"};

        #region Constructors
        public O16O18TargetedResultFromTextImporter(string filename) : base(filename) { }
        #endregion

        protected override TargetedResultDTO ConvertTextToDataObject(List<string> processedData)
        {
            var result = new O16O18TargetedResultDTO();

            result.DatasetName = LookupData(processedData, datasetHeaders);

            if (result.DatasetName == "-1")
            {
                result.DatasetName = TryGetDatasetNameFromFileName();
            }

            GetBasicResultDTOData(processedData, result);

            result.IntensityI2 = ParseFloatField(LookupData(processedData, _intensityI2Headers));
            result.IntensityI4 = ParseFloatField(LookupData(processedData, _intensityI4Headers));
            result.IntensityTheorI0 = ParseFloatField(LookupData(processedData, _intensityTheorIoHeaders));
            result.IntensityTheorI2 = ParseFloatField(LookupData(processedData, _intensityTheorI2Headers));
            result.IntensityTheorI4 = ParseFloatField(LookupData(processedData, _intensityTheorI4Headers));
            result.IntensityI4Adjusted = ParseFloatField(LookupData(processedData, _intensityI4AdjustedHeaders));
            result.Ratio = ParseFloatField(LookupData(processedData, _ratioHeaders));
            result.RatioFromChromCorr = ParseFloatField(LookupData(processedData, _ratioChromCorrHeaders));
            result.ChromCorrO16O18SingleLabel = ParseFloatField(LookupData(processedData, _chromCorrO16O18SingleLabelHeaders));
            result.ChromCorrO16O18DoubleLabel = ParseFloatField(LookupData(processedData, _chromCorrO16O18DoubleLabelHeaders));

            return result;
        }
    }
}
