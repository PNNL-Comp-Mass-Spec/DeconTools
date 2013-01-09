
namespace DeconTools.Backend.Core.Results
{
    public class DeconMSnResult:IsosResult
    {

        #region Constructors
        #endregion

        #region Properties


        public double ParentMZ { get; set; }
        public double ParentScan { get; set; }
        public double ParentIntensity { get; set; }
        
        public int ScanNum { get; set; }

        public string ExtraInfo { get; set; }

        public int ParentChargeState { get; set; }

        public double OriginalMZTarget { get; set; }

        #endregion

        #region Public Methods




        #endregion

        #region Private Methods

        #endregion

    }
}
