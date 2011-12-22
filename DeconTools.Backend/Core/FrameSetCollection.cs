using System;
using System.Collections.Generic;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class FrameSetCollection
    {
        public FrameSetCollection()
        {
            FrameSetList = new List<FrameSet>();
        }


        public static FrameSetCollection Create(UIMFRun uimfRun, int numFramesSummed, int increment)
        {

            int minFrame = uimfRun.GetMinPossibleFrameNumber();
            int maxFrame = uimfRun.GetMaxPossibleFrameNumber();

            return Create(uimfRun, minFrame, maxFrame, numFramesSummed, increment);
        }


        public static FrameSetCollection Create(UIMFRun uimfRun, int startFrame, int stopFrame, int numFramesSummed, int increment)
        {
            bool numFramesIsOdd = (numFramesSummed % 2 == 1 && numFramesSummed > 0);
            Check.Require(uimfRun != null, "Run is null");
            Check.Require(numFramesIsOdd, "Number of frames summed together must be an odd number");
            Check.Require(startFrame <= stopFrame, "Stop frame must be greater than or equal to the Start frame");

            int minFrame = uimfRun.GetMinPossibleFrameNumber();
            int maxFrame = uimfRun.GetMaxPossibleFrameNumber();

            Check.Require(startFrame >= minFrame, "Start frame must be greater than or equal to " + minFrame);
            
            Check.Require(increment > 0, "Increment must be greater than 0");
            
            if (uimfRun.ContainsMSMSData)
            {
                Check.Require(numFramesSummed==1, "DeconTools currently does not support summing across LC dimension (i.e. frames) when file contains MS2-level data");
            }

            var frameSetCollection = new FrameSetCollection();

            if (stopFrame > maxFrame) stopFrame = maxFrame;

            for (int i = startFrame; i <= stopFrame; i = i + increment)
            {
                int lowerFrame = i - ((numFramesSummed - 1)/2);
                if (lowerFrame < minFrame)
                {
                    lowerFrame = minFrame; //no bounce effect... 
                    if (lowerFrame > maxFrame) lowerFrame = maxFrame; //this will be a very rare condition
                }


                int upperFrame = i + ((numFramesSummed - 1)/2);
                if (upperFrame > maxFrame)
                {
                    upperFrame = upperFrame - (upperFrame - maxFrame);
                    if (upperFrame < 0) upperFrame = 0; // rare condition
                }

                var frameSet = new FrameSet(i, lowerFrame, upperFrame);
                frameSetCollection.FrameSetList.Add(frameSet);


                
            }

            return frameSetCollection;
        }


        public List<FrameSet> FrameSetList { get; set; }


        public FrameSet GetFrameSet(int primaryNum)
        {
            if (FrameSetList == null || FrameSetList.Count == 0) return null;

            return (this.FrameSetList.Find(p => p.PrimaryFrame == primaryNum));
        }
    }
}
