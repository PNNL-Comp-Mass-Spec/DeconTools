
namespace DeconTools.Backend.Core
{
    public class ChromPeak:Peak
    {

        public ChromPeak():base()
        {
            NETValue = -1;
        }


        #region Properties

        public double NETValue { get; set; }

        public float SignalToNoise { get; set; }

        #endregion

       
    }
}
