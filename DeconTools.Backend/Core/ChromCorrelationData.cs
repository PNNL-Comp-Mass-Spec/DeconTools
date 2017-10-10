using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Core
{
    public class ChromCorrelationData
    {

        #region Constructors
        public ChromCorrelationData()
        {
            CorrelationDataItems = new List<ChromCorrelationDataItem>();
        }
        #endregion



        #region Properties

        public List<ChromCorrelationDataItem> CorrelationDataItems { get; set; }


        public double? RSquaredValsMedian
        {
            get
            {
                var validItems = CorrelationDataItems.Select(p => p.CorrelationRSquaredVal).Where(n => n.HasValue).ToList();

                if (validItems.Any())
                {
                    return MathUtils.GetMedian(validItems.Select(r=>r.GetValueOrDefault()).ToList());
                }
                return null;
            }
        }

        public double? RSquaredValsAverage
        {
            get
            {
                var validItems = CorrelationDataItems.Select(p => p.CorrelationRSquaredVal).Where(n => n.HasValue).ToList();

                if (validItems.Any())
                {
                    return validItems.Average();
                }
                return null;
            }
        }

        public double? RSquaredValsStDev
        {
            get
            {
                var validItems = CorrelationDataItems.Select(p => p.CorrelationRSquaredVal).Where(n => n.HasValue).ToList();

                if (validItems.Count > 2)
                {
                    return MathUtils.GetStDev(validItems.Select(p=>p.GetValueOrDefault()).ToList());
                }
                return null;
            }
        }

        #endregion

        #region Public Methods

        public void AddCorrelationData(double correlationSlope, double correlationIntercept, double correlationRSquaredVal)
        {
            var data = new ChromCorrelationDataItem(correlationSlope,correlationIntercept,correlationRSquaredVal);
            CorrelationDataItems.Add(data);
        }

        public void AddCorrelationData(ChromCorrelationDataItem chromCorrelationDataItem)
        {
            CorrelationDataItems.Add(chromCorrelationDataItem);
        }


        public string ToStringWithDetails()
        {
            var data = new StringBuilder();
            var validItems = CorrelationDataItems.Select(p => p.CorrelationRSquaredVal).Where(n => n.HasValue);
            foreach (var validItem in validItems)
            {
                data.Append(validItem + " ");
            }
            return data.ToString();
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
