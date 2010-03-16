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


        private IsotopicProfile m_n15IsotopicProfile;
        public IsotopicProfile N15IsotopicProfile
        {
            get { return m_n15IsotopicProfile; }
            set { m_n15IsotopicProfile = value; }
        }

        public double RatioN14N15 { get; set; }




        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        #endregion

        internal override void AddLabelledIso(IsotopicProfile labelledIso)
        {
            this.N15IsotopicProfile = labelledIso;
        }




    }
}
