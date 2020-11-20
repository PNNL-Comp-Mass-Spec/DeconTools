
namespace DeconTools.Backend.Core.Results
{
    public class DeconMSnResult : IsosResult
    {
        #region Constructors
        #endregion

        #region Properties

        public double ParentMZ { get; set; }
        public int ParentScan { get; set; }
        public double ParentIntensity { get; set; }
        public double ParentScanTICIntensity { get; set; }

        public int ScanNum { get; set; }

        public string ExtraInfo { get; set; }

        public int ParentChargeState { get; set; }

        public double OriginalMZTarget { get; set; }

        public double IonInjectionTime { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
