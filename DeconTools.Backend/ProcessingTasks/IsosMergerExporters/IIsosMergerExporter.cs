using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected string DblToString(double value, byte digitsOfPrecision)
        {
            return PNNLOmics.Utilities.StringUtilities.DblToString(value, digitsOfPrecision);
        }
    }
}
