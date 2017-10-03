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


        public override void AddSelectedChromPeakAndScanSet(ChromPeak bestPeak, ScanSet scanset, Globals.IsotopicProfileType isotopicProfileType)
        {
            //if result was not previously processed, will do a standard add of selected chrom peak and scanset
            //if result was previously processed, add new data to the Labelled results 
            if (isotopicProfileType==Globals.IsotopicProfileType.UNLABELLED)
            {
                base.AddSelectedChromPeakAndScanSet(bestPeak, scanset,isotopicProfileType);
            }
            else
            {
                ChromPeakSelectedN15 = bestPeak;
                ScanSetForN15Profile = scanset;
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

        public MSPeak GetMonoisotopicPeakForLabelledProfile()
        {
            if (Target == null ||
                IsotopicProfile == null ||
                IsotopicProfileLabeled == null)
            {
                return null;
            }

            var numNitrogens = Target.GetAtomCountForElement("N");

            var monoPeakForUnlabelled = IsotopicProfile.getMonoPeak();
            if (monoPeakForUnlabelled == null) return null;

            var expectedMZForLabelled = monoPeakForUnlabelled.XValue+ (Globals.N15_MASS - Globals.N14_MASS) * numNitrogens / IsotopicProfile.ChargeState;

            var monoPeakOfLabelled= Utilities.IsotopicProfileUtilities.GetPeakAtGivenMZ(IsotopicProfileLabeled, expectedMZForLabelled, 0.05);

            return monoPeakOfLabelled;



        }





        #endregion

        #region Private Methods
        #endregion

        internal override void AddLabelledIso(IsotopicProfile labelledIso)
        {
            IsotopicProfileLabeled = labelledIso;
        }

        internal override void AddTheoreticalLabelledIsotopicProfile(IsotopicProfile theorLabelledIso)
        {
            TheorIsotopicProfileLabeled = theorLabelledIso;
        }




    }
}
