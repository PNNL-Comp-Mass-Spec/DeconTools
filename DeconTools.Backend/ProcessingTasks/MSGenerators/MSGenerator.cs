using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.MSGenerators
{
    public abstract class MSGenerator : Task
    {
        public bool IsTICRequested { get; set; }

        public virtual double MinMZ { get; set; }

        public virtual double MaxMZ { get; set; }

        public abstract XYData GenerateMS(Run run, ScanSet lcScanSet, ScanSet imsScanSet = null);

        //protected abstract void createNewScanResult(ResultCollection resultList, ScanSet scanSet);

        public override void Execute(ResultCollection resultList)
        {
            resultList.Run.XYData = GenerateMS(resultList.Run, resultList.Run.CurrentScanSet);
        }

        public float GetTIC(XYData xyData, double minMZ, double maxMZ)
        {
            if (xyData == null)
            {
                return -1;
            }

            var xValues = xyData.Xvalues;
            var yValues = xyData.Yvalues;

            if (xValues == null || yValues == null)
            {
                return -1;
            }

            double summedIntensities = 0;
            for (var i = 0; i < yValues.Length; i++)
            {
                var xValue = xValues[i];

                if (xValue > minMZ && xValue < maxMZ)
                {
                    summedIntensities += yValues[i];
                }
            }

            return (float)summedIntensities;
        }
    }
}
