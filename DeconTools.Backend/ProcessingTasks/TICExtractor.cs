using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public abstract class  TICExtractor:Task
    {
        public abstract void GetTIC(ResultCollection resultList);

        
        public override void Execute(ResultCollection resultList)
        {
            GetTIC(resultList);
        }
    }
}
