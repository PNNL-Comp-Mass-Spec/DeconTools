using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Core
{
    public class ScanSetFactory
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public ScanSet CreateScanSet(Run run, int scan, int scansSummed)
        {
            int currentLevel = run.GetMSLevel(scan);

            List<int> lowerScansToSum = getLowerScans(run, scan, currentLevel, (scansSummed - 1) / 2);
            List<int> upperScansToSum = getUpperScans(run, scan, currentLevel, (scansSummed - 1) / 2);
            
            List<int> scansToSum = lowerScansToSum;
            scansToSum.Add(scan);
            scansToSum.AddRange(upperScansToSum);
            //scansToSum.Sort();

            return new ScanSet(scan, scansToSum.ToArray());

        }

        public FrameSet CreateFrameSet(Run run, int frame, int framesSummed)
        {
            Check.Require(run is UIMFRun, "Cannot create frameset on a Run other than a UIMFRun.");

            List<int> lowerFramesToSum = getLowerFrames(run, frame, (framesSummed - 1) / 2);
            List<int> upperFramesToSum = getUpperFrames(run, frame, (framesSummed - 1) / 2);
            List<int> framesToSum = lowerFramesToSum;
            framesToSum.Add(frame);
            framesToSum.AddRange(upperFramesToSum);
            return new FrameSet(frame, framesToSum.ToArray());

        }

        private List<int> getLowerFrames(Run run, int frame, int numLowerScansToGet)
        {
            int currentFrame = frame - 1;
            List<int> lowerFrames = new List<int>();

            int framesCounter = 0;
            while (currentFrame >= 1 && numLowerScansToGet > framesCounter)
            {
                lowerFrames.Add(currentFrame);
                framesCounter++;
                currentFrame--;
            }

            return lowerFrames;
        }

        private List<int> getUpperFrames(Run run, int frame, int numUpperFramesToGet)
        {
            int currentFrame = frame + 1;
            List<int> frames = new List<int>();

            int framecounter = 0;
            int frameUpperLimit = ((UIMFRun)run).GetNumFrames();

            while (currentFrame <= frameUpperLimit && numUpperFramesToGet > framecounter)
            {
                frames.Add(currentFrame);
                framecounter++;
                currentFrame++;
            }
            return frames;
        }

        #endregion



        private List<int> getLowerScans(Run run, int startingScan, int currentMSLevel, int numLowerScansToGet)
        {
            int currentScan = startingScan - 1;
            List<int> lowerScans = new List<int>();

            int scansCounter = 0;
            while (currentScan >= 1 && numLowerScansToGet > scansCounter)
            {
                if (run.GetMSLevel(currentScan) == currentMSLevel)
                {
                    lowerScans.Add(currentScan);
                    scansCounter++;
                }
                currentScan--;


            }
            return lowerScans;

        }
        private List<int> getUpperScans(Run run, int startingScan, int currentMSLevel, int numUpperScansToGet)
        {
            int currentScan = startingScan + 1;
            List<int> scans = new List<int>();

            int scansCounter = 0;
            int scanUppperLimit = run.GetNumMSScans();

            while (currentScan <= scanUppperLimit && numUpperScansToGet > scansCounter)
            {
                if (run.GetMSLevel(currentScan) == currentMSLevel)
                {
                    scans.Add(currentScan);
                    scansCounter++;
                }
                currentScan++;


            }
            return scans;

        }


        #region Private Methods
        #endregion
    }
}
