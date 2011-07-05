using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class ScanNETPair
    {
        public ScanNETPair(float scan, float net)
        {
            Scan = scan;
            NET = net;
        }

        internal float Scan;
        internal float NET;

    }
}
