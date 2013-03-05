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

        public List<ChromPeakQualityData> ChromPeakQualityData { get; set; }

        #endregion

        #region Public Methods



        #endregion


    }
}