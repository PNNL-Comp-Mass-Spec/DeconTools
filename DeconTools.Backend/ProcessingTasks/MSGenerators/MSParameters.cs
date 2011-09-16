using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Parameters
{
    [Serializable]
    public class MSParameters : IParameters
    {

        public MSParameters()
            : this(0, 10000)
        {


        }

        public MSParameters(double minMZ, double maxMZ)
        {
            this.minMZ = minMZ;
            this.maxMZ = maxMZ;

        }


        private double minMZ;

        /// <summary>
        /// Minimum m/z value to be used various operations such as MS summing and peak finding
        /// </summary>
        public double MinMZ
        {
            get { return minMZ; }
            set { minMZ = value; }
        }

        private double maxMZ;

        /// <summary>
        /// Maximum m/z value to be used various operations such as MS summing and peak finding
        /// </summary>
        public double MaxMZ
        {
            get { return maxMZ; }
            set { maxMZ = value; }
        }

        





    }
}
