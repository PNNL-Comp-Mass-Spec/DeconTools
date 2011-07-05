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
            this.FrameSet = frameset;
            this.ScanSet = scanset;
            
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
