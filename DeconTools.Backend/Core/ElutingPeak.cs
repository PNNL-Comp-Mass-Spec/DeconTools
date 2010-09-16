using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Core
{
    public class ElutingPeak
    {
        public List<MSPeakResult> PeakList { get; set; } //TODO:  SK ElutingPeak class added 9-16-10

        public int ID { get; set; }

        public float RetentionTime { get; set; }

        public float intensity { get; set; }

        public int ScanStart { get; set; }

        public int ScanEnd { get; set; }
    }
}
