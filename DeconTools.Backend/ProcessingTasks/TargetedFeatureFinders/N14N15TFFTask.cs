using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public sealed class N14N15TFFTask : TFFBase
    {
        #region Constructors
        public N14N15TFFTask()
            : this(0.02)
        {

        }

        /// <summary>
        /// Finds both N14 and N15 features using a PPM tolerance (default 0.02)
        /// </summary>
        /// <param name="toleranceInPPM">Tolerance in PPM</param>
        public N14N15TFFTask(double toleranceInPPM)
        {
            ToleranceInPPM = toleranceInPPM;
            Name = "N14N15FeatureFinder";

        }

        #endregion

        #region Properties

        public IsotopicProfile LabeledTheorProfile { get; set; }
        #endregion

        #region Public Methods
        //public override void FindFeature(DeconTools.Backend.Core.ResultCollection resultColl)
        //{
        //    Check.Require(resultColl.Run.CurrentMassTag != null, string.Format("{0} failed. MassTag has not been defined.", this.Name));
        //    Check.Require(resultColl.Run.CurrentMassTag.IsotopicProfileLabelled != null, string.Format("{0} failed. Labelled Theoretical profile not defined. Make sure to run a TheoreticalIsotopicProfile generator", this.Name));

        //    MassTagResultBase massTagresult = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);

        //    BasicMSFeatureFinder bff = new BasicMSFeatureFinder();

        //    IsotopicProfile labelledIso = bff.FindMSFeature(resultColl.Run.PeakList, resultColl.Run.CurrentMassTag.IsotopicProfileLabelled, ToleranceInPPM, false);

        //    if (labelledIso == null)
        //    {
        //        massTagresult.Flags.Add(new LabeledProfileMissingResultFlag());
        //    }

        //    massTagresult.AddLabelledIso(labelledIso);

        //}

        #endregion


    }
}
