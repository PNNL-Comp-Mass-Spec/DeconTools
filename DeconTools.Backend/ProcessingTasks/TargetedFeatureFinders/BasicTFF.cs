
namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public sealed class BasicTFF : TFFBase
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
            : this(toleranceInPPM, requiresMonoPeak, Globals.IsotopicProfileType.UNLABELLED)
        {
        }

        public BasicTFF(double toleranceInPPM, bool requiresMonoPeak, Globals.IsotopicProfileType isotopicProfileTarget)
        {
            ToleranceInPPM = toleranceInPPM;
            NeedMonoIsotopicPeak = requiresMonoPeak;
            IsotopicProfileType = isotopicProfileTarget;
            NumPeaksUsedInAbundance = 1;

        }

        #endregion

    }
}
