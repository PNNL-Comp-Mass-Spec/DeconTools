using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class UnlabelledTargetedResultFromTextImporter : TargetedResultFromTextImporter
    {

        #region Constructors
        public UnlabelledTargetedResultFromTextImporter(string filename) : base(filename) { }

        #endregion

        protected override TargetedResult ConvertTextToDataObject(List<string> processedData)
        {
            UnlabelledTargetedResult result = new UnlabelledTargetedResult();

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
            result.NumQualityChromPeaksWithinTol = ParseIntField(LookupData(processedData, numQualitychromPeaksWithinTolHeaders));
            result.ScanLC = ParseIntField(LookupData(processedData, scanHeaders));
            result.ScanLCEnd = ParseIntField(LookupData(processedData, scanEndHeaders));
            result.ScanLCStart = ParseIntField(LookupData(processedData, scanStartHeaders));




            return result;



        }


    }
}
