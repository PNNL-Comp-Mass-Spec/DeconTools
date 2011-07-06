using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class O16O18TargetedResultFromTextImporter : TargetedResultFromTextImporter
    {

        string[] intensityI2Headers = { "intensityI2", "i2" };
        string[] intensityI4Headers = { "intensityI4", "i4" };
        string[] intensityTheorIOHeaders = { "intensityTheorI0", "i0theor" };
        string[] intensityTheorI2Headers = { "intensityTheorI2", "i2theor" };
        string[] intensityTheorI4Headers = { "intensityTheorI4", "i4theor" };
        string[] intensityI4AdjustedHeaders = { "intensityI4Adjusted" };


        #region Constructors
        public O16O18TargetedResultFromTextImporter(string filename):base(filename){}
        #endregion

        protected override TargetedResult ConvertTextToDataObject(List<string> processedData)
        {
            O16O18TargetedResult result = new O16O18TargetedResult();

            result.DatasetName = LookupData(processedData, datasetHeaders);

            if (result.DatasetName == "-1")
            {
                result.DatasetName = TryGetDatasetNameFromFileName();
            }

            result.ChargeState = ParseIntField(LookupData(processedData, chargeStateHeaders));
            result.FitScore = ParseFloatField(LookupData(processedData, fitScoreHeaders));
            result.Intensity = ParseFloatField(LookupData(processedData, intensityRepHeaders));
            result.IntensityI0 = ParseFloatField(LookupData(processedData, intensityI0Headers));

            result.IScore = ParseFloatField(LookupData(processedData, iscoreHeaders));
            result.MassTagID = ParseLongField(LookupData(processedData, targetIDHeaders));
            result.MonoMass = ParseDoubleField(LookupData(processedData, monomassHeaders));
            result.MonoMZ = ParseDoubleField(LookupData(processedData, mzHeaders));
            result.NET = ParseFloatField(LookupData(processedData, netHeaders));
            result.NumChromPeaksWithinTol = ParseIntField(LookupData(processedData, numchromPeaksWithinTolHeaders));
            result.ScanLC = ParseIntField(LookupData(processedData, scanHeaders));
            result.ScanLCEnd = ParseIntField(LookupData(processedData, scanEndHeaders));
            result.ScanLCStart = ParseIntField(LookupData(processedData, scanStartHeaders));
            
            result.IntensityI2 = ParseFloatField(LookupData(processedData, intensityI2Headers));
            result.IntensityI4 = ParseFloatField(LookupData(processedData, intensityI4Headers));
            result.IntensityTheorI0 = ParseFloatField(LookupData(processedData, intensityTheorIOHeaders));
            result.IntensityTheorI2 = ParseFloatField(LookupData(processedData, intensityTheorI2Headers));
            result.IntensityTheorI4 = ParseFloatField(LookupData(processedData, intensityTheorI4Headers));
            result.IntensityI4Adjusted = ParseFloatField(LookupData(processedData, intensityI4AdjustedHeaders));

            return result;
        }
    }
}
