using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Core
{
    public class ElutingPeak
    {
        #region Constructors

        public ElutingPeak()
        {
            PeakList = new List<MSPeakResult>();
            this.ID = -1;
            this.ScanStart = -1;
            this.ScanEnd = -1;
            this.RetentionTime = 0;
            this.Intensity = 0;
        }
        
        #endregion

        #region Properties
        public List<MSPeakResult> PeakList { get; set; }

        public int ID { get; set; }

        public float RetentionTime { get; set; }

        public float Intensity { get; set; }

        public int ScanStart { get; set; }

        public int ScanEnd { get; set; } 
        #endregion


    }
}
