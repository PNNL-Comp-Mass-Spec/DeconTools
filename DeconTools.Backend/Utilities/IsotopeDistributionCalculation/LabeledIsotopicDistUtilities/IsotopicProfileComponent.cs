using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.LabeledIsotopicDistUtilities
{
    public class IsotopicProfileComponent
    {

        public IsotopicProfileComponent(IsotopicProfile iso, double fraction, string description = "")
        {
            IsotopicProfile = iso;
            Fraction = fraction;
            Description = description;
        }


        public IsotopicProfile IsotopicProfile { get; set; }

        public double Fraction { get; set; }

        public string Description { get; set; }



    }
}
