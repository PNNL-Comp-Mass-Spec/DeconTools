using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

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
            trimIsotopicProfile(mt.IsotopicProfile, 0.01);
            if (mt.ChargeState != 0) mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

        }

        private void trimIsotopicProfile(IsotopicProfile isotopicProfile, double cutOff)
        {
            if (isotopicProfile == null || isotopicProfile.Peaklist == null || isotopicProfile.Peaklist.Count == 0) return;

            int indexOfMaxPeak = isotopicProfile.getIndexOfMostIntensePeak();
            List<MSPeak> trimmedPeakList = new List<MSPeak>();

            bool foundMaxPeak = false;
            for (int i = 0; i < isotopicProfile.Peaklist.Count; i++)
            {
                if (!foundMaxPeak)
                {
                    trimmedPeakList.Add(isotopicProfile.Peaklist[i]);
                }
                else
                {
                    if (isotopicProfile.Peaklist[i].Height > cutOff)
                    {
                        trimmedPeakList.Add(isotopicProfile.Peaklist[i]);
                    }
                    else
                    {
                        break;    // at this point, have found max peak and the right-most peaks have fallen below the cutoff. So exit. 
                    }
                }
                if (indexOfMaxPeak == i) foundMaxPeak = true;
            }
            isotopicProfile.Peaklist = trimmedPeakList;

        }
        #endregion

        #region Private Methods
        #endregion
    
    }
}
