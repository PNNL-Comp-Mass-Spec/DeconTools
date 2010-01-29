using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class MassTagResult : IMassTagResult
    {
        #region Constructors
        public MassTagResult()
            : this(null)
        {
        }

        public MassTagResult(MassTag massTag)
        {
            this.IsotopicProfile = new IsotopicProfile();
            this.MassTag = massTag;
        }

        #endregion

        #region Properties
        private Run run;
        public override Run Run
        {
            get { return run; }
            set { run = value; }
        }

        private ScanSet scanSet;
        public override ScanSet ScanSet
        {
            get { return scanSet; }
            set { scanSet = value; }
        }

        private IsotopicProfile isotopicProfile;
        public override IsotopicProfile IsotopicProfile
        {
            get { return isotopicProfile; }
            set { isotopicProfile = value; }
        }

        private ChromPeak chromPeakSelected;

        public override ChromPeak ChromPeakSelected
        {
            get { return chromPeakSelected; }
            set { chromPeakSelected = value; }
        }


        private List<ChromPeak> chromPeaks;
        public override List<ChromPeak> ChromPeaks
        {
            get { return chromPeaks; }
            set { chromPeaks = value; }
        }
        public override MassTag MassTag { get; set; }
        public override XYData ChromValues { get; set; }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion



        public override double Score { get; set; }

    }
}
