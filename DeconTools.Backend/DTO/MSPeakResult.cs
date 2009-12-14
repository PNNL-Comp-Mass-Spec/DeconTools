using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.DTO
{
    public class MSPeakResult
    {
        public MSPeakResult()
        {

        }
        public MSPeakResult(int peakID, int scanNum, MSPeak peak)
        {
            this.PeakID = peakID;
            this.Scan_num = scanNum;
            this.MSPeak = peak;
        }

        public MSPeakResult(int peakID, int frameNum, int scanNum, MSPeak peak)
        {
            this.PeakID = peakID;
            this.Frame_num = frameNum;
            this.Scan_num = scanNum;
            this.MSPeak = peak;
        }

        public int PeakID { get; set; }
        public int Scan_num { get; set; }
        public int Frame_num { get; set; }
        public MSPeak MSPeak { get; set; }


    }
}
