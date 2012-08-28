using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIMFLibrary.UnitTests
{
    public class TestUtilities
    {

        public static void displayRawMassSpectrum(double[] mzValues, int[] intensities)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mzValues.Length; i++)
            {
                sb.Append(mzValues[i]);
                sb.Append("\t");
                sb.Append(intensities[i]);
                sb.Append(Environment.NewLine);


            }

            Console.WriteLine(sb.ToString());
        }



        public static int getMax(int[] values)
        {
            int max = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                {
                    max = values[i];
                }

            }
            return max;
        }


        public static int getMax(int[][] values, out int xcoord, out int ycoord)
        {
            int max = 0;
            xcoord = 0;
            ycoord = 0;

            for (int i = 0; i < values.Length; i++)
            {
                for (int j = 0; j < values[i].Length; j++)
                {
                    if (values[i][j] > max)
                    {
                        max = values[i][j];
                        xcoord = i;
                        ycoord = j;
                    }
                }
            }

            return max;
        }

        public static void printAsAMatrix(int[] frameVals, float[] intensityVals, float cutoff)
        {
            StringBuilder sb = new StringBuilder();
            int frameValue = frameVals[0];
            for (int i = 0; i < frameVals.Length; i++)
            {

                if (frameValue != frameVals[i])
                {
                    sb.Append("\n");
                    frameValue = frameVals[i];
                }
                else
                {
                    if (intensityVals[i] < cutoff)
                    {
                        sb.Append("0,");
                    }
                    else
                    {
                        sb.Append(intensityVals[i] + ",");
                    }
                }

            }

            Console.WriteLine(sb.ToString());


        }

        public static void printAsAMatrix(int[] frameVals, int[] intensityVals, float cutoff)
        {
            StringBuilder sb = new StringBuilder();
            int frameValue = frameVals[0];
            for (int i = 0; i < frameVals.Length; i++)
            {

                if (frameValue != frameVals[i])
                {
                    sb.Append("\n");
                    frameValue = frameVals[i];
                }
                else
                {
                    if (intensityVals[i] < cutoff)
                    {
                        sb.Append("0,");
                    }
                    else
                    {
                        sb.Append(intensityVals[i] + ",");
                    }
                }

            }

            Console.WriteLine(sb.ToString());


        }


        public static void displayFrameParameters(FrameParameters fp)
        {
            StringBuilder sb = new StringBuilder();

            string separator = Environment.NewLine;

            sb.Append("avg TOF length = \t"+ fp.AverageTOFLength);
            sb.Append(separator);
            sb.Append("cal intercept = \t" + fp.CalibrationIntercept);
            sb.Append(separator);
            sb.Append("cal slope = \t" + fp.CalibrationSlope);
            sb.Append(separator);
            sb.Append("frame type = \t" + fp.FrameType);
            sb.Append(separator);
            sb.Append("pressure back = \t" + fp.PressureBack);
            sb.Append(separator);
            sb.Append("pressure front = \t" + fp.PressureFront);
            sb.Append(separator);
            sb.Append("high pressure funnel pressure= \t" + fp.HighPressureFunnelPressure);
            sb.Append(separator);
            sb.Append("ion funnel trap pressure= \t" + fp.IonFunnelTrapPressure);
            sb.Append(separator);
            sb.Append("quadrupole pressure = \t" + fp.QuadrupolePressure);
            sb.Append(separator);
            sb.Append("rear ion funnel pressure = \t" + fp.RearIonFunnelPressure);
            sb.Append(separator);
            sb.Append("start time = \t" + fp.StartTime);
            sb.Append(separator);
            sb.Append("num scans = \t" + fp.Scans);
            sb.Append(separator);
            sb.Append("IMF profile = \t" + fp.IMFProfile);

            Console.WriteLine(sb.ToString());

        }


        public static void display2DChromatogram(int[] frameORScanVals, int[] intensityVals)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < frameORScanVals.Length; i++)
            {
                sb.Append(frameORScanVals[i]);
                sb.Append("\t");
                sb.Append(intensityVals[i]);
                sb.Append(Environment.NewLine);


            }

            Console.WriteLine(sb.ToString());
        }
    }
}
