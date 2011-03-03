using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Utilities.Converters
{

    //TODO: REVISIT 
    public class IntensityToImageConverter //should implement TASK so that we can run execute on this, we might not expose any of the methods in that case. 
    {
        private ColorBlend color_blend;
        private float[] color_positions;

        private float[] COLOR_POS = new float[] { 0.0f, 0.4F, .75f, .86f, .91f, .975f, .995F, 1.0f };

        public IntensityToImageConverter()
        {
            initializeColorMap();
        }


        public void initializeColorMap(){
            color_blend = new ColorBlend();
            color_positions = new float[] { COLOR_POS[0], COLOR_POS[1], COLOR_POS[2], COLOR_POS[3], COLOR_POS[4], COLOR_POS[5], COLOR_POS[6], COLOR_POS[7]};
            color_blend.Colors = new Color[] { Color.Purple, Color.Red, Color.Yellow, Color.GreenYellow, Color.Lime, Color.SkyBlue, Color.Blue, Color.DarkBlue};
            color_blend.Positions = new float[color_positions.Length];
            for (int i = 0; i < color_positions.Length; i++)
            {
                color_blend.Positions[i] = color_positions[i];
            }

        }

        public List<MSPeakResult> getFrameAndScanNumberListFromIntensityMap(int[][] intensityMap, int maxIntensity, float threshold, ushort startFrameInMap, ushort startScanInMap, ushort startFrame, ushort startScan, Dictionary<ushort, List<ushort>> frameAndScanNumbers, out ushort minimumScanNumber, out ushort maximumScanNumber, out ushort totalSummed)
        {
            List<MSPeakResult> peaksForCurveFitting = new List<MSPeakResult>();
            int peakId = 0;
            ushort frameIndex = startFrameInMap;
            ushort scanIndex = startScanInMap;
            ushort scanNumber = startScan;
            ushort frameNumber = startFrame;
            totalSummed = 0;
            //multiply the threshold by max intensity for now
             threshold *= maxIntensity;

             //start at the center of the map
             minimumScanNumber = ushort.MaxValue;
             maximumScanNumber = ushort.MinValue;

                //go up from the start frame value
                while (frameIndex > 0 && intensityMap[frameIndex][startScanInMap] >= threshold )
                {
                    List<ushort> scanNumberList = new List<ushort>();
                    int end = intensityMap[frameIndex].Length;
                    scanIndex = startScanInMap;
                    scanNumber = startScan;
                    //go left to determine the first value taht's below the threshold
                    while (scanIndex > 0 && intensityMap[frameIndex][scanIndex] > threshold)
                    {
                        MSPeak peak = new MSPeak();
                        peak.Height = intensityMap[frameIndex][scanIndex];
                        MSPeakResult msPeak = new MSPeakResult(peakId++, frameNumber, scanNumber, peak);
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
                    scanIndex = (ushort) (startScanInMap + 1);
                    scanNumber = (ushort) (startScan + 1);
                    //go right to determine the next value that's below the threshold
                    while (scanIndex < end && intensityMap[frameIndex][scanIndex] > threshold )
                    {
                        MSPeak peak = new MSPeak();
                        peak.Height = intensityMap[frameIndex][scanIndex];
                        MSPeakResult msPeak = new MSPeakResult(peakId++, frameNumber, scanNumber, peak);
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
                    else
                    {
                        scanNumberList.Sort();
                        frameAndScanNumbers.Add(frameNumber, scanNumberList);
                        totalSummed += (ushort) scanNumberList.Count;
                    }

                    frameNumber--;
                }

                //go down
                frameIndex = (ushort)(startFrameInMap + 1);
                scanIndex = startScanInMap;
                scanNumber = startScan;
                frameNumber = (ushort)(startFrame + 1);

                while (frameIndex < intensityMap.Length - 1 && intensityMap[frameIndex][startScanInMap] >= threshold )
                {
                    //processing frame
                    // Console.WriteLine("processing frame " + frameIndex);
                    List<ushort> scanNumberList = new List<ushort>();
                    

                    int end = intensityMap[frameIndex].Length;
                    scanIndex = startScanInMap;
                    scanNumber = startScan;
                    //go left to determine the first value taht's below the threshold
                    while (scanIndex > 0 && intensityMap[frameIndex][scanIndex] > threshold )
                    {
                        MSPeak peak = new MSPeak();
                        peak.Height = intensityMap[frameIndex][scanIndex];
                        MSPeakResult msPeak = new MSPeakResult(peakId++, frameNumber, scanNumber, peak);
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
                    scanIndex = (ushort) (startScanInMap+1);
                    scanNumber = (ushort) (startScan+1);
                    //go right to determine the next value that's below the threshold
                    while (scanIndex < end && intensityMap[frameIndex][scanIndex] > threshold )
                    {
                        MSPeak peak = new MSPeak();
                        peak.Height = intensityMap[frameIndex][scanIndex];
                        MSPeakResult msPeak = new MSPeakResult(peakId++, frameNumber, scanNumber, peak);
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
                    if (scanNumberList.Count< 3)
                    {
                        break;
                    }
                    else
                    {
                        scanNumberList.Sort();
                        frameAndScanNumbers.Add(frameNumber, scanNumberList);
                        totalSummed += (ushort)scanNumberList.Count;
                    }
                    frameNumber++;
                }

            

            return peaksForCurveFitting;

        }

         public Bitmap getBitMapFromIntensityMap(int [][] intensities, int maxIntensity, int frames, int scans, float threshold)
        {

            //the x dimension for intensities here is SCANS and the y dimension is FRAMES
            Bitmap imageFile = new Bitmap(frames, scans);


            //go through the frames first
            for (int i = 0; i < frames; i++)
            {
                for (int j = 0; j < scans; j++)
                {
                    if (intensities[i][j] == 0)
                    {
                        imageFile.SetPixel(i, j, Color.Blue);
                    }
                    else
                    {
                        float intensity = (float)intensities[i][j] / maxIntensity;

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

        public Color getColorFroomIntensity (int colorAsInt) {
            int red = (int)((colorAsInt >> 0x18) & 0xff);
            Console.WriteLine(red);
            int green = (int)((colorAsInt >> 0x10) & 0xff);
            Console.WriteLine(green);
            int blue = (int)(colorAsInt & 0xff);
            Console.WriteLine(blue);
            return Color.FromArgb((byte)((colorAsInt >> 0x18) & 0xff), 
                          (byte)((colorAsInt >> 0x10) & 0xff), 
                          (byte)((colorAsInt >> 8) & 0xff), 
                          (byte)(colorAsInt & 0xff));
        }


        public Color getRGB(float intensity)
        {
            float interp;
            int red=0, green=0, blue=0;

            for (int i = 1; i < this.color_blend.Positions.Length; i++)
            {
                if (((float)1.0 - intensity) <= this.color_blend.Positions[i])
                {
                    interp = (((float)1.0 - intensity) - this.color_blend.Positions[i - 1]) / (this.color_blend.Positions[i] - this.color_blend.Positions[i - 1]);

                    red = (int)(((float)(this.color_blend.Colors[i].R - this.color_blend.Colors[i - 1].R)) * interp) + this.color_blend.Colors[i - 1].R;
                    green = (int)(((float)(this.color_blend.Colors[i].G - this.color_blend.Colors[i - 1].G) * interp)) + this.color_blend.Colors[i - 1].G;
                    blue = (int)(((float)(this.color_blend.Colors[i].B - this.color_blend.Colors[i - 1].B) * interp)) + this.color_blend.Colors[i - 1].B;
                    break;
                }
            }

            Color c;

            try
            {
                c = Color.FromArgb(red, green, blue);
            }
            catch (Exception e)
            {
                c = Color.Blue;
            }

            return c;
            

        }

    }


}
