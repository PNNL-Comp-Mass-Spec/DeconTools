using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMCS_0437_AgilentTOFFileReadingProblem
    {
        [Test]
        public void Test1()
        {
            string testfile = @"\\protoapps\UserData\Nikola\DDD_Milk\D6.1.forExpRepAnal_3.14.2012.d";

            Run run = new RunFactory().CreateRun(testfile);

            TestUtilities.DisplayRunInformation(run);



        }

    }
}
