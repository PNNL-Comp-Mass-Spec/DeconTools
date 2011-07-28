
namespace DeconTools.Workflows.Backend.Results
{
    public class AlignmentResult
    {

      

        #region Constructors
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

        public float[] massErrorResidualsBeforeAlignment { get; set; }

        public float[] massErrorResidualsAfterAlignement { get; set; }

        public float[] ScanValuesForMassErrorResiduals { get; set; }

        public float[] mass_vs_mz_residualsBeforeAlignment { get; set; }

        public float[] mass_vs_mz_residualsMZValues { get; set; }

        public float[] mass_vs_mz_residualsAfterAlignment { get; set; }

        public double NETStDev { get; set; }

        public double NETAverage { get; set; }

        public double MassAverage { get; set; }

        public double MassStDev { get; set; }

        public double[,] NETHistogramData { get; set; }

        public double[,] massHistogramData { get; set; }
    }
}
