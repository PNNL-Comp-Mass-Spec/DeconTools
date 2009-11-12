using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend
{
    [Serializable]
    public class StandardIsosResult : IsosResult
    {

        public StandardIsosResult()
        {

        }
        
        public StandardIsosResult(Run run, ScanSet scanset)
        {
            this.run = run;
            this.scanSet = scanset;
        }


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
