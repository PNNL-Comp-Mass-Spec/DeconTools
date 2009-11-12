using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class FrameSet
    {

        public FrameSet(int primaryFrame)
        {
            this.primaryFrame = primaryFrame;
            this.IndexValues = new List<int>();
            this.indexValues.Add(primaryFrame);

        }

        public FrameSet(int primaryFrame, int[] indexArray)
        {
            this.primaryFrame = primaryFrame;
            this.IndexValues = new List<int>();

            foreach (int indexItem in indexArray)
            {
                this.IndexValues.Add(indexItem);
            }

        }

        public FrameSet(int primaryFrame, int lowerFrame, int upperFrame)
        {
            this.primaryFrame = primaryFrame;
            Check.Require(lowerFrame <= upperFrame, "Lower frame number must be less than or equal to the frame scan number");
            this.IndexValues = new List<int>();

            for (int i = lowerFrame; i <= upperFrame; i++)
            {
                this.IndexValues.Add(i);
            }

        }

        private int primaryFrame;

        public int PrimaryFrame
        {
            get { return primaryFrame; }
            set { primaryFrame = value; }
        }

        private List<int> indexValues;

        public virtual List<int> IndexValues
        {
            get { return indexValues; }
            set { indexValues = value; }
        }


        internal virtual int getLowestFrameNumber()
        {
            int lowVal = int.MaxValue;

            for (int i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] < lowVal) lowVal = indexValues[i];

            }
            return lowVal;
        }

        internal int getHighestFrameNumber()
        {
            int highVal = -1;
            for (int i = 0; i < IndexValues.Count; i++)
            {
                if (IndexValues[i] > highVal) highVal = indexValues[i];
            }
            return highVal;
        }



        internal int Count()
        {
            if (this.indexValues == null || this.indexValues.Count == 0) return 0;
            return this.indexValues.Count;
        }
    }



}
