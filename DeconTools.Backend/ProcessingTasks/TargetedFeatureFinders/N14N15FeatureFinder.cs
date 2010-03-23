using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Algorithms;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class N14N15FeatureFinder : ITargetedFeatureFinder
    {
        #region Constructors
        public N14N15FeatureFinder()
            : this(0.02)
        {

        }

        /// <summary>
        /// Finds both N14 and N15 features using a m/z tolerance (default 0.02)
        /// </summary>
        /// <param name="toleranceInMZ">Tolerance in m/z</param>
        public N14N15FeatureFinder(double toleranceInMZ)
        {
            this.Tolerance = toleranceInMZ;

        }

        #endregion

        #region Properties

        public IsotopicProfile LabeledTheorProfile { get; set; }
        #endregion

        #region Public Methods
        public override void FindFeature(DeconTools.Backend.Core.ResultCollection resultColl)
        {
            resultColl.MassTagResultType = Globals.MassTagResultType.N14N15_MASSTAG_RESULT;

            base.FindFeature(resultColl);

            MassTagResultBase massTagresult = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);

            
            LabeledTheorProfile = N15IsotopeProfileGenerator.GetN15IsotopicProfile(resultColl.Run.CurrentMassTag, 0.005);

            BasicMSFeatureFinder bff = new BasicMSFeatureFinder();
            IsotopicProfile labelledIso = bff.FindMSFeature(resultColl.Run.PeakList, LabeledTheorProfile, Tolerance, false);

            massTagresult.AddLabelledIso(labelledIso);

        }

     

        private void getN15FormulaAsIntArray(MassTag massTag, double p)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
