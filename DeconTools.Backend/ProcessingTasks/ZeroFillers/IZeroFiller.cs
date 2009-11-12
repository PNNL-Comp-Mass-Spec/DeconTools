using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ZeroFillers
{
    public abstract class IZeroFiller:Task
    {

        public abstract void ZeroFill(ResultCollection resultList);


        public override void Execute(ResultCollection resultList)
        {
            ZeroFill(resultList);
        }
    }
}
