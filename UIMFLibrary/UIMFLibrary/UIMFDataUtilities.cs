using System;
using System.Collections.Generic;
using System.Text;

namespace UIMFLibrary
{
    public class UIMFDataUtilities
    {

        public static void ParseOutZeroValues(ref double[] xvals, ref int[] yvals)
        {
            ParseOutZeroValues(ref xvals, ref yvals, 1, 100000);
        }
        
        public static void ParseOutZeroValues(ref double[] xvals, ref int[] yvals, double minMZ, double maxMZ)
        {

            int intensityArrLength = yvals.Length;
            int[] tempIntensities = yvals;
            int zeros = 0;


            for (int k = 0; k < intensityArrLength; k++)
            {
                if (tempIntensities[k] != 0 && (minMZ <= xvals[k] && maxMZ >= xvals[k]))
                {
                    xvals[k - zeros] = xvals[k];
                    yvals[k - zeros] = tempIntensities[k];
                }
                else
                {
                    zeros++;
                }
            }
            // resize arrays cutting off the zeroes at the end.
            Array.Resize(ref xvals, intensityArrLength - zeros);
            Array.Resize(ref yvals, intensityArrLength - zeros);

        }


    }
}
