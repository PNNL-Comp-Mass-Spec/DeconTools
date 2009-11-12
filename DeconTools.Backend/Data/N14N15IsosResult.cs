using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend
{
    [Serializable]
    public class N14N15IsosResult : IsosResult
    {
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

    }
}
