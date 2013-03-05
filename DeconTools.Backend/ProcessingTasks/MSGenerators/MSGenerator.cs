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
            if (xydata == null || xydata.Xvalues == null || xydata.Yvalues == null) return -1;

            double summedIntensities = 0;
            for (int i = 0; i < xydata.Yvalues.Length; i++)
            {
                if (xydata.Xvalues[i] > minMZ && xydata.Xvalues[i] < maxMZ)
                {
                    summedIntensities += xydata.Yvalues[i];
                }
            }

            return (float)summedIntensities;
        }

    }
}
