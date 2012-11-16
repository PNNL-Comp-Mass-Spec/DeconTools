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

            int minFrame = uimfRun.GetMinPossibleLCScanNum();
            int maxFrame = uimfRun.GetMaxPossibleLCScanNum();

            return Create(uimfRun, minFrame, maxFrame, numFramesSummed, increment,processMSMS);
        }


        public static FrameSetCollection Create(UIMFRun uimfRun, int startFrame, int stopFrame, int numFramesSummed, int increment, bool processMSMS=false)
        {
            bool numFramesIsOdd = (numFramesSummed % 2 == 1 && numFramesSummed > 0);
            Check.Require(uimfRun != null, "Run is null");
            Check.Require(numFramesIsOdd, "Number of frames summed together must be an odd number");
            Check.Require(startFrame <= stopFrame, "Stop frame must be greater than or equal to the Start frame");

            int minFrame = uimfRun.GetMinPossibleLCScanNum();
            int maxFrame = uimfRun.GetMaxPossibleLCScanNum();

            Check.Require(startFrame >= minFrame, "Start frame must be greater than or equal to " + minFrame);
            
            Check.Require(increment > 0, "Increment must be greater than 0");
            
            var frameSetCollection = new FrameSetCollection();

			List<int> ms1Frames = uimfRun.MS1Frames;
			List<int> ms2Frames = uimfRun.MS2Frames;
			int numOfConsecutiveMs2Frames = uimfRun.GetNumberOfConsecutiveMs2Frames();

            if (stopFrame > maxFrame) stopFrame = maxFrame;

            for (int frame = startFrame; frame <= stopFrame; frame = frame + increment)
            {
                int currentMSLevel = uimfRun.GetMSLevel(frame);

				int numberOfFrameIndexesToSkip = 1;
				List<int> currentFrames = ms1Frames;

				// Handle MS/MS data
				if(currentMSLevel == 2)
				{
					// If we do not want to process MS2 frames, then just skip this frame
					if (!processMSMS) continue;

					// Set the number of frames to skip to be the number of consecutive MS2 frames per MS1 frame
					numberOfFrameIndexesToSkip = numOfConsecutiveMs2Frames;

					// Use MS2 frames instead of MS1 frames
					currentFrames = ms2Frames;
				}

				int indexOfCurrentFrame = currentFrames.IndexOf(frame);

				// If we could not find the frame number, then it is a frame we want to ignore. For example, this would happen for Calibration frames.
				if (indexOfCurrentFrame < 0) continue;

				int lowerIndex = indexOfCurrentFrame - numberOfFrameIndexesToSkip;
				int upperIndex = indexOfCurrentFrame + numberOfFrameIndexesToSkip;

				List<int> framesToSum = new List<int>();
				int numLowerFramesToGet = (numFramesSummed - 1) / 2;
				int numUpperFramesToGet = (numFramesSummed - 1) / 2;

				//get lower frames
				int framesCounter = 0;
				while (lowerIndex >= 0 && numLowerFramesToGet > framesCounter)
				{
					framesToSum.Insert(0, currentFrames[lowerIndex]);
					lowerIndex -= numberOfFrameIndexesToSkip;
					framesCounter++;
				}

				//get middle frame
				framesToSum.Add(frame);

				//get upper frames
				framesCounter = 0;
				int maxPossibleFrameIndex = currentFrames.Count - 1;
				while (upperIndex <= maxPossibleFrameIndex && numUpperFramesToGet > framesCounter)
				{
					framesToSum.Add(currentFrames[upperIndex]);
					upperIndex += numberOfFrameIndexesToSkip;
					framesCounter++;
				}

				var frameset = new FrameSet(frame, framesToSum.ToArray());
				frameSetCollection.FrameSetList.Add(frameset);
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
