using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class UimfScanResult : ScanResult
    {
        public UimfScanResult(FrameSet frameset)
        {
            this.Frameset = frameset;
        }
        
        public int FrameNum { get; set; }

        public FrameSet Frameset { get; set; }

        public double FramePressureFront { get; set; }

        public double FramePressureBack { get; set; }

      
    }
}
