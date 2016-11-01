using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqTargetMSAlign:IqTarget
    {
        public IqTargetMSAlign ()
        {
            ChargeState = new List<int>();
            ScanList = new List<int>();
        }

        public IqTargetMSAlign(IqWorkflow workflow) : base(workflow)
        {
            ChargeState = new List<int>();
            ScanList = new List<int>();
        }

        public List<int> ChargeState;
        public List<int> ScanList;
    }
}
