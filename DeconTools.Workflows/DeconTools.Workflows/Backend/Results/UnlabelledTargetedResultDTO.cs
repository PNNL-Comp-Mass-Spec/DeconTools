namespace DeconTools.Workflows.Backend.Results
{
    public class UnlabeledTargetedResultDTO : TargetedResultDTO
    {

        #region Constructors
        public UnlabeledTargetedResultDTO()
        {
            ScanLCStart = 0;
            ScanLCEnd = 0;
            TargetID = 0;
            NET = 0;
            NumChromPeaksWithinTol = 0;

        }
        #endregion

        //public static UnlabeledTargetedResult CreateResult(MassTagResult result)
        //{
        //    UnlabeledTargetedResult r = new UnlabeledTargetedResult();
        //    r.ChargeState = result.MassTag == null ? 0 : result.MassTag.ChargeState;
        //    r.DatasetName = result.Run.DatasetName;
        //    r.IndexOfMostAbundantPeak = result.IsotopicProfile == null ? (short)0 : (short)result.IsotopicProfile.getIndexOfMostIntensePeak();
        //    r.Intensity = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.IntensityAggregate;
        //    r.IntensityI0 = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.GetMonoAbundance();
        //    r.IntensityMostAbundantPeak = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.getMostIntensePeak().Height;
        //    r.IScore = (float)result.InterferenceScore;
        //    r.MassTagID = result.MassTag == null ? 0 : result.MassTag.ID;
        //    r.MonoMass = result.IsotopicProfile.MonoIsotopicMass;
        //    r.MonoMZ = result.IsotopicProfile.MonoPeakMZ;
        //    r.NET = (float)result.GetNET();
        //    r.NumChromPeaksWithinTol = result.NumChromPeaksWithinTolerance;
        //    r.NumQualityChromPeaksWithinTol = result.NumQualityChromPeaks;
        //    r.ScanLC = result.GetScanNum();
        //    if (result.ChromPeakSelected != null)
        //    {
        //        double sigma = result.ChromPeakSelected.Width / 2.35;
        //        r.ScanLCStart = (int)Math.Round(result.ChromPeakSelected.XValue - sigma);
        //        r.ScanLCEnd = (int)Math.Round(result.ChromPeakSelected.XValue + sigma);
        //    }

        //    return r;

        //}


        #region Properties



        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion



    }
}
