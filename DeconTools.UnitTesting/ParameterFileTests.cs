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
        string standardXCaliburParamFile1 = "..\\..\\TestFiles\\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash.xml";

        string xcaliburSum5Adv3ParamFile1 = "..\\..\\TestFiles\\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Sum5_Advance3.xml";

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

        [Test]
        public void ensureNumScansToAdvance_parameterIsLoading1()
        {
            Project.Reset();
            Project.getInstance().LoadOldDecon2LSParameters(standardXCaliburParamFile1);
            Assert.AreEqual(1, Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumPeaksUsedInAbundance);
        }

        [Test]
        public void summingAndScanAdvance_parametersTesting()
        {
            Project.Reset();
            Project.getInstance().LoadOldDecon2LSParameters(xcaliburSum5Adv3ParamFile1);

            Assert.AreEqual(true, Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SumSpectraAcrossScanRange);
            Assert.AreEqual(5, Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToSumOver);
            Assert.AreEqual(3, Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance);


        }





    }
}
