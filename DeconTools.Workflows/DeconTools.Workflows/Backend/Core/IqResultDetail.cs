using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqResultDetail
    {
        #region Properties

        public XYData Chromatogram { get; set; }

        public XYData MassSpectrum { get; set; }

        /// <summary>
        /// Not used by new workflows. Remove When Ready
        /// </summary>

        public List<ChromPeakQualityData> ChromPeakQualityData { get; set; }

        #endregion

        #region Public Methods



        #endregion


    }
}
