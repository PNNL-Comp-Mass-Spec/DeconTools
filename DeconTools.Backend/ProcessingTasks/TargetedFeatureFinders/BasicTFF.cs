using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class BasicTFF : TFFBase
    {
        #region Constructors
        public BasicTFF()
            : this(5)     // default toleranceInPPM
        {

        }

        public BasicTFF(double toleranceInPPM)
            : this(toleranceInPPM,true)
        {

        }

        public BasicTFF(double toleranceInPPM, bool requiresMonoPeak)
            : this(toleranceInPPM, requiresMonoPeak, IsotopicProfileType.UNLABELLED)
        {
        }

        public BasicTFF(double toleranceInPPM, bool requiresMonoPeak, IsotopicProfileType isotopicProfileTarget)
        {
            this.ToleranceInPPM = toleranceInPPM;
            this.NeedMonoIsotopicPeak = requiresMonoPeak;
            this.IsotopicProfileType = isotopicProfileTarget;

        }

        #endregion






    }
}
