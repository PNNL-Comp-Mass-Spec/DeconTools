using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class TicketXX_Kronwitter_notReadingVelosOrbitrap
    {
        /// <summary>
        /// background... see email from S. Kronwitter on Aug 6, 2010. He couldn't open a file using DeconTools_Autoprocessor
        /// ... he was using an old copy.... and trying it on Windows 7 64bit.  Below I show that I can indeed open it and read data. 
        /// </summary>
        [Test]
        public void readSpectraTest1()
        {
            string testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\SKrone_glycopeptide.raw";
            //string testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            Run run = new XCaliburRun(testFile);
            int numScans = run.GetNumMSScans();

            ScanSet scan = new ScanSet(numScans / 2);
            run.GetMassSpectrum(scan, 0, 100000);

            run.XYData.Display();
        }


      
    }
}
