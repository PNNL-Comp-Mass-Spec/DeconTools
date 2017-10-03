using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class MSPeak : Peak
    {

        public MSPeak(double mz, float intensity = 0, float fwhm = 0, float sn = 0)
            : base(mz, intensity, fwhm)
        {

            SignalToNoise = sn;
            MSFeatureID = -1;
        }

        public float SignalToNoise { get; set; }

        /// <summary>
        /// The MSFeatureID to which this peak has been assigned
        /// </summary>
        public int MSFeatureID { get; set; }

    }
}
