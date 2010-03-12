using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class BasicTFeatureFinder : ITargetedFeatureFinder
    {
        #region Constructors
        public BasicTFeatureFinder()
            : this(0.005)     // default mzTolerance
        {

        }

        public BasicTFeatureFinder(double mzTolerance)
        {
            this.Tolerance = mzTolerance;
        }

        public BasicTFeatureFinder(double mzTolerance, bool requiresMonoPeak)
        {
            this.NeedMonoIsotopicPeak = requiresMonoPeak;

        }

        #endregion
          

  
    

     
    }
}
