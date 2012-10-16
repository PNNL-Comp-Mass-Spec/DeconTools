using System;
using System.Collections.Generic;
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
        TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();
        IsotopicDistributionCalculator isotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;
        AminoAcidFormulaBuilder formBuild = new AminoAcidFormulaBuilder();


        [Test]
        public void GetIsotopicProfileTest1()
        {
            PeptideUtils peptideUtils = new PeptideUtils();
            var empFormula=  peptideUtils.GetEmpiricalFormulaForPeptideSequence("SAMPLERSAMPLER");

            IsotopicDistributionCalculator isoCalc = IsotopicDistributionCalculator.Instance;
            IsotopicProfile iso=  isoCalc.GetIsotopePattern(empFormula);
            

            TestUtilities.DisplayIsotopicProfileData(iso);




        }


        [Test]
        public void GetIsotopicProfileTest2()
        {
            PeptideUtils peptideUtils = new PeptideUtils();
            var empFormula = "S3";

            IsotopicDistributionCalculator isoCalc = IsotopicDistributionCalculator.Instance;
            IsotopicProfile iso = isoCalc.GetIsotopePattern(empFormula);


            TestUtilities.DisplayIsotopicProfileData(iso);




        }




        [Test]
        public void TestSingletonPattern()
        {
            IsotopicDistributionCalculator myPatterner = IsotopicDistributionCalculator.Instance;
            Assert.IsFalse(myPatterner.IsSetToLabeled);
            myPatterner.GetAveraginePattern(2000);
        }


        [Test]
        public void getIsotopicProfileTest1()
        {
            PeptideUtils utils = new PeptideUtils();
            string formula = utils.GetEmpiricalFormulaForPeptideSequence("SAMPLERPAMPLERSAMPLERPAMPLERSAMPLERPAMPLERSAMPLERPAMPLER");

            IsotopicProfile iso1 = isotopicDistributionCalculator.GetIsotopePattern(formula);
            PeakUtilities.TrimIsotopicProfile(iso1, 0.001);

            TomIsotopicPattern tomGen = new TomIsotopicPattern();
            IsotopicProfile iso2 = tomGen.GetIsotopePattern(formula);
            PeakUtilities.TrimIsotopicProfile(iso2, 0.001);

            for (int i = 0; i < iso1.Peaklist.Count; i++)
            {
                decimal roundedI1 = (decimal)Math.Round(iso1.Peaklist[i].Height, 2);
                decimal roundedI2 = (decimal)Math.Round(iso2.Peaklist[i].Height, 2);

                Assert.AreEqual(roundedI1, roundedI2);
            }

            TestUtilities.DisplayIsotopicProfileData(iso1);


        }

        [Test]
        public void getN15IsotopicProfileTest1()
        {
            bool isIt = isotopicDistributionCalculator.IsSetToLabeled;


            PeptideUtils utils = new PeptideUtils();
            //string formula = utils.GetEmpiricalFormulaForPeptideSequence("SAMPLERPAMPLERSAMPLERPAMPLERSAMPLERPAMPLERSAMPLERPAMPLER");
            string formula = utils.GetEmpiricalFormulaForPeptideSequence("SAMPLERPAMPLERSAMPLERPAMPLER");



            int numNitrogens = utils.GetNumAtomsForElement("N", formula);


            IsotopicProfile iso1 = isotopicDistributionCalculator.GetIsotopePattern(formula);
            
            PeakUtilities.TrimIsotopicProfile(iso1, 0.001);

            TestUtilities.DisplayIsotopicProfileData(iso1);

            isotopicDistributionCalculator.SetLabeling("N", 14, 0.02, 15, 0.98);

            IsotopicProfile iso2 = isotopicDistributionCalculator.GetIsotopePattern(formula);
            //PeakUtilities.TrimIsotopicProfile(iso2, 0.001);

            isotopicDistributionCalculator.ResetToUnlabeled();
            IsotopicProfile iso3 = isotopicDistributionCalculator.GetIsotopePattern(formula);
            PeakUtilities.TrimIsotopicProfile(iso3, 0.001);


            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(iso2);
        

            for (int i = 0; i < iso1.Peaklist.Count; i++)
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
            Dictionary<string, int> formula = formBuild.FormulaToDictionary("C80H100N20O30Na2F3Mg2S10");

            IsotopicProfile cluster = isotopicDistributionCalculator.GetIsotopePattern(formula);
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());

        }


        [Test]
        public void TestIsotopeDict2()
        {
            Dictionary<string, int> formula = formBuild.FormulaToDictionary("C150H300N50O60");

            IsotopicProfile cluster = isotopicDistributionCalculator.GetIsotopePattern(formula);


            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());

        }

        [Test]
        public void TestItAll()
        {

            Dictionary<string, int> formula = formBuild.ConvertToMolecularFormula("ANKYLSRRH");
            IsotopicProfile cluster = isotopicDistributionCalculator.GetIsotopePattern(formula);
            StringBuilder sb = new StringBuilder();
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
            bool isIt = isotopicDistributionCalculator.IsSetToLabeled;
            isotopicDistributionCalculator.SetLabeling("N", 14, .02, 15, .98);
            IsotopicProfile cluster = isotopicDistributionCalculator.GetAveraginePattern(1979);
            isotopicDistributionCalculator.ResetToUnlabeled();
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());


            IsotopicProfile cluster2 = _tomIsotopicPatternGenerator.GetAvnPattern(2000, true);


            Console.WriteLine(cluster2.Peaklist.Count);


            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster2);

            Console.WriteLine(sb.ToString());
        }


        [Test]
        public void GenerateTheoreticalProfileFromAve2()
        {
            bool isIt = isotopicDistributionCalculator.IsSetToLabeled;
            //		isotopicDistributionCalculator.SetLabeling("N", 14, .02, 15, .98);
            IsotopicProfile cluster = isotopicDistributionCalculator.GetAveraginePattern(1979);
            //		isotopicDistributionCalculator.ResetToUnlabled();
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());


            IsotopicProfile cluster2 = _tomIsotopicPatternGenerator.GetAvnPattern(1979, false);


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
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //using a String Key with a dictionary
            string elementKey = "C";

            double elementMass = Constants.Elements[elementKey].MassMonoIsotopic;
            string elementName = Constants.Elements[elementKey].Name;
            string elementSymbol = Constants.Elements[elementKey].Symbol;

            Assert.AreEqual(12.0, elementMass);
            Assert.AreEqual("Carbon", elementName);
            Assert.AreEqual("C", elementSymbol);

            //using a Select Key and Enum
            double elementMass3 = Constants.Elements[ElementName.Hydrogen].MassMonoIsotopic;
            string elementName3 = Constants.Elements[ElementName.Hydrogen].Name;
            string elementSymbol3 = Constants.Elements[ElementName.Hydrogen].Symbol;

            Assert.AreEqual(1.00782503196, elementMass3);
            Assert.AreEqual("Hydrogen", elementName3);
            Assert.AreEqual("H", elementSymbol3);

            //isotopes abundance on earth
            double elementC12Mass = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C12"].Mass;
            double elementC13Mass = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C13"].Mass;

            double elementC12MassC12Abundance = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C12"].NaturalAbundance;
            double elementC13MassC13Abundance = Constants.Elements[ElementName.Carbon].IsotopeDictionary["C13"].NaturalAbundance;

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

            IsotopicProfile cluster = _tomIsotopicPatternGenerator.GetAvnPattern(2000, false);


            Console.WriteLine(cluster.Peaklist.Count);

            StringBuilder sb = new StringBuilder();


            Console.WriteLine(sb.ToString());
        }
        [Test]
        public void tester1()
        {
            int[] empIntArray = { 49, 81, 19, 13, 0 };
            string formula = "C49H81N19O13";

            IsotopicProfile iso = _tomIsotopicPatternGenerator.GetIsotopePattern(formula, _tomIsotopicPatternGenerator.aafIsos);

            //   TestUtilities.DisplayIsotopicProfileData(iso);
        }


        [Test]
        public void compareTomIsotopicDist_with_Mercury()
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

            StringBuilder sb = new StringBuilder();
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, distcreator.IsotopicProfile);

            IsotopicProfile cluster = _tomIsotopicPatternGenerator.GetAvnPattern(monoMass, false);
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);

            Console.Write(sb.ToString());

        }

        [Test]
        public void test2()
        {

            //Peptide testPeptide = new Peptide("P");
            //IsotopicProfile cluster = TomIsotopicPattern.GetIsotopePattern(testPeptide.GetEmpiricalFormulaIntArray(), TomIsotopicPattern.aafIsos);
            IsotopicProfile cluster = _tomIsotopicPatternGenerator.GetIsotopePattern("C5H9N1O2", _tomIsotopicPatternGenerator.aafIsos);
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            TestUtilities.IsotopicProfileDataToStringBuilder(sb, cluster);



            Dictionary<string, int> testAgain = formBuild.ConvertToMolecularFormula("P");
            IsotopicProfile cluster2 = isotopicDistributionCalculator.GetIsotopePattern(testAgain);
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

            IsotopicProfile cluster = _tomIsotopicPatternGenerator.GetIsotopePattern(target.EmpiricalFormula, _tomIsotopicPatternGenerator.aafIsos);


            TestUtilities.DisplayIsotopicProfileData(cluster);

        }

    }
}
