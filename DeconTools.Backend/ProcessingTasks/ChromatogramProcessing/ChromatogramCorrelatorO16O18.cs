using System.Linq;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class ChromatogramCorrelatorO16O18 : ChromatogramCorrelatorBase
    {
        public ChromatogramCorrelatorO16O18(int numPointsInSmoother, double minRelativeIntensityForChromCorr = 0.00001, double chromToleranceInPPM = 20)
            : base(numPointsInSmoother, minRelativeIntensityForChromCorr, chromToleranceInPPM)
        {

        }

        public override ChromCorrelationData CorrelateData(Run run, TargetedResultBase result, int startScan, int stopScan)
        {

            var chromCorrData=   CorrelateO16O18Profiles(run, result.IsotopicProfile, startScan, stopScan);

            var o16O18Result = result as O16O18TargetedResultObject;

            if (o16O18Result!=null)
            {
                if (chromCorrData.CorrelationDataItems.Any())
                {
                    o16O18Result.ChromCorrO16O18SingleLabel= chromCorrData.CorrelationDataItems[0].CorrelationRSquaredVal;
                    o16O18Result.ChromCorrO16O18DoubleLabel = chromCorrData.CorrelationDataItems[1].CorrelationRSquaredVal;
                }
            }


            return chromCorrData;


        }


        public ChromCorrelationData CorrelateO16O18Profiles(Run run, IsotopicProfile iso, int startScan, int stopScan)
        {
            var correlationData = new ChromCorrelationData();
            var indexO16MonoPeak = 0;
            var indexO18SingleLabelPeak = 2;
            var indexO18DoubleLabelPeak = 4;

            var baseMZValue = iso.Peaklist[indexO16MonoPeak].XValue;
            bool baseChromDataIsOK;
            var basePeakChromXYData = GetBaseChromXYData(run, startScan, stopScan, baseMZValue);

            baseChromDataIsOK = basePeakChromXYData != null && basePeakChromXYData.Xvalues != null &&
                                 basePeakChromXYData.Xvalues.Length > 3;

            if (!baseChromDataIsOK) return new ChromCorrelationData();

            ChromCorrelationDataItem o18SingleLabelChromCorrDataItem;
            if (indexO18SingleLabelPeak >= iso.Peaklist.Count)
            {
                o18SingleLabelChromCorrDataItem = new ChromCorrelationDataItem();
            }
            else
            {
                var correlatedO18SingleLabelMzValue = iso.Peaklist[indexO18SingleLabelPeak].XValue;
                o18SingleLabelChromCorrDataItem = GetChromCorrDataItem(run, startScan, stopScan, basePeakChromXYData, correlatedO18SingleLabelMzValue);
            }

            ChromCorrelationDataItem o18DoubleLabelChromCorrDataItem;
            if (indexO18DoubleLabelPeak >= iso.Peaklist.Count)
            {
                o18DoubleLabelChromCorrDataItem = new ChromCorrelationDataItem();
            }
            else
            {
                var correlatedO18DoubleLabelMzValue = iso.Peaklist[indexO18DoubleLabelPeak].XValue;
                o18DoubleLabelChromCorrDataItem = GetChromCorrDataItem(run, startScan, stopScan, basePeakChromXYData, correlatedO18DoubleLabelMzValue);
            }

            correlationData.AddCorrelationData(o18SingleLabelChromCorrDataItem);
            correlationData.AddCorrelationData(o18DoubleLabelChromCorrDataItem);
            return correlationData;
        }


        private ChromCorrelationDataItem GetChromCorrDataItem(Run run, int startScan, int stopScan, XYData baseXYData, double correlatedMZValue)
        {
            bool chromDataIsOK;
            var chromPeakXYData = GetCorrelatedChromPeakXYData(run, startScan, stopScan, baseXYData, correlatedMZValue);

            chromDataIsOK = chromPeakXYData != null && chromPeakXYData.Xvalues != null &&
                         chromPeakXYData.Xvalues.Length > 3;

            ChromCorrelationDataItem dataItem;
            if (chromDataIsOK)
            {
                double slope;
                double intercept;
                double rsquaredVal;

                chromPeakXYData = FillInAnyMissingValuesInChromatogram(baseXYData, chromPeakXYData);

                GetElutionCorrelationData(baseXYData, chromPeakXYData, out slope, out intercept, out rsquaredVal);

                dataItem = new ChromCorrelationDataItem(slope, intercept, rsquaredVal);


            }
            else
            {

                dataItem = new ChromCorrelationDataItem();

            }

            return dataItem;
        }


    }
}
