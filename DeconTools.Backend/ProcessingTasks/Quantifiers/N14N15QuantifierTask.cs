using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Algorithms.Quantifiers;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;

namespace DeconTools.Backend.ProcessingTasks.Quantifiers
{
    public class N14N15QuantifierTask : Task
    {
        #region Constructors
        public N14N15QuantifierTask()
            : this(3)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numPeaksUsedInRatioCalc">Number of peaks used in the ratio calculation</param>
        public N14N15QuantifierTask(int numPeaksUsedInRatioCalc)
        {
            this.NumPeaksUsedInRatioCalc = numPeaksUsedInRatioCalc;

        }

        #endregion

        #region Properties
        public int NumPeaksUsedInRatioCalc { get; set; }
        #endregion

        #region Public Methods


        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultColl)
        {
            MassTag mt = resultColl.Run.CurrentMassTag;
            Check.Require(mt != null, "Current mass tag is not defined.");

            MassTagResultBase currentResult = resultColl.GetMassTagResult(mt);

            Check.Require(currentResult != null, "Quantifier failed. Result doesn't exist for current mass tag.");
            Check.Require(currentResult is N14N15_TResult, "Quantifier failed. Result is not of the N14N15 type.");

            N14N15_TResult n14n15Result = ((N14N15_TResult)currentResult);

            BasicN14N15Quantifier quant = new BasicN14N15Quantifier();

            IsotopicProfile iso1 = ((N14N15_TResult)currentResult).IsotopicProfile;
            IsotopicProfile iso2 = ((N14N15_TResult)currentResult).IsotopicProfileLabeled;

            if (iso1 == null || iso2 == null)
            {
                n14n15Result.RatioN14N15 = -1;
            }
            else
            {
                double ratioContribIso1;
                double ratioContribIso2;
                double ratio;


                quant.GetRatioBasedOnTopPeaks(iso1, iso2,mt.IsotopicProfile, N15IsotopeProfileGenerator.GetN15IsotopicProfile(mt, 0.005),
                    currentResult.ScanSet.BackgroundIntensity, NumPeaksUsedInRatioCalc,
                    out ratio, out ratioContribIso1, out ratioContribIso2);

                n14n15Result.RatioN14N15 = ratio;
                n14n15Result.RatioContributionN14 = ratioContribIso1;
                n14n15Result.RatioContributionN15 = ratioContribIso2;

            }

            //((N14N15_TResult)currentResult).RatioN14N15 = quant.GetRatioBasedOnAreaUnderPeaks(resultColl.Run.XYData.Xvalues, resultColl.Run.XYData.Yvalues, iso1, iso2, currentResult.ScanSet.BackgroundIntensity);


        }

        private object IsotopicProfile(N14N15_TResult n14N15_TResult)
        {
            throw new NotImplementedException();
        }
    }
}
