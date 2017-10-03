using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Utilities.Converters
{

    //TODO: REVISIT
    public class IntensityToImageConverter //should implement TASK so that we can run execute on this, we might not expose any of the methods in that case.
    {
        private ColorBlend color_blend;
        private float[] color_positions;

        private readonly float[] COLOR_POS = { 0.0f, 0.4F, .75f, .86f, .91f, .975f, .995F, 1.0f };

        public IntensityToImageConverter()
        {
            initializeColorMap();
        }


        public void initializeColorMap()
        {
            color_blend = new ColorBlend();
            color_positions = new[] { COLOR_POS[0], COLOR_POS[1], COLOR_POS[2], COLOR_POS[3], COLOR_POS[4], COLOR_POS[5], COLOR_POS[6], COLOR_POS[7] };
            color_blend.Colors = new[] { Color.Purple, Color.Red, Color.Yellow, Color.GreenYellow, Color.Lime, Color.SkyBlue, Color.Blue, Color.DarkBlue };
            color_blend.Positions = new float[color_positions.Length];
            for (var i = 0; i < color_positions.Length; i++)
            {
                color_blend.Positions[i] = color_positions[i];
            }

        }

        /// <summary>
        /// TODO: add info here
        /// </summary>
        /// <param name="intensityMap"></param>
        /// <param name="maxIntensity"></param>
        /// <param name="threshold"></param>
        /// <param name="startFrameInMap"></param>
        /// <param name="startScanInMap"></param>
        /// <param name="startFrame"></param>
        /// <param name="startScan"></param>
        /// <param name="frameAndScanNumbers"></param>
        /// <param name="minimumScanNumber"></param>
        /// <param name="maximumScanNumber"></param>
        /// <param name="totalSummed"></param>
        /// <returns></returns>
        public List<MSPeakResult> getFrameAndScanNumberListFromIntensityMap(int[][] intensityMap, int maxIntensity, float threshold, ushort startFrameInMap, ushort startScanInMap, ushort startFrame, ushort startScan, Dictionary<ushort, List<ushort>> frameAndScanNumbers, out ushort minimumScanNumber, out ushort maximumScanNumber, out ushort totalSummed)
        {
            var peaksForCurveFitting = new List<MSPeakResult>(3000);
            var peakId = 0;
            var frameIndex = startFrameInMap;
            ushort scanIndex;
            ushort scanNumber;
            var frameNumber = startFrame;
            totalSummed = 0;

            //multiply the threshold by max intensity for now
            threshold *= maxIntensity;

            //start at the center of the map
            minimumScanNumber = ushort.MaxValue;
            maximumScanNumber = ushort.MinValue;

            //go up from the start frame value
            while (frameIndex > 0 && intensityMap[frameIndex][startScanInMap] >= threshold)
            {
                var scanNumberList = new List<ushort>(200);
                var end = intensityMap[frameIndex].Length;
                scanIndex = startScanInMap;
                scanNumber = startScan;
                //go left to determine the first value taht's below the threshold
                while (scanIndex > 0 && intensityMap[frameIndex][scanIndex] > threshold)
                {
                    const double mz = 0;
                    var intensity = intensityMap[frameIndex][scanIndex];
                    var peak = new MSPeak(mz, intensity);

                    var msPeak = new MSPeakResult(peakId++, frameNumber, scanNumber, peak);
                    peaksForCurveFitting.Add(msPeak);

                    if (scanNumber < minimumScanNumber)
                    {
                        minimumScanNumber = scanNumber;
                    }
                    else if (scanNumber > maximumScanNumber)
                    {
                        maximumScanNumber = scanNumber;
                    }

                    scanNumberList.Add(scanNumber--);

                    scanIndex--;

                }


                //start the search for the right value from the next scan
                scanIndex = (ushort)(startScanInMap + 1);
                scanNumber = (ushort)(startScan + 1);
                //go right to determine the next value that's below the threshold
                while (scanIndex < end && intensityMap[frameIndex][scanIndex] > threshold)
                {
                    const double mz = 0;
                    var intensity = intensityMap[frameIndex][scanIndex];
                    var peak = new MSPeak(mz, intensity);

                    var msPeak = new MSPeakResult(peakId++, frameNumber, scanNumber, peak);
                    peaksForCurveFitting.Add(msPeak);
                    if (scanNumber < minimumScanNumber)
                    {
                        minimumScanNumber = scanNumber;
                    }
                    else if (scanNumber > maximumScanNumber)
                    {
                        maximumScanNumber = scanNumber;
                    }

                    scanNumberList.Add(scanNumber++);
                    scanIndex++;
                }

                frameIndex--;

                //this means we've finished adding scan numbers to the list for current frame
                //now check if that is more than 3
                if (scanNumberList.Count < 3)
                {
                    break;
                }
                scanNumberList.Sort();
                frameAndScanNumbers.Add(frameNumber, scanNumberList);
                totalSummed += (ushort)scanNumberList.Count;

                frameNumber--;
            }

            //go down
            frameIndex = (ushort)(startFrameInMap + 1);
            frameNumber = (ushort)(startFrame + 1);

            while (frameIndex < intensityMap.Length - 1 && intensityMap[frameIndex][startScanInMap] >= threshold)
            {
                //processing frame
                // Console.WriteLine("processing frame " + frameIndex);
                var scanNumberList = new List<ushort>(200);


                var end = intensityMap[frameIndex].Length;
                scanIndex = startScanInMap;
                scanNumber = startScan;
                //go left to determine the first value taht's below the threshold
                while (scanIndex > 0 && intensityMap[frameIndex][scanIndex] > threshold)
                {
                    const double mz = 0;
                    var intensity = intensityMap[frameIndex][scanIndex];
                    var peak = new MSPeak(mz, intensity);

                    var msPeak = new MSPeakResult(peakId++, frameNumber, scanNumber, peak);
                    peaksForCurveFitting.Add(msPeak);
                    if (scanNumber < minimumScanNumber)
                    {
                        minimumScanNumber = scanNumber;
                    }
                    else if (scanNumber > maximumScanNumber)
                    {
                        maximumScanNumber = scanNumber;
                    }

                    scanNumberList.Add(scanNumber--);
                    scanIndex--;

                }

                //start the search for the right value from the next scan
                scanIndex = (ushort)(startScanInMap + 1);
                scanNumber = (ushort)(startScan + 1);
                //go right to determine the next value that's below the threshold
                while (scanIndex < end && intensityMap[frameIndex][scanIndex] > threshold)
                {
                    const double mz = 0;
                    var intensity = intensityMap[frameIndex][scanIndex];
                    var peak = new MSPeak(mz, intensity);

                    var msPeak = new MSPeakResult(peakId++, frameNumber, scanNumber, peak);
                    peaksForCurveFitting.Add(msPeak);
                    if (scanNumber < minimumScanNumber)
                    {
                        minimumScanNumber = scanNumber;
                    }
                    else if (scanNumber > maximumScanNumber)
                    {
                        maximumScanNumber = scanNumber;
                    }

                    scanNumberList.Add(scanNumber++);
                    scanIndex++;
                }


                frameIndex++;

                //this means we've finished adding scan numbers to the list for current frame
                //now check if that is more than 3
                if (scanNumberList.Count < 3)
                {
                    break;
                }

                scanNumberList.Sort();
                frameAndScanNumbers.Add(frameNumber, scanNumberList);
                totalSummed += (ushort)scanNumberList.Count;
                frameNumber++;
            }

            return peaksForCurveFitting;

        }

        public Bitmap getBitMapFromIntensityMap(int[][] intensities, int maxIntensity, int frames, int scans, float threshold)
        {

            //the x dimension for intensities here is SCANS and the y dimension is FRAMES
            var imageFile = new Bitmap(frames, scans);


            //go through the frames first
            for (var i = 0; i < frames; i++)
            {
                for (var j = 0; j < scans; j++)
                {
                    if (intensities[i][j] == 0)
                    {
                        imageFile.SetPixel(i, j, Color.Blue);
                    }
                    else
                    {
                        var intensity = (float)intensities[i][j] / maxIntensity;

                        if (intensity < threshold)
                        {
                            imageFile.SetPixel(i, j, Color.Blue);
                        }
                        else
                        {
                            imageFile.SetPixel(i, j, getRGB(intensity));
                        }
                    }
                }

            }

            //imageFile.Save("C:\\ProteomicsSoftwareTools\\TestImage.png", System.Drawing.Imaging.ImageFormat.Png);
            return imageFile;


        }

        public Color getColorFromIntensity(int colorAsInt)
        {
            var red = (colorAsInt >> 0x18) & 0xff;
            Console.WriteLine(red);
            var green = (colorAsInt >> 0x10) & 0xff;
            Console.WriteLine(green);
            var blue = colorAsInt & 0xff;
            Console.WriteLine(blue);
            return Color.FromArgb((byte)((colorAsInt >> 0x18) & 0xff),
                          (byte)((colorAsInt >> 0x10) & 0xff),
                          (byte)((colorAsInt >> 8) & 0xff),
                          (byte)(colorAsInt & 0xff));
        }


        public Color getRGB(float intensity)
        {
            int red = 0, green = 0, blue = 0;

            for (var i = 1; i < color_blend.Positions.Length; i++)
            {
                if (((float)1.0 - intensity) <= color_blend.Positions[i])
                {
                    var interp = (((float)1.0 - intensity) - color_blend.Positions[i - 1]) / (color_blend.Positions[i] - color_blend.Positions[i - 1]);

                    red = (int)((color_blend.Colors[i].R - color_blend.Colors[i - 1].R) * interp) + color_blend.Colors[i - 1].R;
                    green = (int)(((color_blend.Colors[i].G - color_blend.Colors[i - 1].G) * interp)) + color_blend.Colors[i - 1].G;
                    blue = (int)(((color_blend.Colors[i].B - color_blend.Colors[i - 1].B) * interp)) + color_blend.Colors[i - 1].B;
                    break;
                }
            }

            Color c;

            try
            {
                c = Color.FromArgb(red, green, blue);
            }
            catch (Exception)
            {
                c = Color.Blue;
            }

            return c;


        }

    }


}
