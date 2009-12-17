using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class N14N15_TResult : IsosResult
    {
        #region Constructors
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

        private List<ChromPeak> chromPeaks;
        public List<ChromPeak> ChromPeaks
        {
            get { return chromPeaks; }
            set { chromPeaks = value; }
        }

        private IsotopicProfile isotopicProfile;

        public override IsotopicProfile IsotopicProfile
        {
            get { return isotopicProfile; }
            set { isotopicProfile = value; }
        }

        public MassTag MassTag { get; set; }

        public XYData ChromValues { get; set; }


        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        #endregion


    }
}
