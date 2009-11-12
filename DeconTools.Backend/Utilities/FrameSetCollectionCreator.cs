using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Utilities
{
    public class FrameSetCollectionCreator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="run"></param>
        /// <param name="start">Start frame</param>
        /// <param name="stop">Stop frame</param>
        /// <param name="range">Range of Frames that makes up each FrameSet created. Must be an odd number.
        /// eg) A FrameSet with primary Frame of 5 and a range of 3, 
        /// will consist of Frames 4,5,6</param>
        /// <param name="increment">Frame increment. Must be greater than 0. This will inform the Creator 
        /// which primary Frames will be created. eg)  If starting at Frame 3. The first Frameset will 
        /// have a Primary frame of 3. If the increment is 2. The next frameset will have an Primary Frame of 5. 
        /// This is a way of controlling overlap between FrameSets.</param>
        public FrameSetCollectionCreator(Run run, int start, int stop, int range, int increment)
        {
            this.run = run;
            this.startFrame = start;
            this.stopFrame = stop;
            this.range = range;
            this.increment = increment;
        }


        public FrameSetCollectionCreator(Run run, int range, int increment)
            : this(run, getFirstFrame(run), getLastFrame(run), range, increment)
        {

        }

        private static int getLastFrame(Run run)
        {
            Check.Require(run != null, "Run is null");
            Check.Require(run is UIMFRun, "FrameSet Collections can only be created for UIMF files");

            UIMFRun uimfRun = (UIMFRun)run;
            int totalFrames = uimfRun.GetNumFrames();

            if (totalFrames > 0) return (totalFrames - 1);
            else
            {
                return 1;     //minimum frame value
            }

        }

        private static int getFirstFrame(Run run)
        {
            return 1;        //minimum frame
        }


        private int startFrame;
        private int stopFrame;
        private Run run;
        private int range;
        private int increment;


        public void Create()
        {
            bool isRangeOdd = (range % 2 == 1 && range > 0);

            Check.Require(run != null, "Run is null");
            Check.Require(run is UIMFRun, "FrameSet Collections can only be created for UIMF files");
            Check.Require(startFrame <= stopFrame, "Stop frame must be greater than or equal to the Start frame");
            Check.Require(startFrame > 0, "Start frame must be greater than 0");
            Check.Require(isRangeOdd, "Range value must be an odd number");
            Check.Require(increment > 0, "Increment must be greater than 0");


            UIMFRun uimfRun = (UIMFRun)run;

            int maxFrame = uimfRun.GetNumFrames() - 1;

            if (stopFrame > maxFrame) stopFrame = maxFrame;


            uimfRun.FrameSetCollection = new FrameSetCollection();

            int minFrame = 1;

            for (int i = startFrame; i <= stopFrame; i = i + increment)
            {
                int lowerFrame = i - ((range - 1) / 2);
                if (lowerFrame < minFrame)
                {
                    lowerFrame = minFrame;       //no bounce effect... 
                    if (lowerFrame > maxFrame) lowerFrame = maxFrame;    //this will be a very rare condition
                }


                int upperFrame = i + ((range - 1) / 2);
                if (upperFrame > maxFrame)
                {
                    upperFrame = upperFrame - (upperFrame - maxFrame);
                    if (upperFrame < 0) upperFrame = 0;         // rare condition
                }



                FrameSet frameSet = new FrameSet(i, lowerFrame, upperFrame);
                uimfRun.FrameSetCollection.FrameSetList.Add(frameSet);
            }

        }



    }
}
