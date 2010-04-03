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
            : this(5)     // default toleranceInPPM
        {

        }

        public BasicTFeatureFinder(double toleranceInPPM)
            : this(toleranceInPPM, true)
        {
            this.ToleranceInPPM = toleranceInPPM;
        }

        public BasicTFeatureFinder(double toleranceInPPM, bool requiresMonoPeak)
        {
            this.NeedMonoIsotopicPeak = requiresMonoPeak;
        }

        #endregion






    }
}
