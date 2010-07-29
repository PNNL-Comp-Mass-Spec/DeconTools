using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.Smoothers
{
    public abstract class ISmoother:Task
    {

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.XYData != null, "Smoother not executed; no data in XYData object");
            resultList.Run.XYData = Smooth(resultList.Run.XYData);
        }

        public abstract XYData Smooth(XYData xYData);


     
    }
}
