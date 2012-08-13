
namespace DeconTools.Backend.Core
{
    public class ChromCorrelationDataItem
    {

        #region Constructors
        public ChromCorrelationDataItem()
        {
            CorrelationIntercept = new double?();
            CorrelationSlope = new double?();
            CorrelationRSquaredVal = new double?();
        }

        public ChromCorrelationDataItem(double correlationSlope, double correlationIntercept, double correlationRSquaredVal)
        {
            CorrelationIntercept = correlationIntercept;
            CorrelationRSquaredVal = correlationRSquaredVal;
            CorrelationSlope = correlationSlope;
        }


        #endregion

        #region Properties

        public double? CorrelationSlope { get; set; }
        public double? CorrelationIntercept { get; set; }
        public double? CorrelationRSquaredVal { get; set; }  

        #endregion


        public override string ToString()
        {
            return CorrelationSlope.ToString() + "; "+ CorrelationIntercept.ToString() + "; "+
                   CorrelationRSquaredVal.ToString();
        }

    }
}
