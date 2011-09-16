using DeconTools.Backend.Core;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.TargetedProcessing_Tests
{
    [TestFixture]
    public class MassTagInitializationTests
    {
        [Test]
        public void initializeFromEmpiricalFormula()
        {
            PeptideTarget mt = new PeptideTarget();

            mt.Code = "SAMPLER";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();

            Assert.AreEqual("C33H58N10O11S", mt.EmpiricalFormula);
            Assert.AreEqual(33,mt.GetAtomCountForElement("C"));
            

            
        }


    }
}
