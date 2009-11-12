using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class MSPeak
    {

        public MSPeak()
        {

        }

        public MSPeak(double mz, float intensity, float fwhm, float sn)
        {
            this.mZ = mz;
            this.intensity = intensity;
            this.FWHM = fwhm;
            this.SN = sn;

        }
        
        private double mZ;

        public double MZ
        {
            get { return mZ; }
            set { mZ = value; }
        }
        private float intensity;

        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }
        private float fWHM;

        public float FWHM
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
    }
}
