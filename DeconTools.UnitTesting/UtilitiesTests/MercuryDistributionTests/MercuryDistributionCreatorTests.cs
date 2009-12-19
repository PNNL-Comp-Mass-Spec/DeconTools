using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;


namespace DeconTools.UnitTesting.UtilitiesTests
{
    [TestFixture]
    public class MercuryDistributionCreatorTests
    {
        [Test]
        public void GetDistributionTest1()
        {

            //I got the data below from file: QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW
            // scan = 6067
            //can use this as a means of overlaying the mercury data on the actual data


            double mz = 1154.98841279744;    //mono MZ
            int chargestate = 2;
            double fwhm = 0.0290254950523376;   //from second peak of isotopic profile

            double monoMass = 1154.98841279744 * chargestate - chargestate * 1.00727649;
            double resolution = mz / fwhm;

            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(monoMass, chargestate, resolution);

            
            Assert.AreEqual(3778, distcreator.Data.Xvalues.Length);
            Assert.AreEqual("C103H151N28O31S1", distcreator.MolecularFormula.ToFormulaString());
            //MolecularFormula.Phosphorus.

            StringBuilder sb=new StringBuilder();
            TestUtilities.GetXYValuesToStringBuilder(sb, distcreator.Data.Xvalues, distcreator.Data.Yvalues);
            Console.Write(sb.ToString());

        }

        [Test]
        public void getpeaksFromDistributionTest1()
        {

            double mz = 1154.98841279744;    //mono MZ
            int chargestate = 2;
            double fwhm = 0.0290254950523376;   //from second peak of isotopic profile

            double monoMass = 1154.98841279744 * chargestate - chargestate * 1.00727649;
            double resolution = mz / fwhm;

            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(monoMass, chargestate, resolution);

            distcreator.getIsotopicProfile();
            Assert.AreEqual(8, distcreator.IsotopicProfile.GetNumOfIsotopesInProfile());

            StringBuilder sb=new StringBuilder();
            TestUtilities.ReportIsotopicProfileData(sb, distcreator.IsotopicProfile);
            Console.Write(sb.ToString());

        }

        [Test]
        public void getFormulasTest1()
        {
            MercuryDistributionCreator merc = new MercuryDistributionCreator();
            int chargeState = 3;
            MolecularFormula formula;

            for (double mz = 500; mz < 800; mz++)
            {

                formula = merc.GetAveragineFormula(mz, chargeState);

                StringBuilder sb = new StringBuilder();
                sb.Append(mz);
                sb.Append("\t");
                reportFormula(sb, formula);

                Console.Write(sb.ToString());
            }


        }

       





        private void reportFormula(StringBuilder sb, MolecularFormula formula)
        {
            sb.Append(formula.ToFormulaString());
            sb.Append("\n");
        }

       




    }
}
