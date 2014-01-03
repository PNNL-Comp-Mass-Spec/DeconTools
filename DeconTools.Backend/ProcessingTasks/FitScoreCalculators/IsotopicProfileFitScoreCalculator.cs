using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public class IsotopicProfileFitScoreCalculator:Task
    {
        #region properties
        /// <summary>
        /// score experimental profile to labeled or unlabeled theoretical profile
        /// </summary>
        public DeconTools.Backend.Globals.IsotopicProfileType IsotopicProfileType { get; set; }

        public IsotopicProfileFitScoreCalculator()
        {
            IsotopicProfileType = DeconTools.Backend.Globals.IsotopicProfileType.UNLABELLED;
        }

        public IsotopicProfileFitScoreCalculator(DeconTools.Backend.Globals.IsotopicProfileType lableType)
        {
            IsotopicProfileType = lableType;
        }
        #endregion

        #region Public Methods
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag is empty");
            Check.Require(resultList.Run.XYData != null && resultList.Run.XYData.Xvalues != null && resultList.Run.XYData.Xvalues.Length > 0, this.Name + " failed; Run's XY data is empty. Need to Run an MSGenerator");
            Check.Require(resultList.CurrentTargetedResult != null, "No MassTagResult has been generated for CurrentMassTag");

            IsotopicProfile theorProfile = new IsotopicProfile();
            switch (IsotopicProfileType)
            {
                case DeconTools.Backend.Globals.IsotopicProfileType.UNLABELLED:
                    Check.Require(resultList.Run.CurrentMassTag.IsotopicProfile != null, "Target's theoretical isotopic profile has not been established");
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfile;
                    break;
                case DeconTools.Backend.Globals.IsotopicProfileType.LABELLED:
                    //Check.Require(resultList.Run.CurrentMassTag.IsotopicProfileLabelled != null, this.Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");
                    Check.Require(resultList.Run.CurrentMassTag.IsotopicProfileLabelled != null, "Target's labelled theoretical isotopic profile has not been established");
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfileLabelled;
                    break;
                default:
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfile;
                    break;
            }
            
           resultList.CurrentTargetedResult.Score = CalculateFitScore(theorProfile, resultList.CurrentTargetedResult.IsotopicProfile, resultList.Run.XYData);
        }
        #endregion


        public double CalculateFitScore(IsotopicProfile theorProfile, IsotopicProfile observedProfile, XYData massSpecXYData )
        {
            if (observedProfile == null || observedProfile.Peaklist == null || observedProfile.Peaklist.Count == 0)
            {
                return 1.0;   // this is the worst possible fit score. ( 0.000 is the best possible fit score);  Maybe we want to return a '-1' to indicate a failure...              
                
            }

            int indexOfMostAbundantTheorPeak = theorProfile.GetIndexOfMostIntensePeak();
            int indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(observedProfile.Peaklist,
                theorProfile.getMostIntensePeak().XValue, 0, observedProfile.Peaklist.Count - 1, 0.1);


            if (indexOfCorrespondingObservedPeak < 0)      // most abundant peak isn't present in the actual theoretical profile... problem!
            {
                return 1.0;
            }

            double mzOffset = observedProfile.Peaklist[indexOfCorrespondingObservedPeak].XValue - theorProfile.Peaklist[indexOfMostAbundantTheorPeak].XValue;

            XYData theorXYData = theorProfile.GetTheoreticalIsotopicProfileXYData(observedProfile.GetFWHM());
            //theorXYData.Display();

            theorXYData.OffSetXValues(mzOffset);     //May want to avoid this offset if the masses have been aligned using LCMS Warp

            //theorXYData.Display();

            var areafitter = new AreaFitter();
			int ionCountUsed;

			double fitval = areafitter.GetFit(theorXYData, massSpecXYData, 0.1, out ionCountUsed);

            if (double.IsNaN(fitval) || fitval > 1) fitval = 1;
            return fitval;
        }

    }
}
