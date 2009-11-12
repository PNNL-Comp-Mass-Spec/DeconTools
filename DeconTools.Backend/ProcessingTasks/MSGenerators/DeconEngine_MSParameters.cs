using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Parameters;

namespace DeconTools.Backend
{
    public class DeconEngine_MSParameters : MSParameters
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scanNumber"></param>
        /// <param name="numScansToSum">the number of MS spectra to sum together. Default is '1'</param>
        #region Constructors

        public DeconEngine_MSParameters()
        {
            this.MinMZ = 200;       //default value
            this.MaxMZ = 2000;      //default value

        }

        public DeconEngine_MSParameters(int lowerScanIndex, int upperScanIndex, double minMZ, double maxMZ)
            : this()
        {
            this.LowerScanIndex = lowerScanIndex;
            this.UpperScanIndex = upperScanIndex;
            this.MinMZ = minMZ;
            this.MaxMZ = maxMZ;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scanNumber"></param>
        /// <param name="numScansToSum">Minimum is '1'; set to a large number ('100000') to sum all MS spectra</param>
        public DeconEngine_MSParameters(int scanIndex, int numScansToSum)
            : this()
        {
            this.currentScanNumber = scanIndex;
            this.NumScansToSum = numScansToSum;
            setLowerAndUpperScans();

        }

        public DeconEngine_MSParameters(int scanIndex, int numScansToSum, double minMZ, double maxMZ, bool dummy)
            : this(scanIndex, numScansToSum)
        {
            this.MinMZ = minMZ;
            this.MaxMZ = maxMZ;
        }
        
        #endregion

     
        #region Properties

        private int currentScanNumber;
        public int CurrentScanNumber
        {
            get { return currentScanNumber; }
            set { currentScanNumber = value; }
        }

        private int numScansToSum;
        public int NumScansToSum
        {
            get { return numScansToSum; }
            set
            {
                if (value < 1) value = 1;       //minimum NumScansToSum must be 1
                numScansToSum = value;
            }
        }

        private int lowerScanIndex;
        public int LowerScanIndex              //NOTE: deconEngine uses a 1-based scanNumber!
        {
            get { return lowerScanIndex; }
            set
            {
                if (value < 1) value = 1;       //ensures there are no negative scan index values
                lowerScanIndex = value;
            }
        }

        private int upperScanIndex;
        public int UpperScanIndex
        {
            get { return upperScanIndex; }
            set
            {
                if (value < 0) value = 0;
                upperScanIndex = value;
            }
        }
        
        #endregion

        private void setLowerAndUpperScans()
        {
            int halfWidth = (this.NumScansToSum - 1) / 2;      // beware that this works for values 1,3,5 ... even numbers will cause an integer rounding effect
            this.LowerScanIndex = this.currentScanNumber - halfWidth;
            this.UpperScanIndex = this.currentScanNumber + halfWidth;
        }






    }
}
