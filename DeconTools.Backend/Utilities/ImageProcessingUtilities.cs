using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Utilities
{
    class ImageProcessingUtilities
    {

        public static void GetImageObjectFromIntensity(int [][] intensityData)
        {
            int maximumIntensity = 0;

            for (int i = 0; i < intensityData.Length; i++)
            {
                for (int j = 0; j < intensityData[i].Length; j++)
                {
                    if (intensityData[i][j] > maximumIntensity)
                    {
                        maximumIntensity = intensityData[i][j];
                    }
                }
                
            }


            for (int i = 0; i < intensityData.Length; i++)
            {
                for (int j = 0; j < intensityData[i].Length; j++)
                {
                    intensityData[i][j] /= maximumIntensity;
                }
            }


        }



    }
}
