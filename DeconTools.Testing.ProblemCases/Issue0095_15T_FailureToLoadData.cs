using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class Issue0095_15T_FailureToLoadData
    {
        //Resolved:  This seemed to be a file permissions issue


        [Test]
        public void thisTestFailed_unauthorizedAccessError()
        {
            string testFile = @"\\proto-3\15T_DMS1\CCR5_DEGLY_1_3_01_635";

            BrukerV3Run run = new BrukerV3Run(testFile);

            ScanSet scanSet = new ScanSet(2);
            run.GetMassSpectrum(scanSet);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
    
        }


        [Test]
        public void accessLocalFile_test1()
        {
            string testFile = @"D:\Data\Redmine_issues\Issue0095_15T_FailureToLoadData\CCR5_DEGLY_1_3_01_635";

            BrukerV3Run run = new BrukerV3Run(testFile);

            ScanSet scanSet = new ScanSet(2);
            run.GetMassSpectrum(scanSet);
            Assert.That(run.XYData.Xvalues != null);
            Assert.That(run.XYData.Xvalues.Length > 0);
            Assert.AreEqual(195733, run.XYData.Xvalues.Length);
        }

    }
}
