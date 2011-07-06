using System;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Results
{
    public class UnlabelledTargetedResult : TargetedResult
    {

        #region Constructors
        public UnlabelledTargetedResult()
        {
            this.ScanLCStart = 0;
            this.ScanLCEnd = 0;
            this.MassTagID = 0;
            this.NET = 0;
            this.NumChromPeaksWithinTol = 0;

        }
        #endregion

        public static UnlabelledTargetedResult CreateResult(MassTagResult result)
        {
            UnlabelledTargetedResult r = new UnlabelledTargetedResult();
            r.ChargeState = result.MassTag == null ? 0 : result.MassTag.ChargeState;
            r.DatasetName = result.Run.DatasetName;
            r.IndexOfMostAbundantPeak = result.IsotopicProfile == null ? (short)0 : (short)result.IsotopicProfile.getIndexOfMostIntensePeak();
            r.Intensity = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.IntensityAggregate;
            r.IntensityI0 = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.GetMonoAbundance();
            r.IntensityMostAbundantPeak = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.getMostIntensePeak().Height;
            r.IScore = (float)result.InterferenceScore;
            r.MassTagID = result.MassTag == null ? 0 : result.MassTag.ID;
            r.MonoMass = result.IsotopicProfile.MonoIsotopicMass;
            r.MonoMZ = result.IsotopicProfile.MonoPeakMZ;
            r.NET = (float)result.GetNET();
            r.NumChromPeaksWithinTol = result.NumChromPeaksWithinTolerance;

            r.ScanLC = result.GetScanNum();
            if (result.ChromPeakSelected != null)
            {
                double sigma = result.ChromPeakSelected.Width / 2.35;
                r.ScanLCStart = (int)Math.Round(result.ChromPeakSelected.XValue - sigma);
                r.ScanLCEnd = (int)Math.Round(result.ChromPeakSelected.XValue + sigma);
            }

            return r;

        }


        #region Properties
      


        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
