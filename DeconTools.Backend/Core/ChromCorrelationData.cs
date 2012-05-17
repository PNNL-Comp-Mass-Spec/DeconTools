using System.Collections.Generic;
using System.Linq;
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

        public double RSquaredValsMedian
        {
            get
            {
                if (CorrelationDataItems.Count>0)
                {
                    return MathUtils.GetMedian(CorrelationDataItems.Select(p => p.CorrelationRSquaredVal).ToList());  
                }
                return -1;
            }
        }
        
        public double RSquaredValsAverage
        {
            get
            {
                if (CorrelationDataItems.Count>0)
                {
                    return CorrelationDataItems.Select(p => p.CorrelationRSquaredVal).Average();  
                }
                return -1;
            }
        }

        public double RSquaredValsStDev
        {
            get
            {
                if (CorrelationDataItems.Count>2)
                {
                    return MathUtils.GetStDev(CorrelationDataItems.Select(p => p.CorrelationRSquaredVal).ToList()); 
                }
                return -1;
            }
        }

        #endregion

        #region Public Methods
        
        public void AddCorrelationData(double correlationSlope, double correlationIntercept, double correlationRSquaredVal)
        {
            ChromCorrelationDataItem data = new ChromCorrelationDataItem(correlationSlope,correlationIntercept,correlationRSquaredVal);
            CorrelationDataItems.Add(data);
        }

        public void AddCorrelationData(ChromCorrelationDataItem chromCorrelationDataItem)
        {
            CorrelationDataItems.Add(chromCorrelationDataItem);
        }

        

        #endregion

        #region Private Methods

        #endregion

    }
}
