using System;
using System.Diagnostics;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using NUnit.Framework;
using PNNLOmics.Data.Constants;
using PNNLOmics.Data.FormulaBuilder;

namespace DeconTools.UnitTesting2.TheoreticalIsotopicProfileTests
{
    [TestFixture]
    public class IsotopicDistributionCalculatorTests
    {
        readonly TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();
        readonly IsotopicDistributionCalculator isotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;
        readonly AminoAcidFormulaBuilder formBuild = new AminoAcidFormulaBuilder();


        [Test]
        public void GetIsotopicProfileTest1()
        {
            var peptideUtils = new PeptideUtils();
            var empFormula=  peptideUtils.GetEmpiricalFormulaForPeptideSequence("SAMPLERSAMPLER");

            var isoCalc = IsotopicDistributionCalculator.Instance;
            var iso=  isoCalc.GetIsotopePattern(empFormula);


            TestUtilities.DisplayIsotopicProfileData(iso);




        }


        [Test]
        public void GetIsotopicProfileTest2()
        {
            var peptideUtils = new PeptideUtils();
            var empFormula = "S3";

            var isoCalc = IsotopicDistributionCalculator.Instance;
            var iso = isoCalc.GetIsotopePattern(empFormula);


            TestUtilities.DisplayIsotopicProfileData(iso);




        }




        [Test]
        public void TestSingletonPattern()
        {
            var myPatterner = IsotopicDistributionCalculator.Instance;
            Assert.IsFalse(myPatterner.IsSetToLabeled);
            myPatterner.GetAveraginePattern(2000);
        }


        [Test]
        public void getIsotopicProfileTest1()
        {
            var utils = new PeptideUtils();
            var formula = utils.GetEmpiricalFormulaForPeptideSequence("SAMPLERPAMPLERSAMPLERPAMPLERSAMPLERPAMPLERSAMPLERPAMPLER");

            var iso1 = isotopicDistributionCalculator.GetIsotopePattern(formula);
            PeakUtilities.TrimIsotopicProfile(iso1, 0.001);

            var tomGen = new TomIsotopicPattern();
            var iso2 = tomGen.GetIsotopePattern(formula);
            PeakUtilities.TrimIsotopicProfile(iso2, 0.001);

            for (var i = 0; i < iso1.Peaklist.Count; i++)
            {
                var roundedI1 = (decimal)Math.Round(iso1.Peaklist[i].Height, 2);
                var roundedI2 = (decimal)Math.Round(iso2.Peaklist[i].Height, 2);

                Assert.AreEqual(roundedI1, roundedI2);
            }

            TestUtilities.DisplayIsotopicProfileData(iso1);


        }

        [Test]
        public void getN15IsotopicProfileTest1()
        {
            var isIt = isotopicDistributionCalculator.IsSetToLabeled;


            var utils = new PeptideUtils();
            //string formula = utils.GetEmpiricalFormulaForPeptideSequence("SAMPLERPAMPLERSAMPLERPAMPLERSAMPLERPAMPLERSAMPLERPAMPLER");
            var formula = utils.GetEmpiricalFormulaForPeptideSequence("SAMPLERPAMPLERSAMPLERPAMPLER");



            var numNitrogens = utils.GetNumAtomsForElement("N", formula);


            var iso1 = isotopicDistributionCalculator.GetIsotopePattern(formula);

            PeakUtilities.TrimIsotopicProfile(iso1, 0.001);

            TestUtilities.DisplayIsotopicProfileData(iso1);

            isotopicDistributionCalculator.SetLabeling("N", 14, 0.02, 15, 0.98);

            var iso2 = isotopicDistributionCalculator.GetIsotopePattern(formula);
            //PeakUtilities.TrimIsotopicProfile(iso2, 0.001);

            isotopicDistributionCalculator.ResetToUnlabeled();
            var iso3 = isotopicDistributionCalculator.GetIsotopePattern(formula);
            PeakUtilities.TrimIsotopicProfile(iso3, 0.001);


            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(iso2);


            for (var i = 0; i < iso1.Peaklist.Count; i++)
            {
                Assert.AreEqual((decimal)Math.Round(iso1.Peaklist[i].Height, 4), (decimal)Math.Round(iso3.Peaklist[i].Height, 4));

            }

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(iso3);

            Console.WriteLine("Num nitrogens= " + numNitrogens);
        }




