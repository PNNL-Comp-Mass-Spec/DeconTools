using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public abstract class ITargetedFeatureFinder:Task
    {
        #region Properties
        #endregion

        #region Public Methods
        public abstract void FindFeature(ResultCollection resultList);

        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultList)
        {
            // generate theor feature 
            
            FindFeature(resultList);
        }
    }
}
