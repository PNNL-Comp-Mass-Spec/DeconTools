using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public class MassTagFitScoreCalculator:Task
    {
     

        #region Public Methods
        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag is empty");
            Check.Require(resultColl.Run.CurrentMassTag.IsotopicProfile != null, this.Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");
            Check.Require(resultColl.Run.XYData != null && resultColl.Run.XYData.Xvalues != null && resultColl.Run.XYData.Xvalues.Length > 0, this.Name + " failed; Run's XY data is empty. Need to Run an MSGenerator");

            TargetedResultBase result = resultColl.CurrentTargetedResult;
            Check.Require(result != null, "No MassTagResult has been generated for CurrentMassTag");

            if (result.IsotopicProfile == null || result.IsotopicProfile.Peaklist == null || result.IsotopicProfile.Peaklist.Count == 0)
            {
                result.Score = 1;   // this is the worst possible fit score. ( 0.000 is the best possible fit score);  Maybe we want to return a '-1' to indicate a failure...              
                return;
            }

            IsotopicProfile theorProfile = resultColl.Run.CurrentMassTag.IsotopicProfile;
            int indexOfMostAbundantTheorPeak = theorProfile.GetIndexOfMostIntensePeak();
            int indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(result.IsotopicProfile.Peaklist,
                theorProfile.getMostIntensePeak().XValue, 0, result.IsotopicProfile.Peaklist.Count - 1, 0.1);


            if (result.IsotopicProfile.Peaklist.Count <= indexOfMostAbundantTheorPeak)      // most abundant peak isn't present in the actual theoretical profile... problem!
            {
                result.Score = 1;
                return;
            }


            double mzOffset = result.IsotopicProfile.Peaklist[indexOfCorrespondingObservedPeak].XValue - theorProfile.Peaklist[indexOfMostAbundantTheorPeak].XValue;

            double fwhm = result.IsotopicProfile.GetFWHM();

            XYData theorXYData = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(theorProfile, result.IsotopicProfile.GetFWHM());
            //theorXYData.Display();

            theorXYData.OffSetXValues(mzOffset);     //May want to avoid this offset if the masses have been aligned using LCMS Warp

            //theorXYData.Display();

            AreaFitter areafitter = new AreaFitter(theorXYData, resultColl.Run.XYData, 0.1);
            double fitval = areafitter.getFit();

            if (fitval == double.NaN || fitval > 1) fitval = 1;
            result.Score = fitval;







        }
        #endregion

        
    }
}
