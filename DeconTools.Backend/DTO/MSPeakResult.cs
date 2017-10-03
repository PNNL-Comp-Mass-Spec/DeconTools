using DeconTools.Backend.Core;

namespace DeconTools.Backend.DTO
{
    public class MSPeakResult : Peak
    {
        public MSPeakResult()
        {
            ChromID = -1;
            PeakID = -1;
            Scan_num = -1;
            FrameNum = -1;
            MSPeak = null;

        }
        public MSPeakResult(int peakID, int scanNum, MSPeak peak):this()
        {
            PeakID = peakID;
            Scan_num = scanNum;
            MSPeak = peak;

        }

        public MSPeakResult(int peakID, int frameNum, int scanNum, MSPeak peak)
            : this(peakID, scanNum, peak)
        {
            FrameNum = frameNum;
        }

        public new float Width
        {
            get => MSPeak.Width;
            set => MSPeak.Width = value;
        }
        public new double XValue
        {
            get => MSPeak.XValue;
            set => MSPeak.XValue = value;
        }

        public new float Height
        {
            get => MSPeak.Height;
            set => MSPeak.Height = value;
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
