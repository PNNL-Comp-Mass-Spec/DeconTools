
namespace DeconTools.Backend.Utilities
{
    public class XYDataUtilities
    {
        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public static XYData NormalizeXYData(XYData xyData)
        {
            var normalized = new XYData
            {
                Xvalues = xyData.Xvalues,
                Yvalues = xyData.Yvalues
            };

            var maxYValue = double.MinValue;

            foreach (var currentVal in normalized.Yvalues)
            {
                if (currentVal > maxYValue)
                {
                    maxYValue = currentVal;
                }
            }

            for (var i = 0; i < normalized.Yvalues.Length; i++)
            {
                normalized.Yvalues[i] = normalized.Yvalues[i] / maxYValue;
            }

            return normalized;
        }

        public static XYData SubtractXYData(XYData xyData1, XYData xyData2, double minX, double maxX, double tolerance)
        {
            var startIndex1 = MathUtils.GetClosest(xyData1.Xvalues, minX, tolerance);
            var stopIndex1 = MathUtils.GetClosest(xyData1.Xvalues, maxX, tolerance);

            var startIndex2 = MathUtils.GetClosest(xyData2.Xvalues, minX, tolerance);
            var stopIndex2 = MathUtils.GetClosest(xyData2.Xvalues, maxX, tolerance);

            var subtracted = new XYData
            {
                Xvalues = xyData1.Xvalues,
                Yvalues = xyData1.Yvalues
            };

            for (var i = startIndex1; i <= stopIndex1; i++)
            {
                var currentXVal = subtracted.Xvalues[i];

                var indexOfClosest = MathUtils.GetClosest(xyData2.Xvalues, currentXVal, tolerance);
                subtracted.Yvalues[i] = subtracted.Yvalues[i] - xyData2.Yvalues[indexOfClosest];
            }
            return subtracted;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
