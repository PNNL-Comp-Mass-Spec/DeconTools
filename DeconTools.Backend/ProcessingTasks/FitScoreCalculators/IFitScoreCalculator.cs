using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public abstract class IFitScoreCalculator : Task
    {
        public abstract void GetFitScores(IEnumerable<IsosResult> isosResults);

        public override void Execute(ResultCollection resultColl)
        {
            GetFitScores(resultColl.IsosResultBin);
        }
    }
}
