
namespace DeconTools.Backend.Parameters
{
    public class PeakDetectorParameters
    {

        #region Constructors
        public PeakDetectorParameters()
        {
            MinX = double.MinValue;
            MaxX = double.MaxValue;
            PeaksAreStored = false;
            PeakToBackgroundRatio = 3;
            SignalToNoiseThreshold = 2;
            IsDataThresholded = false;
            PeakFitType = Globals.PeakFitType.QUADRATIC;
        }

        #endregion

        #region Properties

        public double MinX { get; set; }

        public double MaxX { get; set; }

        public bool PeaksAreStored { get; set; }

        public double PeakToBackgroundRatio { get; set; }

        public double SignalToNoiseThreshold { get; set; }

        public bool IsDataThresholded { get; set; }

        public Globals.PeakFitType PeakFitType { get; set; }

        #endregion

   

    }
}
