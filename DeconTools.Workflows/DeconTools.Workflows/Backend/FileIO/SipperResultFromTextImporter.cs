using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class SipperResultFromTextImporter : TargetedResultFromTextImporter
    {
        private string[] _areaUnderDiffCurveHeaders ={"AreaDifferenceCurve"};
        private string[] _areaUnderRatioCurveHeaders = {"AreaRatioCurve"};
        private string[] _areaUnderRatioRevisedHeaders = { "AreaRatioCurveRevised" };
        private string[] _RSquaredForRatio = { "RSquared" };
        private string[] _chromCorrMinHeaders = {"ChromCorrMin"};
        private string[] _chromCorrMaxHeaders = { "ChromCorrMax" };
        private string[] _chromCorrAverageHeaders = { "ChromCorrAverage" };
        private string[] _chromCorrMedianHeaders = { "ChromCorrMedian" };
        private string[] _chromCorrStdevHeaders = { "ChromCorrStdev" };
        private string[] _numCarbonsLabelledHeaders = {"NumCarbonsLabelled" , "AmountLabelling"};
        private string[] _percentPeptidesLabelledHeaders = { "PercentPeptidesLabelled", "FractionLabel" };
        private string[] _percentCarbonsLabelledHeaders = { "PercentCarbonsLabelled"};
        
        private string[] _numHQProfilePeaksHeaders = { "NumHQProfilePeaks" };

        public SipperResultFromTextImporter(string filename) : base(filename) { }


        protected override Results.TargetedResultDTO ConvertTextToDataObject(List<string> processedData)
        {
            SipperLcmsFeatureTargetedResultDTO result = new SipperLcmsFeatureTargetedResultDTO();
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
            result.RSquaredValForRatioCurve = ParseDoubleField(LookupData(processedData, _RSquaredForRatio));
            result.ChromCorrelationMin = ParseDoubleField(LookupData(processedData, _chromCorrMinHeaders));
            result.ChromCorrelationMax = ParseDoubleField(LookupData(processedData, _chromCorrMaxHeaders));
            result.ChromCorrelationAverage = ParseDoubleField(LookupData(processedData, _chromCorrAverageHeaders));
            result.ChromCorrelationMedian = ParseDoubleField(LookupData(processedData, _chromCorrMedianHeaders));
            result.ChromCorrelationStdev = ParseDoubleField(LookupData(processedData, _chromCorrStdevHeaders));
            result.PercentPeptideLabelled = ParseDoubleField(LookupData(processedData, _percentPeptidesLabelledHeaders));
            result.NumCarbonsLabelled = ParseDoubleField(LookupData(processedData, _numCarbonsLabelledHeaders));
            result.PercentCarbonsLabelled = ParseDoubleField(LookupData(processedData, _percentCarbonsLabelledHeaders));
            result.NumHighQualityProfilePeaks = ParseIntField(LookupData(processedData, _numHQProfilePeaksHeaders));
           
            
            return result;

        }
    }
}
