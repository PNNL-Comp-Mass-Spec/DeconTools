using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Core.Results
{
    public class DeuteratedTargetedResultObject : TargetedResultBase
    {
        #region Constructors
        public DeuteratedTargetedResultObject() : base() { }

        public DeuteratedTargetedResultObject(TargetBase target) : base(target) { }
        #endregion

        #region Properties

        public double RatioDH { get; set; }
        public double IntensityI0HydrogenMono { get; set; }
        public double HydrogenI0 { get; set; }
        public double HydrogenI1 { get; set; }
        public double HydrogenI2 { get; set; }
        public double HydrogenI3 { get; set; }
        public double HydrogenI4 { get; set; }
        
        public double DeuteriumI0 { get; set; }
        public double DeuteriumI1 { get; set; }
        public double DeuteriumI2 { get; set; }
        public double DeuteriumI3 { get; set; }
        public double DeuteriumI4 { get; set; }
        
        public double TheoryI0 { get; set; }
        public double TheoryI1 { get; set; }
        public double TheoryI2 { get; set; }
        public double TheoryI3 { get; set; }
        public double TheoryI4 { get; set; }
        
        public double RawI0 { get; set; }
        public double RawI1 { get; set; }
        public double RawI2 { get; set; }
        public double RawI3 { get; set; }
        public double RawI4 { get; set; }
        public double LabelingEfficiency { get; set; }

        #endregion

       
    }
}
