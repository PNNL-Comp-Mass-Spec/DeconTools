using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public abstract class IMassTagResult : IsosResult
    {
        #region Constructors
        #endregion

        #region Properties
        public abstract List<ChromPeak> ChromPeaks { get; set; }

        public abstract MassTag MassTag { get; set; }

        public abstract XYData ChromValues { get; set; }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
