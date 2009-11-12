using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;


namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class ParameterFileTests
    {
        string replaceRapidScoreParamFile1 = "..\\..\\TestFiles\\replaceRAPIDScoreParameterFile1.xml";

        [Test]
        public void test1()
        {
        }


        [Test]
        public void ensureReplaceRAPIDScore_parameterIsLoading1()
        {
            Project.Reset();
            Project.getInstance().LoadOldDecon2LSParameters(replaceRapidScoreParamFile1);

            Assert.AreEqual(true, Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ReplaceRAPIDScoreWithHornFitScore);

        }



    }
}
