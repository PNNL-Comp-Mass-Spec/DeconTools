
namespace DeconTools.Workflows.Backend.Results
{
    //this is a result class that is free of specialized objects (as opposed to IsosResult or TargetedResultBase)
    public abstract class DeconToolsResultDTO
    {
        #region Constructors
        #endregion

        #region Properties

        /// <summary>
        /// Scan (LC)
        /// </summary>
        public string DatasetName { get; set; }
        public int ScanLC { get; set; }

        public int ChargeState { get; set; }
        public double MonoMass { get; set; }
        public double MonoMassCalibrated { get; set; }
        public double MassErrorBeforeCalibration { get; set; }
        public double MassErrorAfterCalibration { get; set; }

        public double MonoMZ { get; set; }
        public float FitScore { get; set; }
        public float IScore { get; set; }

        //Representative intensity for the isotopic profile.
        public float Intensity { get; set; }

        /// <summary>
        ///Intensity of the monoisotopic peak
        /// </summary>
        public float IntensityI0 { get; set; }

        /// <summary>
        /// Intensity of the most abundant peak of the isotopic profile
        /// </summary>
        public float IntensityMostAbundantPeak { get; set; }

        /// <summary>
        /// Index of the most abundant peak of the isotopic profile
        /// </summary>
        public short IndexOfMostAbundantPeak { get; set; }

        public ValidationCode ValidationCode { get; set; }

        #endregion

    }
}
