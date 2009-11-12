using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.Smoothers
{
    public abstract class ISmoother:Task
    {

        public abstract void Smooth(ResultCollection resultList);

        public override void Execute(ResultCollection resultList)
        {
            Smooth(resultList);
        }
    }
}
