
namespace DeconTools.Workflows.Backend.Results
{
    public class AlignmentResult
    {
        #region Constructors
        public AlignmentResult()
        {
            MassAverage = 0;
            MassStDev = 0;
            NETAverage = 0;
            NETStDev = 0;
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public float[] ScanLCValues { get; set; }

        public float[] NETValues { get; set; }

        public float[,] AlignmentHeatmapScores { get; set; }

        public float[] Mass_vs_scan_ResidualsBeforeAlignment { get; set; }
        public float[] Mass_vs_scan_ResidualsAfterAlignment { get; set; }
        public float[] Mass_vs_scan_ResidualsScanValues { get; set; }

        public float[] Mass_vs_mz_ResidualsBeforeAlignment { get; set; }
        public float[] Mass_vs_mz_ResidualsAfterAlignment { get; set; }
        public float[] Mass_vs_mz_ResidualsMZValues { get; set; }

        public float[] NET_vs_scan_ResidualsBeforeAlignment { get; set; }
        public float[] NET_vs_scan_ResidualsAfterAlignment { get; set; }
        public float[] NET_vs_scan_ResidualsScanValues { get; set; }

        public float[] NET_vs_MZ_ResidualsBeforeAlignment { get; set; }
        public float[] NET_vs_MZ_ResidualsAfterAlignment { get; set; }
        public float[] NET_vs_MZ_ResidualsMZValues { get; set; }

        public double NETStDev { get; set; }

        public double NETAverage { get; set; }

        public double MassAverage { get; set; }

        public double MassStDev { get; set; }

        public double[,] NETHistogramData { get; set; }

        public double[,] massHistogramData { get; set; }
    }
}
