using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class IsotopicProfileChromData
    {
        #region Constructors
        #endregion

        #region Properties

        public IsotopicProfile IsotopicProfile { get; set; }

        public Dictionary<MSPeak, XYData> ChromXYData { get; set; }

        public Dictionary<MSPeak, List<IPeak>> ChromPeakData { get; set; }

        public Dictionary<MSPeak, IPeak> ChromBestPeakData { get; set; }






        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
