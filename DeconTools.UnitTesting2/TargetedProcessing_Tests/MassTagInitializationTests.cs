using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting2.TargetedProcessing_Tests
{
    [TestFixture]
    public class MassTagInitializationTests
    {
        [Test]
        public void initializeFromEmpiricalFormula()
        {
            MassTag mt = new MassTag();
            mt.EmpiricalFormula = "C100H302N70O84S6";

            int[] empiricalArray = mt.GetEmpiricalFormulaAsIntArray();

            Assert.AreEqual(100, empiricalArray[0]);
            Assert.AreEqual(302, empiricalArray[1]);
            Assert.AreEqual(70, empiricalArray[2]);
            Assert.AreEqual(84, empiricalArray[3]);
            Assert.AreEqual(6, empiricalArray[4]);
        }


    }
}
