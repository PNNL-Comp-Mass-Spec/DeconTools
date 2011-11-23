using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ZeroFillers
{
    public abstract class ZeroFiller:Task
    {

        //TODO: fix so this method so it doesn't work on resultList
        public abstract void ZeroFill(ResultCollection resultList);


        public override void Execute(ResultCollection resultList)
        {
            ZeroFill(resultList);
        }
    }
}
