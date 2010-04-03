using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        
        
        public double RatioN14N15 { get; set; }


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

        #endregion

        #region Private Methods
        #endregion

        internal override void AddLabelledIso(IsotopicProfile labelledIso)
        {
            this.IsotopicProfileLabeled = labelledIso;
        }




    }
}
