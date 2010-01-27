using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;

namespace DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator
{
    public class MercuryFeatureGenerator : ITheorFeatureGenerator
    {

        MercuryDistributionCreator distributionCreator;

        #region Constructors
        public MercuryFeatureGenerator()
        {
            this.Name = "MercuryFeatureGenerator";
        }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void GenerateTheorFeature(DeconTools.Backend.Core.ResultCollection results)
        {
            Check.Require(results.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag hasn't been declared");
            //Check.Require(results.Run.CurrentMassTag.EmpiricalFormula!=null, this.Name + "failed; Problem with EmpiricalFormular of current mass tag.");

            MassTag mt = results.Run.CurrentMassTag;

            IMassTagResult result = results.GetMassTagResult(mt);

            Check.Require(result != null, this.Name + " failed; This task requires a MassTagResult, which is null");
            double fwhm = result.IsotopicProfile.GetFWHM();


            XYData theorXYData = new XYData();
            for (int i = 0; i < mt.IsotopicProfile.Peaklist.Count; i++)
            {

                XYData peakXYData = getTheorPeakData(mt.IsotopicProfile.Peaklist[i], fwhm);




            }


            distributionCreator.MolecularFormula = new MolecularFormula();
            distributionCreator.getIsotopicProfile();
            distributionCreator.OffsetDistribution(result.IsotopicProfile);

            AreaFitter areafitter = new AreaFitter(distributionCreator.Data, result.Run.XYData, 10);
            double fitval = areafitter.getFit();

            if (fitval == double.NaN || fitval > 1) fitval = 1;

            result.IsotopicProfile.Score = fitval;

        }

        private XYData getTheorPeakData(MSPeak mSPeak, double fwhm)
        {
            throw new NotImplementedException();
        }

  
    }
}
