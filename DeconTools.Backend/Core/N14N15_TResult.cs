using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class N14N15_TResult : IMassTagResult
    {
        #region Constructors
        public N14N15_TResult()
        {
            this.IsotopicProfile = new IsotopicProfile();
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

        private IsotopicProfile n15IsotopicProfile;
        public IsotopicProfile N15IsotopicProfile
        {
            get { return n15IsotopicProfile; }
            set { n15IsotopicProfile = value; }
        }




        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        #endregion



        public override double Score
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
