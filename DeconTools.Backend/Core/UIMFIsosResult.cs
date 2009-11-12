using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class UIMFIsosResult : IsosResult
    {

        public UIMFIsosResult()
        {

        }
        
        public UIMFIsosResult(Run run, FrameSet frameset, ScanSet scanset)
        {
            this.Run = run;
            this.frameSet = frameset;
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

        private FrameSet frameSet;

        public FrameSet FrameSet
        {
            get { return frameSet; }
            set { frameSet = value; }
        }

        private double driftTime;

        public double DriftTime
        {
            get { return driftTime; }
            set { driftTime = value; }
        }

        

    }
}
