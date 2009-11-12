using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public abstract class  I_TIC_Extractor:Task
    {
        public abstract void getTIC(ResultCollection resultList);

        
        public override void Execute(ResultCollection resultList)
        {
            getTIC(resultList);
        }
    }
}
