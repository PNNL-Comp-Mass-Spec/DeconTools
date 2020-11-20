using System;
using System.Xml.Linq;

namespace DeconTools.Backend.Parameters
{
    [Serializable]
    public class PeakDetectorParameters : ParametersBase
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

        /// <summary>
        /// Set this to True if the peaks will be written to a text file (and thus should be stored in memory)
        /// </summary>
        public bool PeaksAreStored { get; set; }

        public double PeakToBackgroundRatio { get; set; }

        public double SignalToNoiseThreshold { get; set; }

        public bool IsDataThresholded { get; set; }

        public Globals.PeakFitType PeakFitType { get; set; }

        #endregion

        public override void LoadParameters(XElement xElement)
        {
            throw new NotImplementedException();
        }

        public override void LoadParametersV2(XElement xElement)
        {
            PeakToBackgroundRatio = GetDoubleValue(xElement, "PeakBackgroundRatio", PeakToBackgroundRatio);
            SignalToNoiseThreshold = GetDoubleValue(xElement, "SignalToNoiseThreshold", SignalToNoiseThreshold);
            IsDataThresholded = GetBoolVal(xElement, "IsDataThresholded", IsDataThresholded);
            PeakFitType = (Globals.PeakFitType)GetEnum(xElement, "PeakFitType", PeakFitType.GetType(), PeakFitType);

            //TODO: move this parameter
            PeaksAreStored = GetBoolVal(xElement, "WritePeaksToTextFile", PeaksAreStored);
        }
    }
}
