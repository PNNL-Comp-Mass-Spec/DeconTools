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
            this.ScanMaxIntensity = -1;
            this.RetentionTime = 0;
            this.Intensity = 0;
            
        }
        
        #endregion

        #region Properties

        public int ID { get; set; }
        public ChromPeak ChromPeak { get; set; }
        public List<MSPeakResult> PeakList { get; set; }




        public float RetentionTime { get; set; }

        public float Intensity { get; set; }

        public int ScanStart { get; set; }

        public int ScanEnd { get; set; }

        public int ScanMaxIntensity { get; set; } 
        
        public ScanSet ScanSet {get;set;}
        

        
        #endregion


        public MSPeakResult GetMSPeakResultRepresentative()
        {
            if (this.PeakList == null || this.PeakList.Count == 0) return null;
            else
            {
                return this.PeakList[0];
            }
        }



    }
}
