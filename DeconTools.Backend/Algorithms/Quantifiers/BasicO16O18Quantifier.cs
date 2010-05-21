using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Algorithms.Quantifiers
{
    public class BasicO16O18Quantifier : O16O18QuantifierBase
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public override double GetRatio(IsotopicProfile isotopicProfile)
        {
            return Get_I0_I4_ratio(isotopicProfile);
        }

        public double Get_I0_I4_ratio(IsotopicProfile iso)
        {
            double i0Intensity = getI0Intensity(iso);
            double i2Intensity = getI2Intensity(iso);
            double i4Intensity = GetI4Intensity(iso);

            if (i4Intensity == 0) i4Intensity = double.Epsilon;

            return i0Intensity / i4Intensity;
        }


        public double GetAdjusted_I0_I4_YeoRatio(IsotopicProfile iso, IsotopicProfile theorIso)
        {
            double i0 = getI0Intensity(iso);
            double i2 = getI2Intensity(iso);
            double i4 = GetI4Intensity(iso);

            double theorI0 = getI0Intensity(theorIso);
            double theorI2 = getI2Intensity(theorIso);
            double theorI4 = GetI4Intensity(theorIso);

            double adjustedI4Intensity;

            if (i4 == 0)
            {
                adjustedI4Intensity = i4;
            }
            else
            {
                // see Yeo et al (2001), Analytical Chemistry. "Proteolytic O-18 labeling for comparative proteomics: Model studies with two serotypes of adenovirus."
                adjustedI4Intensity = i4 - (theorI4 / theorI0 * i0);

                if (i2 > 0)
                {
                    adjustedI4Intensity = adjustedI4Intensity - (theorI2 / theorI0) * (i2 - theorI2 / theorI0 * i0) + 0.5 * (i2 - theorI2 / theorI0 * i0);
                }
                else
                {
                    //TODO:  if there is an intensity at I4, there should be something at I2. 
                }
  
            }





            if (adjustedI4Intensity <= 0) adjustedI4Intensity = 0;


            return i0 / adjustedI4Intensity;


        }




        private double GetI4Intensity(IsotopicProfile iso)
        {
            if (iso == null || iso.Peaklist == null || iso.Peaklist.Count < 5)
            {
                return 0;
            }

            double intensity = iso.Peaklist[4].Height;
            return intensity;
        }

        private double getI2Intensity(IsotopicProfile iso)
        {
            if (iso == null || iso.Peaklist == null || iso.Peaklist.Count < 3)
            {
                return 0;
            }

            double intensity = iso.Peaklist[2].Height;

            return intensity;
        }

        private double getI0Intensity(IsotopicProfile iso)
        {
            if (iso == null || iso.Peaklist == null || iso.Peaklist.Count < 1)
            {
                return 0;
            }

            double intensity = iso.Peaklist[0].Height;
            if (intensity == 0) intensity = 0;

            return intensity;
        }



        #endregion

        #region Private Methods
        #endregion


    }
}
