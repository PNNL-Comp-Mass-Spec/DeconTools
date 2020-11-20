
namespace DeconTools.Workflows.Backend.Results
{
    public class N14N15TargetedResultDTO : UnlabeledTargetedResultDTO
    {
        #region Properties

        public int ScanN15 { get; set; }
        public int ScanN15Start { get; set; }
        public int ScanN15End { get; set; }
        public float NETN15 { get; set; }
        public short NumChromPeaksWithinTolN15 { get; set; }
        public short NumQualityChromPeaksWithinTolN15 { get; set; }

        public double MonoMassN15 { get; set; }
        public double MonoMassCalibratedN15 { get; set; }
        public double MonoMZN15 { get; set; }
        public float IntensityN15 { get; set; }

        public float FitScoreN15 { get; set; }
        public float IScoreN15 { get; set; }
        public float RatioContributionN14 { get; set; }
        public float RatioContributionN15 { get; set; }
        public float Ratio { get; set; }

        #endregion

    }
}
