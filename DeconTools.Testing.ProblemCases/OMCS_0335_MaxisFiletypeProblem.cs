using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    public class OMCS_0335_MaxisFiletypeProblem
    {

        [Test]
        public void tryCreateRun()
        {
            string path = @"\\proto-5\BionetXfer\People\Robby\maxis_data\2012_05_15_MN9_A_000001.d";

            var run = new RunFactory().CreateRun(path);

            Assert.IsNotNull(run);


        }

    }
}
