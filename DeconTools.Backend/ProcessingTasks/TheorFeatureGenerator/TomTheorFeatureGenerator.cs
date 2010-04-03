using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator
{
    public class TomTheorFeatureGenerator:ITheorFeatureGenerator
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public override void GenerateTheorFeature(ResultCollection results)
        {
            Check.Require(results.Run.CurrentMassTag != null, "FeatureGenerator failed. MassTag not defined.");

            
            MassTag mt = results.Run.CurrentMassTag;
            Check.Require(mt.GetEmpiricalFormulaAsIntArray() != null, "Theoretical feature generator failed. Can't retrieve empirical formula from Mass Tag");

            try
            {
                mt.IsotopicProfile= TomIsotopicPattern.GetIsotopePattern(mt.GetEmpiricalFormulaAsIntArray(), TomIsotopicPattern.aafIsos);
            }
            catch (Exception ex)
            {
                throw new Exception("Theoretical feature generator failed. Details: " + ex.Message);
            }
            PeakUtilities.TrimIsotopicProfile(mt.IsotopicProfile, 0.01);
            if (mt.ChargeState != 0) mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

        }

     
        #endregion

        #region Private Methods
        #endregion
    
    }
}
