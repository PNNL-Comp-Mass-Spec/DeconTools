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
            this.ChromID = -1;
            this.PeakID = -1;
            this.Scan_num = -1;
            this.Frame_num = -1;
            this.MSPeak = null;

        }
        public MSPeakResult(int peakID, int scanNum, MSPeak peak):this()
        {
            this.PeakID = peakID;
            this.Scan_num = scanNum;
            this.MSPeak = peak;

        }

        public MSPeakResult(int peakID, int frameNum, int scanNum, MSPeak peak)
            : this(peakID, scanNum, peak)
        {
            this.Frame_num = frameNum;
        }

        public int PeakID { get; set; }
        public int Scan_num { get; set; }

        /// <summary>
        /// Use this property to assign a peak to particular LC chromatogram
        /// </summary>
        public int ChromID { get; set; }

        public int Frame_num { get; set; }


        public MSPeak MSPeak { get; set; }


    }
}
