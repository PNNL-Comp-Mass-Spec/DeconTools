using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18IqResult:IqResult
    {

        #region Constructors
        public O16O18IqResult(IqTarget target)
            : base(target)
        {

        }
        #endregion

        #region Properties


        public double RatioO16O18DoubleLabel { get; set; }

        public double RatioO16O18SingleLabel { get; set; }

        public double RatioSingleToDoubleLabel { get; set; }

        public double CorrelationO16O18SingleLabel { get; set; }

        public double CorrelationO16O18DoubleLabel { get; set; }

        public double CorrelationBetweenSingleAndDoubleLabel { get; set; }

        public double FitScoreO18Profile { get; set; }

        #endregion

        #region Public Methods

        //NOTE: this is duplicated code from the O16O18IterativeTff
        public IsotopicProfile ConvertO16ProfileToO18(IsotopicProfile theorFeature, int numPeaksToShift)
        {
            var o18Iso = new IsotopicProfile { ChargeState = theorFeature.ChargeState, Peaklist = new List<MSPeak>() };
            double mzBetweenIsotopes = 1.003 / theorFeature.ChargeState;

            foreach (var theorpeak in theorFeature.Peaklist)
            {
                var peak = new MSPeak(theorpeak.XValue, theorpeak.Height, theorpeak.Width, theorpeak.SignalToNoise);

                peak.XValue += numPeaksToShift * mzBetweenIsotopes;

                o18Iso.Peaklist.Add(peak);

            }

            return o18Iso;
        }


        #endregion

        #region Private Methods

        #endregion

        public double GetCorrelationO16O18SingleLabel()
        {
            double failedValue = -1;

            if (CorrelationData!=null && CorrelationData.CorrelationDataItems.Count>1)
            {
                var corr = CorrelationData.CorrelationDataItems[0].CorrelationRSquaredVal == null
                               ? failedValue
                               : (double) CorrelationData.CorrelationDataItems[0].CorrelationRSquaredVal;

                return corr;
            }

            return failedValue;
        }

        public double GetCorrelationO16O18DoubleLabel()
        {
            double failedValue = -1;

            if (CorrelationData != null && CorrelationData.CorrelationDataItems.Count > 1)
            {
                var corr = CorrelationData.CorrelationDataItems[1].CorrelationRSquaredVal == null
                               ? failedValue
                               : (double)CorrelationData.CorrelationDataItems[1].CorrelationRSquaredVal;

                return corr;
            }

            return failedValue;
        }

        public double GetCorrelationBetweenSingleAndDoubleLabel()
        {
            double failedValue = -1;

            if (CorrelationData != null && CorrelationData.CorrelationDataItems.Count > 2)
            {
                var corr = CorrelationData.CorrelationDataItems[2].CorrelationRSquaredVal == null
                               ? failedValue
                               : (double)CorrelationData.CorrelationDataItems[2].CorrelationRSquaredVal;

                return corr;
            }

            return failedValue;
        }

        public double GetRatioO16O18DoubleLabel()
        {
            double failedValue = -9999;

            if (CorrelationData != null && CorrelationData.CorrelationDataItems.Count > 1)
            {
                var corr = CorrelationData.CorrelationDataItems[1].CorrelationSlope == null
                               ? failedValue
                               : (double)CorrelationData.CorrelationDataItems[1].CorrelationSlope;

                return corr;
            }

            return failedValue;
        }


        public double GetRatioO16O18SingleLabel()
        {
            double failedValue = -9999;

            if (CorrelationData != null && CorrelationData.CorrelationDataItems.Count > 1)
            {
                var ratio = CorrelationData.CorrelationDataItems[0].CorrelationSlope == null
                               ? failedValue
                               : (double)CorrelationData.CorrelationDataItems[0].CorrelationSlope;

                return ratio;
            }

            return failedValue;
        }

        public double GetRatioSingleToDoubleLabel()
        {
            double failedValue = -9999;

            if (CorrelationData != null && CorrelationData.CorrelationDataItems.Count > 2)
            {
                var slope = CorrelationData.CorrelationDataItems[2].CorrelationSlope == null
                               ? failedValue
                               : (double)CorrelationData.CorrelationDataItems[2].CorrelationSlope;

                return slope;
            }

            return failedValue;
        }

    }
}
