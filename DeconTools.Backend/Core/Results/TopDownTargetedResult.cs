using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core.Results
{
    public class TopDownTargetedResult : TargetedResultBase
    {
        public TopDownTargetedResult() : base() { }

        public TopDownTargetedResult(TargetBase target) : base(target) { }
    }
}