        [Test]
        public void TestIsotopeDict3()
        {
            var formula = formBuild.FormulaToDictionary("C80H100N20O30Na2F3Mg2S10");

            var cluster = isotopicDistributionCalculator.GetIsotopePattern(formula);
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());

        }


        [Test]
        public void TestIsotopeDict2()
        {
            var formula = formBuild.FormulaToDictionary("C150H300N50O60");

            var cluster = isotopicDistributionCalculator.GetIsotopePattern(formula);


            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());

        }

        [Test]
        public void TestItAll()
        {

            var formula = formBuild.ConvertToMolecularFormula("ANKYLSRRH");
            var cluster = isotopicDistributionCalculator.GetIsotopePattern(formula);
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());

        }



        /// <summary>
        /// Shows how to apply labeling and get the averagine pattern.  Compares Tom's code to my code.
        /// </summary>
        [Test]
        public void GenerateTheoreticalProfileFromAve()
        {
            var isIt = isotopicDistributionCalculator.IsSetToLabeled;
            isotopicDistributionCalculator.SetLabeling("N", 14, .02, 15, .98);
            var cluster = isotopicDistributionCalculator.GetAveraginePattern(1979);
            isotopicDistributionCalculator.ResetToUnlabeled();
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());


            var cluster2 = _tomIsotopicPatternGenerator.GetAvnPattern(2000, true);


            Console.WriteLine(cluster2.Peaklist.Count);


            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster2);

            Console.WriteLine(sb.ToString());
        }


        [Test]
        public void GenerateTheoreticalProfileFromAve2()
        {
            var isIt = isotopicDistributionCalculator.IsSetToLabeled;
            //		isotopicDistributionCalculator.SetLabeling("N", 14, .02, 15, .98);
            var cluster = isotopicDistributionCalculator.GetAveraginePattern(1979);
            //		isotopicDistributionCalculator.ResetToUnlabled();
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());


            var cluster2 = _tomIsotopicPatternGenerator.GetAvnPattern(1979, false);


            Console.WriteLine(cluster2.Peaklist.Count);


            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster2);

            Console.WriteLine(sb.ToString());
        }



        [Test]
        public void GetAveragineFormulaForGivenMass()
        {
            var averagineEmpiricalFormula= isotopicDistributionCalculator.GetAveragineFormulaAsString(1979,true);

            Console.WriteLine(averagineEmpiricalFormula);

            averagineEmpiricalFormula = isotopicDistributionCalculator.GetAveragineFormulaAsString(1979, false);

            Console.WriteLine(averagineEmpiricalFormula);

        }



        [Test]
        public void TestElements()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //using a string Key with a dictionary
            var elementKey = "C";

            var elementMass = Constants.Elements[elementKey].MassMonoIsotopic;
            var elementName = Constants.Elements[elementKey].Name;
            var elementSymbol = Constants.Elements[elementKey].Symbol;

            Assert.AreEqual(12.0, elementMass);
            Assert.AreEqual("Carbon", elementName);
            Assert.AreEqual("C", elementSymbol);

            //using a Select Key and Enum
            var elementMass3 = Constants.Elements[ElementName.Hydrogen].MassMonoIsotopic;
            var elementName3 = Constants.Elements[ElementName.Hydrogen].Name;
            var elementSymbol3 = Constants.Elements[ElementName.Hydrogen].Symbol;

            Assert.AreEqual(1.00782503196, elementMass3);
            Assert.AreEqual("Hydrogen", elementName3);
            Assert.AreEqual("H", elementSymbol3);

            //isotopes abundance on earth
            var elementC12Mass = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C12"].Mass;
            var elementC13Mass = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C13"].Mass;

            var elementC12MassC12Abundance = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C12"].NaturalAbundance;
            var elementC13MassC13Abundance = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C13"].NaturalAbundance;

            double elementC12MassC12IsotopeNumber = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C12"].IsotopeNumber;
            double elementC13MassC13IsotopeNumber = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C13"].IsotopeNumber;

            Assert.AreEqual(elementC12Mass, 12.0);
            Assert.AreEqual(elementC13Mass, 13.0033548385);
            Assert.AreEqual(elementC12MassC12Abundance, 0.98892228);
            Assert.AreEqual(elementC13MassC13Abundance, 0.01107828);
            Assert.AreEqual(elementC12MassC12IsotopeNumber, 12);
            Assert.AreEqual(elementC13MassC13IsotopeNumber, 13);

            stopWatch.Stop();
            Console.WriteLine("This took " + stopWatch.Elapsed + "seconds to TestElements");
        }




        [Test]
        public void test01()
        {

            var cluster = _tomIsotopicPatternGenerator.GetAvnPattern(2000, false);


            Console.WriteLine(cluster.Peaklist.Count);

            var sb = new StringBuilder();


            Console.WriteLine(sb.ToString());
        }
        [Test]
        public void tester1()
        {
            int[] empIntArray = { 49, 81, 19, 13, 0 };
            var formula = "C49H81N19O13";

            var iso = _tomIsotopicPatternGenerator.GetIsotopePattern(formula, _tomIsotopicPatternGenerator.aafIsos);

            //   TestUtilities.DisplayIsotopicProfileData(iso);
        }


        [Test]
        public void compareTomIsotopicDist_with_Mercury()
        {

            var mz = 1154.98841279744;    //mono MZ
            var chargestate = 2;
            var fwhm = 0.0290254950523376;   //from second peak of isotopic profile

            var monoMass = 1154.98841279744 * chargestate - chargestate * 1.00727649;
            var resolution = mz / fwhm;

            var distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(monoMass, chargestate, resolution);

            distcreator.getIsotopicProfile();
            Assert.AreEqual(5, distcreator.IsotopicProfile.GetNumOfIsotopesInProfile());

            var sb = new StringBuilder();
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, distcreator.IsotopicProfile);

            var cluster = _tomIsotopicPatternGenerator.GetAvnPattern(monoMass, false);
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());

        }

        [Test]
        public void test2()
        {

            //Peptide testPeptide = new Peptide("P");
            //IsotopicProfile cluster = TomIsotopicPattern.GetIsotopePattern(testPeptide.GetEmpiricalFormulaIntArray(), TomIsotopicPattern.aafIsos);
            var cluster = _tomIsotopicPatternGenerator.GetIsotopePattern("C5H9N1O2", _tomIsotopicPatternGenerator.aafIsos);
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);



            var testAgain = formBuild.ConvertToMolecularFormula("P");
            var cluster2 = isotopicDistributionCalculator.GetIsotopePattern(testAgain);
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster2);

            Console.Write(sb.ToString());
        }

        [Test]
        public void test3()
        {
            TargetBase target = new PeptideTarget();
            target.Code = "TTPSIIAYTDDETIVGQPAKRTTPSIIAYTDDETIVGQPAKRTTPSIIAYTDDETIVGQPAKR";
            target.EmpiricalFormula = target.GetEmpiricalFormulaFromTargetCode();

            var cluster = _tomIsotopicPatternGenerator.GetIsotopePattern(target.EmpiricalFormula, _tomIsotopicPatternGenerator.aafIsos);


            TestUtilities.DisplayIsotopicProfileData(cluster);

        }

    }
}
