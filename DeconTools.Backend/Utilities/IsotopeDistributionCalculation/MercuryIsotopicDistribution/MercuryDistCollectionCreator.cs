using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities.MercuryIsotopeDistribution
{
#if !Disable_DeconToolsV2

    public class MercuryDistCollectionCreator
    {
        private DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters;
        private MercuryDistributionCreator mercDistCreator;
        private string averagineFormula;
        private string tagFormula;



        public MercuryDistCollectionCreator()
        {
            this.hornParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            
            this.averagineFormula = this.hornParameters.AveragineFormula;
            this.tagFormula = this.hornParameters.TagFormula;

        }

        public MercuryDistCollectionCreator(string averagineFormula, string tagFormula)
        {
            this.averagineFormula = averagineFormula;
            this.tagFormula = tagFormula;
        }

        

        public MercuryDistCollection CreateMercuryDistCollection(double startMass, double stopMass, double stepSize, double fwhm)
        {
            var mercDistCollection = new MercuryDistCollection();
            mercDistCreator = new MercuryDistributionCreator();


            Check.Require(stepSize > 0, "Step size must be greater than 0");
            Check.Require(stopMass >= startMass, "Stop MZ must be greater than Start MZ");
            Check.Require(startMass > 0, "Starting MZ must be greater than 0");

            for (var mass = startMass; mass <= stopMass; mass=mass+stepSize)
            {
                var mercdist = new MercuryDist();
                mercDistCreator.CreateDistribution(mass,1, fwhm);
                mercdist.Xydata = mercDistCreator.Data;
                mercDistCollection.mercDistList.Add(mercdist);
            }


            return mercDistCollection;

        }

    }

#endif

}
