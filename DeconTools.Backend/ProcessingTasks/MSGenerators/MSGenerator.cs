using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.MSGenerators
{
    public abstract class MSGenerator : Task
    {
        public bool IsTICRequested { get; set; }

        public virtual double MinMZ { get; set; }

        public virtual double MaxMZ { get; set; }

        public abstract XYData GenerateMS(Run run, ScanSet lcScanSet, ScanSet imsScanset=null);

        //protected abstract void createNewScanResult(ResultCollection resultList, ScanSet scanSet);

        public override void Execute(ResultCollection resultList)
        {
            resultList.Run.XYData = GenerateMS(resultList.Run, resultList.Run.CurrentScanSet);
        }

        public float GetTIC(XYData xydata, double minMZ, double maxMZ)
        {
			if (xydata == null) return -1;

        	double[] xValues = xydata.Xvalues;
        	double[] yValues = xydata.Yvalues;

			if (xValues == null || yValues == null) return -1;

            double summedIntensities = 0;
			for (int i = 0; i < yValues.Length; i++)
			{
				double xValue = xValues[i];

				if (xValue > minMZ && xValue < maxMZ)
                {
					summedIntensities += yValues[i];
                }
            }

            return (float)summedIntensities;
        }
    }
}
