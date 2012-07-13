using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class MSPeak : Peak
    {

        public MSPeak()
        {
            this.DataIndex = -1;
            this.MSFeatureID = -1;
            
        }

        public MSPeak(double mz, float intensity, float fwhm, float sn)
        {
            this.mZ = mz;
            this.intensity = intensity;
            this.Width = fwhm;
            this.SN = sn;
        }

        private double mZ;

        public override double XValue
        {
            get { return mZ; }
            set { mZ = value; }
        }
        private float intensity;

        public override float Height
        {
            get { return intensity; }
            set { intensity = value; }
        }
        private float fWHM;

        public override float Width
        {
            get { return fWHM; }
            set { fWHM = value; }
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
