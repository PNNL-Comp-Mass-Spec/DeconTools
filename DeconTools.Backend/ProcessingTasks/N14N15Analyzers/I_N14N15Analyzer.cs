using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.N14N15Analyzers
{
    public abstract class I_N14N15Analyzer:Task
    {

        public abstract void ExtractN14N15Values(ResultCollection resultList);

        public override void Execute(ResultCollection resultList)
        {
            throw new NotImplementedException();
        }
    }
}
