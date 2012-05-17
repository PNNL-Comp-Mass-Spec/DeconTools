
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
            XYData normalized = new XYData();

            normalized.Xvalues = xydata.Xvalues;
            normalized.Yvalues = xydata.Yvalues;

            double maxYValue = double.MinValue;




            for (int i = 0; i < normalized.Yvalues.Length; i++)
            {
                double currentVal = normalized.Yvalues[i];

                if (currentVal>maxYValue)
                {
                    maxYValue = currentVal;
                }
            }


            for (int i = 0; i < normalized.Yvalues.Length; i++)
            {
                normalized.Yvalues[i] = normalized.Yvalues[i]/maxYValue;
            }



            return normalized;


        }


        public static XYData SubtractXYData(XYData xydata1, XYData xydata2, double minX, double maxX, double tolerance)
        {

            int startIndex1 = MathUtils.GetClosest(xydata1.Xvalues, minX, tolerance);
            int stopIndex1 = MathUtils.GetClosest(xydata1.Xvalues, maxX, tolerance);

            int startIndex2 = MathUtils.GetClosest(xydata2.Xvalues, minX, tolerance);
            int stopIndex2 = MathUtils.GetClosest(xydata2.Xvalues, maxX, tolerance);


            XYData subtracted = new XYData();
            subtracted.Xvalues = xydata1.Xvalues;
            subtracted.Yvalues = xydata1.Yvalues;


            for (int i = startIndex1; i <= stopIndex1; i++)
            {
                double currentXVal = subtracted.Xvalues[i];

                int indexOfClosest = MathUtils.GetClosest(xydata2.Xvalues, currentXVal, tolerance);
                subtracted.Yvalues[i] = subtracted.Yvalues[i] - xydata2.Yvalues[indexOfClosest];


            }
            return subtracted;

        }



        #endregion

        #region Private Methods

        #endregion

    }
}
