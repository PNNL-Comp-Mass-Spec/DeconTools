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
    public class OMCS_0592_AgilentD_readingProblem
    {
        [Test]
        public void Test1()
        {
            string testfile = @"C:\Users\d3x720\Downloads\Bioheparin test_Fabio.d";

            Run run = new RunFactory().CreateRun(testfile);

        }

        [Test]
        public void Test2()
        {
            string testfile = @"C:\Users\d3x720\Downloads\Bioheparin test_Fabio.d\Bioheparin test.mzML";

            Run run = new RunFactory().CreateRun(testfile);

            Console.WriteLine(TestUtilities.DisplayRunInformation(run));

        }


    }
}
