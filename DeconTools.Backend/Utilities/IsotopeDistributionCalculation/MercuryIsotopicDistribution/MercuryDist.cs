using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Utilities.MercuryIsotopeDistribution
{
    public class MercuryDist
    {
        private IsotopicProfile isotopicProfile;

        public IsotopicProfile IsotopicProfile
        {
            get { return isotopicProfile; }
            set { isotopicProfile = value; }
        }
        private XYData xydata;

        public XYData Xydata
        {
            get { return xydata; }
            set { xydata = value; }
        }

        public MercuryDist()
        {
            isotopicProfile = new IsotopicProfile();
            xydata = new XYData();

        }
    }
}
