
namespace DeconTools.Backend.Core
{
    public class ChromCorrelationDataItem
    {

        #region Constructors
        public ChromCorrelationDataItem()
        {
            CorrelationIntercept = -9999;
            CorrelationSlope = -9999;
            CorrelationRSquaredVal = -1;
        }

        public ChromCorrelationDataItem(double correlationSlope, double correlationIntercept, double correlationRSquaredVal)
        {
            CorrelationIntercept = correlationIntercept;
            CorrelationRSquaredVal = correlationRSquaredVal;
            CorrelationSlope = correlationSlope;
        }


        #endregion

        #region Properties

        public double CorrelationSlope { get; set; }
        public double CorrelationIntercept { get; set; }
        public double CorrelationRSquaredVal { get; set; }  

        #endregion


        public override string ToString()
        {
            return CorrelationSlope.ToString("0.0000") + "; "+ CorrelationIntercept.ToString("0.0") + "; "+
                   CorrelationRSquaredVal.ToString("0.000");
        }

    }
}
