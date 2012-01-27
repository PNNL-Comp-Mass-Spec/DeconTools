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


        public static FrameSetCollection Create(UIMFRun uimfRun, int numFramesSummed, int increment, bool processMSMS=false)
        {

            int minFrame = uimfRun.GetMinPossibleFrameNumber();
            int maxFrame = uimfRun.GetMaxPossibleFrameNumber();

            return Create(uimfRun, minFrame, maxFrame, numFramesSummed, increment,processMSMS);
        }


        public static FrameSetCollection Create(UIMFRun uimfRun, int startFrame, int stopFrame, int numFramesSummed, int increment, bool processMSMS=false)
        {
            bool numFramesIsOdd = (numFramesSummed % 2 == 1 && numFramesSummed > 0);
            Check.Require(uimfRun != null, "Run is null");
            Check.Require(numFramesIsOdd, "Number of frames summed together must be an odd number");
            Check.Require(startFrame <= stopFrame, "Stop frame must be greater than or equal to the Start frame");

            int minFrame = uimfRun.GetMinPossibleFrameNumber();
            int maxFrame = uimfRun.GetMaxPossibleFrameNumber();

            Check.Require(startFrame >= minFrame, "Start frame must be greater than or equal to " + minFrame);
            
            Check.Require(increment > 0, "Increment must be greater than 0");
            
            var frameSetCollection = new FrameSetCollection();

            if (stopFrame > maxFrame) stopFrame = maxFrame;

            for (int frame = startFrame; frame <= stopFrame; frame = frame + increment)
            {

                int currentMSLevel = uimfRun.GetMSLevel(frame);


                int indexOfCurrentFrame = uimfRun.MS1Frames.IndexOf(frame);
                
                int lowerIndex = indexOfCurrentFrame - 1;
                int upperIndex = indexOfCurrentFrame + 1;

                List<int> framesToSum = new List<int>();
                int numLowerFramesToGet = (numFramesSummed - 1)/2;
                int numUpperFramesToGet = (numFramesSummed - 1)/2;

                //get lower frames. Note that only MS1 frames can be summed
                int framesCounter = 0;
                while (lowerIndex>=0 && numLowerFramesToGet>framesCounter && currentMSLevel==1)
                {
                    framesToSum.Insert(0, uimfRun.MS1Frames[lowerIndex]);
                    lowerIndex--;
                    framesCounter++;
                }

                //get middle frame   note that frameTypes MS1 and MS2 can be added here
                framesToSum.Add(frame);
                
                

                //get upper frames   Note that only MS1 frames can be summed
                framesCounter = 0;
                int maxPossibleFrameIndex = uimfRun.MS1Frames.Count-1;
                while (upperIndex <= maxPossibleFrameIndex && numUpperFramesToGet > framesCounter && currentMSLevel == 1)
                {
                    framesToSum.Add(uimfRun.MS1Frames[upperIndex]);
                    framesCounter++;
                    upperIndex++;
                }


                var frameset = new FrameSet(frame, framesToSum.ToArray());
                if (currentMSLevel==1)
                {
                    frameSetCollection.FrameSetList.Add(frameset); 
                }
                else if (currentMSLevel==2 && processMSMS)
                {
                    frameSetCollection.FrameSetList.Add(frameset);
                }
                
                

               


                
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
