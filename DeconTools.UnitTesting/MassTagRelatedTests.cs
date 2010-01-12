using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class MassTagRelatedTests
    {
        [Test]
        public void createPeptideObjectTest1()
        {
            MassTag mt = new MassTag();
            mt.ID = 56488;
            mt.MonoIsotopicMass = 2275.1694779;
            mt.PeptideSequence = "TTPSIIAYTDDETIVGQPAKR";
            mt.NETVal = 0.3520239f;

            mt.CreatePeptideObject();
            Assert.AreNotEqual(null, mt.Peptide);
            Assert.AreEqual(2275.16959115176m, (decimal)mt.Peptide.MonoIsotopicMass);


        }


    }
}
