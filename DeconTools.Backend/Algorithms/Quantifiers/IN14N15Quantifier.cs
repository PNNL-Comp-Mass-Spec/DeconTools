using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Algorithms.Quantifiers
{
    public abstract class N14N15Quantifier
    {
        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public abstract double GetRatio(double[] xvals, double[] yvals,
            DeconTools.Backend.Core.IsotopicProfile iso1, DeconTools.Backend.Core.IsotopicProfile iso2,
            double backgroundIntensity);
     

        #endregion

        #region Private Methods
        #endregion
    }
}
