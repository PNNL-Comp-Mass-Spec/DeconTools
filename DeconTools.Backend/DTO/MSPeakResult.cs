using DeconTools.Backend.Core;

namespace DeconTools.Backend.DTO
{
    public class MSPeakResult:Peak
    {
        public MSPeakResult()
        {
            this.ChromID = -1;
            this.PeakID = -1;
            this.Scan_num = -1;
            this.FrameNum = -1;
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
            this.FrameNum = frameNum;
        }

        public override float Width
        {
            get
            {
                return this.MSPeak.Width;
            }
            set
            {
                this.MSPeak.Width = value;                     
            }
        }
        public override double XValue
        {
            get { return this.MSPeak.XValue; }
            set { this.MSPeak.XValue = value; }
        }
        
        public override float Height
        {
            get { return this.MSPeak.Height; }
            set { this.MSPeak.Height = value; }
        }

        public int PeakID { get; set; }
        public int Scan_num { get; set; }

        /// <summary>
        /// Use this property to assign a peak to particular LC chromatogram
        /// </summary>
        public int ChromID { get; set; }

        public int FrameNum { get; set; }


        public MSPeak MSPeak { get; set; }

        #region IComparable Members
/*
        public override int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }


            IPeak secondPeak = obj as IPeak;
            if (secondPeak == null)
            {
                return -1;
            }
            else
            {
                
                //TODO:: two peaks are the same if they are within a tolerance of each other in
                //terms of mz, scan and lc frame. in this case we're only implementing mz values
                //
                double toleranceInPPM = 20;
                double differenceInPPM = Math.Abs(1000000 * (secondPeak.XValue - this.XValue) / this.XValue);

                if (differenceInPPM <= toleranceInPPM)
                {
                    return 0;
                }

            }

            return 1;


        }*/

        #endregion
    }
}
