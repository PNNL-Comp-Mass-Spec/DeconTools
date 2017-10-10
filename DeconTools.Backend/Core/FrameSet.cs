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
            this.primaryFrame = primaryFrame;
            IndexValues = frameArray;
        }



        public FrameSet(int primaryFrame)
            : this()
        {
            this.primaryFrame = primaryFrame;
            IndexValues = new List<int>();
            indexValues.Add(primaryFrame);

        }

        public FrameSet(int primaryFrame, int[] indexArray)
            : this()
        {
            this.primaryFrame = primaryFrame;
            IndexValues = new List<int>();

            foreach (var indexItem in indexArray)
            {
                IndexValues.Add(indexItem);
            }

        }

        public FrameSet(int primaryFrame, int lowerFrame, int upperFrame)
            : this()
        {
            this.primaryFrame = primaryFrame;
            Check.Require(lowerFrame <= upperFrame, "Lower frame number must be less than or equal to the frame scan number");
            IndexValues = new List<int>();

            for (var i = lowerFrame; i <= upperFrame; i++)
            {
                IndexValues.Add(i);
            }

        }

        public void AddFrame(int frameNumber)
        {
            if (indexValues != null)
            {
                indexValues.Add(frameNumber);
            }
            else
            {
                indexValues = new List<int> {
                    frameNumber
                };
            }
        }

        private int primaryFrame;

        public int PrimaryFrame
        {
            get => primaryFrame;
            set => primaryFrame = value;
        }
        private bool isContiguous = true;

        public bool IsContiguous
        {
            get => isContiguous;
            set => isContiguous = value;
        }


        public double FramePressure { get; set; }
        public double AvgTOFLength { get; set; }

        private List<int> indexValues;

        public List<int> IndexValues
        {
            get => indexValues;
            set => indexValues = value;
        }


        internal int getLowestFrameNumber()
        {
            var lowVal = int.MaxValue;

            for (var i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] < lowVal) lowVal = indexValues[i];

            }
            return lowVal;
        }

        internal int getHighestFrameNumber()
        {
            var highVal = -1;
            for (var i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] > highVal) highVal = indexValues[i];
            }
            return highVal;
        }



        internal int Count()
        {
            if (indexValues == null || indexValues.Count == 0) return 0;
            return indexValues.Count;
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
