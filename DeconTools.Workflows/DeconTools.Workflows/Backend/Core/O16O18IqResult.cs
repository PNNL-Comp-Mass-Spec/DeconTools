using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


        public double RatioO16O18 { get; set; }

        public double CorrelationO16O18SingleLabel { get; set; }

        public double CorrelationO16O18DoubleLabel { get; set; }



        #endregion

        #region Public Methods

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



        public double GetRatioO16O18()
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

    }
}
