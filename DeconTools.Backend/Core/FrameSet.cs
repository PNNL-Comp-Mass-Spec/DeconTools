using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public sealed class FrameSet
    {
        public FrameSet()
        {
            FramePressure = Single.NaN;
            AvgTOFLength = Single.NaN;
        }

        public FrameSet(int primaryFrame, List<int> frameArray)
            : this()
        {
            PrimaryFrame = primaryFrame;
            IndexValues = frameArray;
        }

        public FrameSet(int primaryFrame)
            : this()
        {
            PrimaryFrame = primaryFrame;
            IndexValues = new List<int> {
                primaryFrame
            };
        }

        public FrameSet(int primaryFrame, int[] indexArray)
            : this()
        {
            PrimaryFrame = primaryFrame;
            IndexValues = new List<int>();

            foreach (var indexItem in indexArray)
            {
                IndexValues.Add(indexItem);
            }
        }

        public FrameSet(int primaryFrame, int lowerFrame, int upperFrame)
            : this()
        {
            PrimaryFrame = primaryFrame;
            Check.Require(lowerFrame <= upperFrame, "Lower frame number must be less than or equal to the frame scan number");
            IndexValues = new List<int>();

            for (var i = lowerFrame; i <= upperFrame; i++)
            {
                IndexValues.Add(i);
            }
        }

        public void AddFrame(int frameNumber)
        {
            if (IndexValues != null)
            {
                IndexValues.Add(frameNumber);
            }
            else
            {
                IndexValues = new List<int> {
                    frameNumber
                };
            }
        }

        public int PrimaryFrame { get; set; }

        public bool IsContiguous { get; set; } = true;

        public double FramePressure { get; set; }
        public double AvgTOFLength { get; set; }

        public List<int> IndexValues { get; set; }

        internal int GetLowestFrameNumber()
        {
            var lowVal = int.MaxValue;

            foreach (var value in IndexValues)
            {
                if (value < lowVal) lowVal = value;
            }
            return lowVal;
        }

        internal int GetHighestFrameNumber()
        {
            var highVal = -1;
            foreach (var value in IndexValues)
            {
                if (value > highVal) highVal = value;
            }
            return highVal;
        }

        internal int Count()
        {
            if (IndexValues == null || IndexValues.Count == 0) return 0;
            return IndexValues.Count;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var item in IndexValues)
            {
                sb.Append(item);
                sb.Append(",");
            }

            return sb.ToString().TrimEnd(',');
        }
    }
}
