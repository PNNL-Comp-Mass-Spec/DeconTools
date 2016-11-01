using System;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for XYValueConverter.
    /// </summary>
    public class XYValueConverter
    {

        public static double[] ConvertFloatsToDoubles(float[] vals)
        {
            if (vals == null) return null;
            double[] outputVals = new double[vals.Length];

            for (int i = 0; i < vals.Length; i++)
            {
                outputVals[i] = (double)vals[i];
            }
            return outputVals;
        }

        public static void ConvertDoublesToFloats(double[] inputVals, ref float[] outputVals)
        {
            if (inputVals == null) return;
            outputVals = new float[inputVals.Length];
            for (int i = 0; i < inputVals.Length; i++)
            {
                outputVals[i] = (float)inputVals[i];
                
            }
        }


    }
}
