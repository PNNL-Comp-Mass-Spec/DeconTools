using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class MSPeak : Peak
    {

        public MSPeak()
            : base()
        {
            

        }

        public MSPeak(double mz, float intensity, float fwhm, float sn)
            : base(mz, intensity, fwhm)
        {

            this.SignalToNoise = sn;
            this.MSFeatureID = -1;
        }


        public float SignalToNoise { get; set; }


        /// <summary>
        /// The MSFeatureID to which this peak has been assigned
        /// </summary>
        public int MSFeatureID { get; set; }

    }
}
