using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PNNLOmics.Data.Constants;
using PNNLOmics.Data.Constants.Libraries;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation
{
    public class BionomialExpansionIsotopicProfileCalculator
    {
        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public void LoadElementData()
        {
            var _elementLibrary = Constants.Elements;
            var carbon = _elementLibrary["C"];
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
