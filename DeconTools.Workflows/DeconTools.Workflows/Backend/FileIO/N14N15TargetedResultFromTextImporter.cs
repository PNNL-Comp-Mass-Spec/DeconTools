
using DeconTools.Workflows.Backend.Results;
namespace DeconTools.Workflows.Backend.FileIO
{
    public class N14N15TargetedResultFromTextImporter : TargetedResultFromTextImporter
    {
        readonly string[] _scanN15Headers = { "ScanN15" };
        readonly string[] _scanN15StartHeaders = { "scanN15Start" };
        readonly string[] _scanN15EndHeaders = { "ScanN15End" };
        readonly string[] _NETN15Headers = { "NETN15" };
        readonly string[] _numChromPeaksWithinTolN15Headers = { "ChromPeaksWithinTolN15", "NumChromPeaksWithinTolN15" };
        readonly string[] _numQualityChromPeaksWithinTolN15Headers = { "NumQualityChromPeaksWithinTolN15" };
        readonly string[] _monoMassN15Headers = { "MonoMassIso2", "MonoisotopicMassN15" };
        readonly string[] _monoMassN15CalibratedHeaders = { "MonoisotopicMassCalibratedN15" };
        readonly string[] _monoMZN15Headers = { "MonoMZN15", "MZ2" };
        readonly string[] _intensityN15Headers = { "AbundanceIso2", "IntensityN15" };
        readonly string[] _fitscoreN15Headers = { "iso2Fit" ,"FitScoreN15" };
        readonly string[] _iScoreN15Headers = { "iscore2" , "IScoreN15" };
        readonly string[] _ratioContribN14Headers = { "iso1RatioContrib" , "RatioContribN14" };
        readonly string[] _ratioContribN15Headers= { "iso2RatioContrib" , "RatioContribN15" };
        readonly string[] _ratioHeaders = { "ratio" };
        readonly string[] _monoMZN14Headers = { "MZ1" , "MonoMZ" };


        #region Constructors
        public N14N15TargetedResultFromTextImporter(string filename) : base(filename) { }
        #endregion

        protected override Results.TargetedResultDTO ConvertTextToDataObject(System.Collections.Generic.List<string> processedData)
        {
            TargetedResultDTO result = new N14N15TargetedResultDTO();

            GetBasicResultDTOData(processedData, result);

            var r = (N14N15TargetedResultDTO) result;

            r.ScanN15 = ParseIntField(LookupData(processedData, _scanN15Headers));
            r.ScanN15Start = ParseIntField(LookupData(processedData, _scanN15StartHeaders));
            r.ScanN15End = ParseIntField(LookupData(processedData, _scanN15EndHeaders));
            r.NETN15 = ParseFloatField(LookupData(processedData, _NETN15Headers));
            r.MonoMassN15 = ParseDoubleField(LookupData(processedData, _monoMassN15Headers));
            r.MonoMZ = ParseDoubleField(LookupData(processedData, _monoMZN14Headers));
            r.MonoMZN15 = ParseDoubleField(LookupData(processedData, _monoMZN15Headers));
            r.MonoMassCalibratedN15 = ParseDoubleField(LookupData(processedData, _monoMassN15CalibratedHeaders));
            r.IntensityN15 = ParseFloatField(LookupData(processedData, _intensityN15Headers));
            r.FitScoreN15 = ParseFloatField(LookupData(processedData, _fitscoreN15Headers));
            r.IScoreN15 = ParseFloatField(LookupData(processedData, _iScoreN15Headers));
            r.RatioContributionN14 = ParseFloatField(LookupData(processedData, _ratioContribN14Headers));
            r.RatioContributionN15 = ParseFloatField(LookupData(processedData, _ratioContribN15Headers));
            r.Ratio = ParseFloatField(LookupData(processedData, _ratioHeaders));

            r.NumChromPeaksWithinTolN15 = ParseShortField(LookupData(processedData, _numChromPeaksWithinTolN15Headers));
            r.NumQualityChromPeaksWithinTolN15 = ParseShortField(LookupData(processedData, _numQualityChromPeaksWithinTolN15Headers));
     
            return result;


        }

       
    }
}
