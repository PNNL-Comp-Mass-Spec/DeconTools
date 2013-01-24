using System.Linq;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class ChromatogramCorrelatorO16O18 : ChromatogramCorrelatorBase
    {
        public ChromatogramCorrelatorO16O18(int numPointsInSmoother, int chromToleranceInPPM = 20, double minRelativeIntensityForChromCorr = 0.00001)
            : base(numPointsInSmoother, chromToleranceInPPM, minRelativeIntensityForChromCorr)
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
            int indexO16MonoPeak = 0;
            int indexO18SingleLabelPeak = 2;
            int indexO18DoubleLabelPeak = 4;

            double baseMZValue = iso.Peaklist[indexO16MonoPeak].XValue;
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
                double correlatedO18SingleLabelMZValue = iso.Peaklist[indexO18SingleLabelPeak].XValue;
                o18SingleLabelChromCorrDataItem = GetChromCorrDataItem(run, startScan, stopScan, basePeakChromXYData, correlatedO18SingleLabelMZValue);
            }

            ChromCorrelationDataItem o18DoubleLabelChromCorrDataItem;
            if (indexO18DoubleLabelPeak >= iso.Peaklist.Count)
            {
                o18DoubleLabelChromCorrDataItem = new ChromCorrelationDataItem();
            }
            else
            {
                double correlatedO18DoubleLabelMZValue = iso.Peaklist[indexO18DoubleLabelPeak].XValue;
                o18DoubleLabelChromCorrDataItem = GetChromCorrDataItem(run, startScan, stopScan, basePeakChromXYData, correlatedO18DoubleLabelMZValue);
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
