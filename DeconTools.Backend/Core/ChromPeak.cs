
namespace DeconTools.Backend.Core
{
    public class ChromPeak : Peak
    {
        public ChromPeak()
        {
            NETValue = -1;
            IntegratedAbundance = -1;
        }

        public ChromPeak(double xValue, float intensity, float width, float signalToNoise)
            : base(xValue, intensity, width)
        {
            SignalToNoise = signalToNoise;
        }

        #region Properties

        public double NETValue { get; set; }

        public float SignalToNoise { get; set; }

        public double IntegratedAbundance { get; set; }

        #endregion

    }
}
