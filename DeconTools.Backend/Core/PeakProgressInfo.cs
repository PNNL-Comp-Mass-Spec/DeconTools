using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    /// <summary>
    /// Contains information that is usually passed along to user interface
    /// via a background worker. This class represents a light class
    /// so that heavy objects (eg Workflow) are not passed. 
    /// </summary>
    public class PeakProgressInfo
    {
        public string ProgressInfoString { get; set; }
    }
}
