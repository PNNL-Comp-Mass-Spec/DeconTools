using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public abstract class IIsosMergerExporter : Task
    {
        public abstract void MergeAndExport(ResultCollection resultList);

        public override void Execute(ResultCollection resultList)
        {
            MergeAndExport(resultList);
        }
    }
}
