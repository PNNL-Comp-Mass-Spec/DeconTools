using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Utilities.MercuryIsotopeDistribution
{
    public class MercuryDistCollection
    {
        public HashSet<MercuryDist> mercDistList { get; set; }

        public MercuryDistCollection()
        {
            mercDistList = new HashSet<MercuryDist>();
        }
    }
}
