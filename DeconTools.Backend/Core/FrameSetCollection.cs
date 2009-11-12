using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class FrameSetCollection
    {
        public FrameSetCollection()
        {
            this.frameSetList = new List<FrameSet>();
        }
        
        private List<FrameSet> frameSetList;

        public List<FrameSet> FrameSetList
        {
            get { return frameSetList; }
            set { frameSetList = value; }
        }


        public FrameSet GetFrameSet(int primaryNum)
        {
            if (this.frameSetList == null || this.frameSetList.Count == 0) return null;

            return (this.frameSetList.Find(p => p.PrimaryFrame == primaryNum));
        }
    }
}
