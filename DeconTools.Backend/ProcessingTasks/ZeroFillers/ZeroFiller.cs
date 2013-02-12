using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ZeroFillers
{
    public abstract class ZeroFiller : Task
    {
        protected ZeroFiller()
        {
            MaxNumPointsToAdd = 3;
        }

        protected ZeroFiller(int maxNumPointsToAdd)
        {
            MaxNumPointsToAdd = maxNumPointsToAdd;
        }

        public int MaxNumPointsToAdd { get; set; }

        public abstract XYData ZeroFill(double[] xvalues, double[] yvalues, int maxZerosToAdd);

        public override void Execute(ResultCollection resultList)
        {
            if (resultList.Run.XYData!=null && resultList.Run.XYData.Xvalues.Length>0)
            {
                resultList.Run.XYData = ZeroFill(resultList.Run.XYData.Xvalues, resultList.Run.XYData.Yvalues, MaxNumPointsToAdd);    
            }

            
        }
    }
}
