using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Algorithms.Quantifiers
{
    public class BasicO16O18Quantifier
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public double Get_I0_I4_ratio(IsotopicProfile iso)
        {
            double i0Intensity = getI0Intensity(iso);
            double i2Intensity = getI2Intensity(iso);
            double i4Intensity = GetI4Intensity(iso);

            return i0Intensity / i4Intensity;
        }



        private double GetI4Intensity(IsotopicProfile iso)
        {
            if (iso == null || iso.Peaklist == null || iso.Peaklist.Count < 5)
            {
                return double.Epsilon;
            }

            double intensity = iso.Peaklist[4].Height;
            if (intensity == 0) intensity = double.Epsilon;

            return intensity;
        }

        private double getI2Intensity(IsotopicProfile iso)
        {
            if (iso == null || iso.Peaklist == null || iso.Peaklist.Count < 3)
            {
                return double.Epsilon;
            }

            double intensity = iso.Peaklist[2].Height;
            if (intensity == 0) intensity = double.Epsilon;

            return intensity;
        }

        private double getI0Intensity(IsotopicProfile iso)
        {
            if (iso == null || iso.Peaklist == null || iso.Peaklist.Count < 1)
            {
                return double.Epsilon;
            }

            double intensity = iso.Peaklist[2].Height;
            if (intensity == 0) intensity = double.Epsilon;

            return intensity;
        }



        #endregion

        #region Private Methods
        #endregion
    }
}
