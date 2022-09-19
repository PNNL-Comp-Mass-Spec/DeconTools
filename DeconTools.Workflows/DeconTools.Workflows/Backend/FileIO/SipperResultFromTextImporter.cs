using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class SipperResultFromTextImporter : TargetedResultFromTextImporter
    {
        private readonly string[] _areaUnderDiffCurveHeaders ={"AreaDifferenceCurve"};
        private readonly string[] _areaUnderRatioCurveHeaders = {"AreaRatioCurve"};
        private readonly string[] _areaUnderRatioRevisedHeaders = { "AreaRatioCurveRevised" };
        private readonly string[] _RSquaredForRatio = { "RSquared" };
        private readonly string[] _chromCorrMinHeaders = {"ChromCorrMin"};
        private readonly string[] _chromCorrMaxHeaders = { "ChromCorrMax" };
        private readonly string[] _chromCorrAverageHeaders = { "ChromCorrAverage" };
        private readonly string[] _chromCorrMedianHeaders = { "ChromCorrMedian" };
        private readonly string[] _chromCorrStdevHeaders = { "ChromCorrStdev" };
        private readonly string[] _numCarbonsLabeledHeaders = { "NumCarbonsLabeled", "NumCarbonsLabelled" , "AmountLabelling"};
        private readonly string[] _percentPeptidesLabeledHeaders = { "PercentPeptidesLabeled", "PercentPeptidesLabelled", "FractionLabel" };
        private readonly string[] _percentCarbonsLabeledHeaders = { "PercentCarbonsLabeled", "PercentCarbonsLabelled"};
        private readonly string[] _numHQProfilePeaksHeaders = { "NumHQProfilePeaks" };
        private readonly string[] _labelDistribHeaders = { "LabelDistributionData" };
        private readonly string[] _fitScoreLabeledHeaders = {"FitScoreLabeled", "FitScoreLabelled"};
        private readonly string[] _contigScoreHeaders = {"ContigScore"};

        public SipperResultFromTextImporter(string filename) : base(filename) { }

        protected override TargetedResultDTO ConvertTextToDataObject(List<string> processedData)
        {
            var result = new SipperLcmsFeatureTargetedResultDTO();
            result.DatasetName = LookupData(processedData, datasetHeaders);

            if (result.DatasetName == "-1")
            {
                result.DatasetName = TryGetDatasetNameFromFileName();
            }

            GetBasicResultDTOData(processedData, result);

            result.AreaUnderDifferenceCurve = ParseDoubleField(LookupData(processedData, _areaUnderDiffCurveHeaders));

            result.AreaUnderRatioCurve = ParseDoubleField(LookupData(processedData, _areaUnderRatioCurveHeaders));
            result.AreaUnderRatioCurveRevised = ParseDoubleField(LookupData(processedData, _areaUnderRatioRevisedHeaders));
            result.MatchedMassTagID = ParseIntField(LookupData(processedData, matchedMassTagIDHeaders));
            result.ChromCorrelationMin = ParseDoubleField(LookupData(processedData, _chromCorrMinHeaders));
            result.ChromCorrelationMax = ParseDoubleField(LookupData(processedData, _chromCorrMaxHeaders));
            result.ChromCorrelationAverage = ParseDoubleField(LookupData(processedData, _chromCorrAverageHeaders));
            result.ChromCorrelationMedian = ParseDoubleField(LookupData(processedData, _chromCorrMedianHeaders));
            result.ChromCorrelationStdev = ParseDoubleField(LookupData(processedData, _chromCorrStdevHeaders));
            result.PercentPeptideLabeled = ParseDoubleField(LookupData(processedData, _percentPeptidesLabeledHeaders));
            result.NumCarbonsLabeled = ParseDoubleField(LookupData(processedData, _numCarbonsLabeledHeaders));
            result.PercentCarbonsLabeled = ParseDoubleField(LookupData(processedData, _percentCarbonsLabeledHeaders));
            result.NumHighQualityProfilePeaks = ParseIntField(LookupData(processedData, _numHQProfilePeaksHeaders));
            result.LabelDistributionVals = ConvertLabelDistStringToArray(LookupData(processedData, _labelDistribHeaders));
            result.FitScoreLabeledProfile = ParseDoubleField(LookupData(processedData, _fitScoreLabeledHeaders));
            result.ContiguousnessScore = ParseIntField(LookupData(processedData, _contigScoreHeaders));
            result.RSquaredValForRatioCurve = ParseDoubleField(LookupData(processedData, _RSquaredForRatio));
            return result;
        }

        private double[] ConvertLabelDistStringToArray(string labelDistString)
        {
            if (string.IsNullOrEmpty(labelDistString))
            {
                return null;
            }

            var delimiter = ',';
            var parsedLabelDistString = labelDistString.Split(delimiter);

            var labelDistVals = new List<double>();

            foreach (var s in parsedLabelDistString)
            {
                var parsedOk = double.TryParse(s, out var val);

                if (parsedOk)
                {
                    labelDistVals.Add(val);
                }
                else
                {
                    return null;
                }
            }

            return labelDistVals.ToArray();
        }
    }
}
