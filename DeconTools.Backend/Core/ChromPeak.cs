using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class ChromPeak:Peak
    {
        #region Constructors
        #endregion

        #region Properties

        public double NETValue { get; set; }

        private double scanTime;
        public override double XValue
        {
            get
            {
                return scanTime;
            }
            set
            {
                scanTime = value; ;
            }
        }

        private float intensity;
        public override float Height
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;
            }
        }

        private float width;
        public override float Width
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

        public override string ToString()
        {
            return (XValue.ToString("0.#") + "; " + Height.ToString("0.#"));
        }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
