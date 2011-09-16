using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Utilities;
using DeconTools.Backend.Algorithms;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class LabeledMultiPeakChromGeneratorTask : Task
    {
         TomTheorFeatureGenerator featureGenerator = new TomTheorFeatureGenerator();
         N15IsotopeProfileGenerator _N15IsotopicProfileGenerator = new N15IsotopeProfileGenerator();


        #region Constructors
        public LabeledMultiPeakChromGeneratorTask()
            : this(3, 25)
        {

        }

        public LabeledMultiPeakChromGeneratorTask(int numPeakForGeneratingChrom, double toleranceInPPM)
        {
            this.NumPeaksForGeneratingChrom = numPeakForGeneratingChrom;
            this.ToleranceInPPM = toleranceInPPM;

        }
        #endregion

        #region Properties
        public int NumPeaksForGeneratingChrom { get; set; }
        public double ToleranceInPPM { get; set; }
        #endregion

   

        #region Private Methods
        #endregion

        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl.Run.CurrentMassTag != null, String.Format("{0} failed. Mass tags haven't been defined.", this.Name));

            resultColl.ResultType = Globals.ResultType.N14N15_TARGETED_RESULT;

            featureGenerator.GenerateTheorFeature(resultColl.Run.CurrentMassTag);   //generate theor profile for unlabeled feature
            IsotopicProfile labeledProfile = _N15IsotopicProfileGenerator.GetN15IsotopicProfile(resultColl.Run.CurrentMassTag, 0.005);

            IsotopicProfileMultiChromatogramExtractor chromExtractor = new IsotopicProfileMultiChromatogramExtractor(
                NumPeaksForGeneratingChrom, ToleranceInPPM);

            TargetedResultBase massTagresult = resultColl.GetTargetedResult(resultColl.Run.CurrentMassTag);

            N14N15_TResult n14n15result;

            if (massTagresult is N14N15_TResult)
            {
                n14n15result = (N14N15_TResult)massTagresult;
            }
            else
            {
                throw new InvalidOperationException(String.Format("{0} failed. There was a problem with the Result type.", this.Name));
            }

            n14n15result.UnlabeledPeakChromData = chromExtractor.GetChromatogramsForIsotopicProfilePeaks(resultColl.MSPeakResultList, resultColl.Run.CurrentMassTag.IsotopicProfile);
            n14n15result.LabeledPeakChromData = chromExtractor.GetChromatogramsForIsotopicProfilePeaks(resultColl.MSPeakResultList, labeledProfile);



        }
    }
}
