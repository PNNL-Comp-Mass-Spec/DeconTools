using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.Smoothers
{
    public abstract class ISmoother:Task
    {

        public override void Execute(ResultCollection resultList)
        {
            resultList.Run.XYData = Smooth(resultList.Run.XYData);
        }

        public abstract XYData Smooth(XYData xYData);


     
    }
}
