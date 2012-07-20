using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class MSPeak : Peak
    {

        public MSPeak():base()
        {
            this.DataIndex = -1;
            this.MSFeatureID = -1;
            
        }

        public MSPeak(double mz, float intensity, float fwhm, float sn)
        {
            this.XValue = mz;
            this.Height = intensity;
            this.Width = fwhm;
            this.SN = sn;
        }

       
      
        private float sN;

        public float SN
        {
            get { return sN; }
            set { sN = value; }
        }

       
        /// <summary>
        /// The MSFeatureID to which this peak has been assigned
        /// </summary>
        public int MSFeatureID { get; set; }

    }
}
