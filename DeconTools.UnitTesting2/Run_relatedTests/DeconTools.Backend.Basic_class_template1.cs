using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    public class DeconTools
    {
        [Test]
        public void readXMLFile()
        {
            Run run=new MZXMLRun(@"\\protoapps\DataPkgs\Public\2010\184_GMAX_Mass_Tag_PRIDE_Submission\Aggregation\PZX201009141632_Auto623288\Gmax0001_run1_8Apr09_Draco_09-01-05_grouped.mzxml");
            
            Console.WriteLine(run.MaxScan);


        }

    }
}
