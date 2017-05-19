
namespace DeconTools.Backend.Utilities
{
    public class XYDataUtilities
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public static XYData NormalizeXYData(XYData xydata)
        {
            var normalized = new XYData();

            normalized.Xvalues = xydata.Xvalues;
            normalized.Yvalues = xydata.Yvalues;

            var maxYValue = double.MinValue;




            for (var i = 0; i < normalized.Yvalues.Length; i++)
            {
                var currentVal = normalized.Yvalues[i];

                if (currentVal>maxYValue)
                {
                    maxYValue = currentVal;
                }
            }


            for (var i = 0; i < normalized.Yvalues.Length; i++)
            {
                normalized.Yvalues[i] = normalized.Yvalues[i]/maxYValue;
            }



            return normalized;


        }


        public static XYData SubtractXYData(XYData xydata1, XYData xydata2, double minX, double maxX, double tolerance)
        {

            var startIndex1 = MathUtils.GetClosest(xydata1.Xvalues, minX, tolerance);
            var stopIndex1 = MathUtils.GetClosest(xydata1.Xvalues, maxX, tolerance);

            var startIndex2 = MathUtils.GetClosest(xydata2.Xvalues, minX, tolerance);
            var stopIndex2 = MathUtils.GetClosest(xydata2.Xvalues, maxX, tolerance);


            var subtracted = new XYData();
            subtracted.Xvalues = xydata1.Xvalues;
            subtracted.Yvalues = xydata1.Yvalues;


            for (var i = startIndex1; i <= stopIndex1; i++)
            {
                var currentXVal = subtracted.Xvalues[i];

                var indexOfClosest = MathUtils.GetClosest(xydata2.Xvalues, currentXVal, tolerance);
                subtracted.Yvalues[i] = subtracted.Yvalues[i] - xydata2.Yvalues[indexOfClosest];


            }
            return subtracted;

        }



        #endregion

        #region Private Methods

        #endregion

    }
}
