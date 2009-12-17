using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class ChromPeak
    {
        #region Constructors
        #endregion

        #region Properties
        private float scanTime;

        public float ScanTime
        {
            get { return scanTime; }
            set { scanTime = value; }
        }
        private float intensity;

        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }
        private float width;

        public float Width
        {
            get { return width; }
            set { width = value; }
        }
        private float sigNoise;

        public float SigNoise
        {
            get { return sigNoise; }
            set { sigNoise = value; }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
