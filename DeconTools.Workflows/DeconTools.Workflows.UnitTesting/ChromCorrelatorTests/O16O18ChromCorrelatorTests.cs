using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.ChromCorrelatorTests
{
    [TestFixture]
    public class O16O18ChromCorrelatorTests
    {

        [Test]
        public void ChromCorrelatorTest1()
        {
            O16O18ChromCorrelator chromCorrelator =new O16O18ChromCorrelator(7,0.05,10,Globals.ToleranceUnit.PPM);

            
        }

    }
}
