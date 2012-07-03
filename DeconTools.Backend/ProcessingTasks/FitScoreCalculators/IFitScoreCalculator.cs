using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public abstract class IFitScoreCalculator : Task
    {
        public abstract void GetFitScores(IEnumerable<IsosResult> isosResults);

        public override void Execute(ResultCollection resultList)
        {
            GetFitScores(resultList.IsosResultBin);
        }
    }
}
