using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;



namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public class DeconToolsFitScoreCalculator : IFitScoreCalculator
    {
        MercuryDistributionCreator distributionCreator;

        PeakUtilities peakUtil = new PeakUtilities();

        public double MZVar { get; set; }


        public DeconToolsFitScoreCalculator()
        {

            this.distributionCreator = new MercuryDistributionCreator();
        }

        public DeconToolsFitScoreCalculator(double mzVar)
            : this()
        {
            this.MZVar = mzVar;
        }


        public override void GetFitScores(ResultCollection resultList)
        {
            Check.Require(resultList != null, "FitScoreCalculator failed. ResultCollection is null");
            Check.Require(resultList.Run != null, "FitScoreCalculator failed. Run is null");
            Check.Require(resultList.Run.CurrentScanSet != null, "FitScoreCalculator failed. Current scanset has not been defined");


            foreach (IsosResult result in resultList.CurrentScanIsosResultBin)
            {

                double resolution = result.IsotopicProfile.GetMZofMostAbundantPeak() / result.IsotopicProfile.GetFWHM();

                distributionCreator.CreateDistribution(result.IsotopicProfile.MonoIsotopicMass, result.IsotopicProfile.ChargeState, resolution);
                distributionCreator.OffsetDistribution(result.IsotopicProfile);

                AreaFitter areafitter = new AreaFitter(distributionCreator.Data, result.Run.XYData, 10);
                double fitval = areafitter.getFit();

                if (fitval == double.NaN || fitval > 1) fitval = 1;

                result.IsotopicProfile.Score = fitval;


            }
        }

        public override void Cleanup()
        {
            base.Cleanup();

        }



    }
}
