using System;
using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Workflows.Backend.Utilities
{
    public class MathUtilities
    {

        public static double GetStDev(IEnumerable<double> vals)
        {
            double count = vals.Count();
            if (count < 3) return double.MinValue;

            var avg = vals.Average();

            

            double sumSquareDiffs = 0;

            foreach (var v in vals)
            {
                sumSquareDiffs += (v - avg) * (v - avg);
                
            }

            var stdev = (Math.Sqrt(sumSquareDiffs / (vals.Count() - 1)));
            return stdev;

        }

        public static List<double> filterWithGrubbsApplied(List<double> vals)
        {
            var filteredVals = new List<double>();
            if (vals.Count < 3)
            {
                return vals;
            }

            var stdev = MathUtilities.GetStDev(vals);
            var average = vals.Average();

            var zValue = 2;

            foreach (var item in vals)
            {
                var diff = Math.Abs(item - average);
                if (diff < (stdev * zValue))
                {
                    filteredVals.Add(item);
                }

            }

            return filteredVals;

        }



        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
