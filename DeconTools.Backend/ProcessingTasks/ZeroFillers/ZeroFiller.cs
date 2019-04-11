using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ZeroFillers
{
    public abstract class ZeroFiller : Task
    {
        public const int DEFAULT_POINTS_TO_ADD = 3;
        public const double DEFAULT_MAX_ZERO_FILL_DISTANCE = 0.05;

        protected ZeroFiller()
            : this(DEFAULT_POINTS_TO_ADD, DEFAULT_MAX_ZERO_FILL_DISTANCE)
        {
        }

        protected ZeroFiller(int maxNumPointsToAdd)
            : this(maxNumPointsToAdd, DEFAULT_MAX_ZERO_FILL_DISTANCE)
        {
        }

        protected ZeroFiller(int maxNumPointsToAdd, double maxZeroFillDistance)
        {
            MaxNumPointsToAdd = maxNumPointsToAdd;
            MaxZeroFillDistance = maxZeroFillDistance;
        }

        public int MaxNumPointsToAdd { get; set; }

        /// <summary>
        /// Maximum distance (in Daltons) between each real data point and the adjacent zero'd values added to the side of the primary data point
        /// </summary>
        public double MaxZeroFillDistance { get; set; }

        public abstract XYData ZeroFill(double[] xValues, double[] yValues, int maxZerosToAdd);

        public abstract XYData ZeroFill(double[] xValues, double[] yValues, int maxZerosToAdd, double maxZeroFillDistance);

        public override void Execute(ResultCollection resultList)
        {
            if (resultList.Run.XYData != null && resultList.Run.XYData.Xvalues.Length > 0)
            {
                resultList.Run.XYData = ZeroFill(resultList.Run.XYData.Xvalues, resultList.Run.XYData.Yvalues, MaxNumPointsToAdd, MaxZeroFillDistance);
            }


        }
    }
}
