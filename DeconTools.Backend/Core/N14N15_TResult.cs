using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Core
{
    public class N14N15_TResult : MassTagResultBase
    {


        #region Constructors

        public N14N15_TResult()
            : this(null)
        {

        }


        public N14N15_TResult(MassTag massTag)
        {
            this.IsotopicProfile = new IsotopicProfile();
            this.MassTag = massTag;
        }
        #endregion

        #region Properties


        private IsotopicProfile m_IsotopicProfileLabeled;
        public IsotopicProfile IsotopicProfileLabeled
        {
            get { return m_IsotopicProfileLabeled; }
            set { m_IsotopicProfileLabeled = value; }
        }


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
            Console.WriteLine("Ratio = \t" + this.RatioN14N15.ToString("0.##"));
        }


        public override void AddSelectedChromPeakAndScanSet(ChromPeak bestPeak, ScanSet scanset)
        {
            //if result was not previously processed, will do a standard add of selected chrom peak and scanset
            //if result was previously processed, add new data to the Labelled results 
            if (!WasPreviouslyProcessed)
            {
                base.AddSelectedChromPeakAndScanSet(bestPeak, scanset);
            }
            else
            {
                this.ChromPeakSelectedN15 = bestPeak;
                this.ScanSetForN15Profile = scanset;
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
                this.NumChromPeaksWithinToleranceForN15Profile = numChromPeaksWithinTolerance;
            }
        }

        public int GetScanNumN15()
        {
            if (this.ScanSetForN15Profile == null) return -1;
            else
            {
                return this.ScanSetForN15Profile.PrimaryScanNumber;
            }
        }


        public double GetNETN15()
        {
            if (this.ChromPeakSelectedN15 == null) return -1;
            return this.ChromPeakSelectedN15.NETValue;

        }

        public MSPeak GetMonoisotopicPeakForLabelledProfile()
        {
            if (this.MassTag == null ||
                this.IsotopicProfile == null ||
                this.IsotopicProfileLabeled == null)
            {
                return null;
            }

            int numNitrogens = this.MassTag.Peptide.GetElementQuantity("N");

            MSPeak monoPeakForUnlabelled = this.IsotopicProfile.getMonoPeak();
            if (monoPeakForUnlabelled == null) return null;

            double expectedMZForLabelled = monoPeakForUnlabelled.XValue+ (Globals.N15_MASS - Globals.N14_MASS) * numNitrogens / this.IsotopicProfile.ChargeState;

            MSPeak monoPeakOfLabelled= Utilities.IsotopicProfileUtilities.GetPeakAtGivenMZ(this.IsotopicProfileLabeled, expectedMZForLabelled, 0.05);

            return monoPeakOfLabelled;



        }





        #endregion

        #region Private Methods
        #endregion

        internal override void AddLabelledIso(IsotopicProfile labelledIso)
        {
            this.IsotopicProfileLabeled = labelledIso;
        }

        internal override void AddTheoreticalLabelledIsotopicProfile(IsotopicProfile theorLabelledIso)
        {
            this.TheorIsotopicProfileLabeled = theorLabelledIso;
        }




    }
}
