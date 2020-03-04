using System;
using System.Collections.Generic;
using PNNLOmics.Utilities;

namespace DeconTools.Backend.Core
{
    public class N14N15_TResult : TargetedResultBase
    {

        #region Constructors

        public N14N15_TResult() : base() { }

        public N14N15_TResult(TargetBase target) : base(target) { }

        #endregion

        #region Properties

        public IsotopicProfile IsotopicProfileLabeled { get; set; }


        public IsotopicProfile TheorIsotopicProfileLabeled { get; set; }


        public int NumChromPeaksWithinToleranceForN15Profile { get; set; }
        public ChromPeak ChromPeakSelectedN15 { get; set; }
        public ScanSet ScanSetForN15Profile { get; set; }

        public double InterferenceScoreN15 { get; set; }
        public double ScoreN15 { get; set; }

        public double RatioN14N15 { get; set; }
        public double RatioContributionN14 { get; set; }
        public double RatioContributionN15 { get; set; }



        /// <summary>
        /// Store chromatogram data for one or more peaks from the unlabeled isotopic profile
        /// </summary>
        public Dictionary<MSPeak, XYData> UnlabeledPeakChromData { get; set; }

        /// <summary>
        /// Store chromatogram data for one or more peaks from the labeled isotopic profile
        /// </summary>
        public Dictionary<MSPeak, XYData> LabeledPeakChromData { get; set; }

        #endregion

        #region Public Methods
        public override void DisplayToConsole()
        {
            base.DisplayToConsole();
            Console.WriteLine("Ratio = \t" + StringUtilities.DblToString(RatioN14N15, 2));
        }

        public override void AddSelectedChromPeakAndScanSet(ChromPeak bestPeak, ScanSet scanSet, Globals.IsotopicProfileType isotopicProfileType)
        {
            //if result was not previously processed, will do a standard add of selected chrom peak and scanSet
            //if result was previously processed, add new data to the Labeled results
            if (isotopicProfileType == Globals.IsotopicProfileType.UNLABELED)
            {
                base.AddSelectedChromPeakAndScanSet(bestPeak, scanSet, isotopicProfileType);
            }
            else
            {
                ChromPeakSelectedN15 = bestPeak;
                ScanSetForN15Profile = scanSet;
            }
        }

        public override void AddNumChromPeaksWithinTolerance(int numChromPeaksWithinTolerance)
        {
            if (!WasPreviouslyProcessed)
            {
                base.AddNumChromPeaksWithinTolerance(numChromPeaksWithinTolerance);
            }
            else
            {
                NumChromPeaksWithinToleranceForN15Profile = numChromPeaksWithinTolerance;
            }
        }

        public int GetScanNumN15()
        {
            if (ScanSetForN15Profile == null) return -1;
            else
            {
                return ScanSetForN15Profile.PrimaryScanNumber;
            }
        }

        public double GetNETN15()
        {
            if (ChromPeakSelectedN15 == null) return -1;
            return ChromPeakSelectedN15.NETValue;

        }

        public MSPeak GetMonoisotopicPeakForLabeledProfile()
        {
            if (Target == null ||
                IsotopicProfile == null ||
                IsotopicProfileLabeled == null)
            {
                return null;
            }

            var nitrogenCount = Target.GetAtomCountForElement("N");

            var monoPeakForUnlabeled = IsotopicProfile.getMonoPeak();
            if (monoPeakForUnlabeled == null) return null;

            var expectedMZForLabeled = monoPeakForUnlabeled.XValue + (Globals.N15_MASS - Globals.N14_MASS) * nitrogenCount / IsotopicProfile.ChargeState;

            var monoPeakOfLabeled = Utilities.IsotopicProfileUtilities.GetPeakAtGivenMZ(IsotopicProfileLabeled, expectedMZForLabeled, 0.05);

            return monoPeakOfLabeled;
        }

        #endregion

        #region Private Methods
        #endregion

        internal override void AddLabeledIso(IsotopicProfile labeledIso)
        {
            IsotopicProfileLabeled = labeledIso;
        }

        internal override void AddTheoreticalLabeledIsotopicProfile(IsotopicProfile theorLabeledIso)
        {
            TheorIsotopicProfileLabeled = theorLabeledIso;
        }




    }
}
