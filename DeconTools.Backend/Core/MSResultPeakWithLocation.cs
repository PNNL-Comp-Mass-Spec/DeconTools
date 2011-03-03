using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Data.Structures;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.DTO
{
    public class MSResultPeakWithLocation : MSPeakResult
    {
        private BitArray2D frameScansRange;

        public BitArray2D FrameScansRange
        {
            get { return frameScansRange; }

            set { frameScansRange = value; }
        }

        public MSResultPeakWithLocation(MSPeakResult peak)
        {
            this.MSPeak = peak.MSPeak;

        }

        public MSResultPeakWithLocation(MSPeakResult peak, int numFrames, int numScans)
        {
            this.MSPeak = peak.MSPeak;
            frameScansRange = new BitArray2D(numScans, numFrames);
        }

        public MSResultPeakWithLocation(MSPeak peak, BitArray2D framesAndScans)
        {
            this.MSPeak = peak;
            this.frameScansRange = framesAndScans;
        }

        public override int CompareTo(object obj)
        {
            IPeak secondPeak = obj as IPeak;
            if (secondPeak == null)
            {
                return -1;
            }
            else
            {

                   //we need a system level global parameter that is the tolerance in PPM
                   //TODO
                    double toleranceInPPM = 20;
                    double differenceInPPM = Math.Abs(1000000 * (secondPeak.XValue - this.MSPeak.XValue) / this.MSPeak.XValue);

                    if (differenceInPPM <= toleranceInPPM)
                    {
                        return 0;
                    }
                    else
                    {
                        return this.MSPeak.XValue.CompareTo(secondPeak.XValue);
                    }

            }

        }

        public void updateFrameScansRange(BitArray2D frameScansFound)
        {
            

        }


        //checks if the given mass is within a tolerance of this feature
        public bool containsMass(double massValue, int toleranceInPPM)
        {
                double differenceInPPM = Math.Abs(1000000 * (this.MSPeak.XValue - massValue) / this.MSPeak.XValue);

                if (differenceInPPM <= toleranceInPPM)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            
        }


        public int containsPeak(MSPeak peak, int frameNum, int scanNum, int toleranceInPPM, int netRange, int driftRange )
        {
            if (peak == null)
            {
                return -1;
            }
            else
            {

                //TODO:: two peaks are the same if they are within a tolerance of each other in
                //terms of mz, scan and lc frame. in this case we're only implementing mz values
                //
                double differenceInPPM = Math.Abs(1000000 * (peak.XValue - this.XValue) / this.XValue);

                if (differenceInPPM <= toleranceInPPM)
                {
                    //it's within the mass tolerance of our peak
                    //now check for net tolerance
                    //check if the frameScansRange bit is set for the frame number and a few other scans within tolerance
                    for (int i = frameNum - netRange; i < frameNum + netRange; i++)
                    {
                        for (int j = scanNum - driftRange; j < scanNum + driftRange; j++)
                        {
                            if (frameScansRange[j, i] == true)
                            {
                                //that means it's within the net and drift time tolerances of the current peak
                                return 0;
                            }

                        }
                    }

                    int netDiff = this.Frame_num.CompareTo(frameNum);
                    if ( netDiff  == 0 ){
                        //now compare on scan range
                        return this.Scan_num.CompareTo( frameNum);
                    }
                    else{
                        return netDiff;
                    }
                }
                else
                {
                    return this.XValue.CompareTo(peak.XValue);
                }
            }
           }
    }
}
