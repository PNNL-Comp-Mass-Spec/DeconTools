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
		public static Dictionary<string, int> BuildAcetal()
		{
			Dictionary<string, int> acetal = new Dictionary<string, int>();
			acetal.Add("C", 2); 
			acetal.Add("H", 3);
			acetal.Add("O", 1);
			return acetal;
		}

		public static Dictionary<string, int> BuildPhosphate()
		{
			Dictionary<string, int> phosphate = new Dictionary<string, int>();
			phosphate.Add("P", 1);
			phosphate.Add("O", 3);
			return phosphate;
		}

		public static Dictionary<string, int> BuildMethyl()
		{
			Dictionary<string, int> methyl = new Dictionary<string, int>();
			methyl.Add("C", 1);
			methyl.Add("H", 3);
			return methyl;
		}


		[Test]
		public void TestIsotopeDict()
		{
			Dictionary<string, int> formula = formBuild.FormulaToDictionary("C80H100N20O30Na2F3Mg2S10");

			isotopicDistributionCalculator.GetIsotopePattern(formula);
		}

        //[Test]
        //public void SendData()
        //{
        //    DataTable dt = TextFileToDataTable(@"C:\Documents and Settings\aldr699\My Documents\ForGordon\ShawnaData.txt");

        //    string[] fields = new string[5] { "n_ac", "n_p", "n_me2","n_me3", "n_me"};
        //    dt.Columns.Add("Formula", typeof(string));

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {


        //        Dictionary<string, int> temp = formBuild.ConvertToMolecularFormula((string)dt.Rows[i]["Clean_Seqence"]);
        //        for (int j = 0; j < fields.Length; j++)
        //        {
        //            int count = int.Parse((string)dt.Rows[i][fields[j]]);
        //            if (j == 0)
        //            {
        //                for (int k = 0; k < count; k++)
        //                {
        //                    formBuild.AddFormulaToPreviousFormula("C2H3O", ref temp);
        //                }
        //            }
        //            else if (j == 1)
        //            {
        //                for (int k = 0; k < count; k++)
        //                {
        //                    formBuild.AddFormulaToPreviousFormula("PO3", ref temp);
        //                }
        //            }
        //            else if (j == 2)
        //            {
        //                for (int k = 0; k < count; k++)
        //                {
        //                    formBuild.AddFormulaToPreviousFormula("C2H6", ref temp);
        //                }
        //            }
        //            else if (j == 3)
        //            {
        //                for (int k = 0; k < count; k++)
        //                {
        //                    formBuild.AddFormulaToPreviousFormula("C3H9", ref temp);
        //                }
        //            }
        //            else if (j == 4)
        //            {
        //                for (int k = 0; k < count; k++)
        //                {
        //                    formBuild.AddFormulaToPreviousFormula("CH3", ref temp);
        //                }
        //            }
        //            count = 0;
        //        }

        //        string input = "";
        //        SortedList<string, int> fine = new SortedList<string, int>();
        //        foreach(KeyValuePair<string, int> l in temp)
        //        {
        //            fine.Add(l.Key, l.Value);
        //        }
        //        foreach (KeyValuePair<string, int> s in fine)
        //        {
        //            input += s.Key + s.Value + " ";
        //        }
        //        dt.Rows[i]["Formula"] = input.TrimEnd(' ');
        //    }
        //    //GuiInteraction.Write(dt, @"C:\Documents and Settings\aldr699\My Documents\ForGordon\ShawnaDatappend.txt");
        //}


		[Test]
		public void TestSingletonPattern()
		{
			IsotopicDistributionCalculator myPatterner = IsotopicDistributionCalculator.Instance;
			Assert.IsFalse(myPatterner.IsSetToLabeled);
			myPatterner.GetAvnPattern(2000);
		}
			



		[Test]
		public void TestIsotopeDict3()
		{
			Dictionary<string, int> formula = formBuild.FormulaToDictionary("C80H100N20O30Na2F3Mg2S10");

			IsotopicProfile cluster =  isotopicDistributionCalculator.GetIsotopePattern(formula);
			StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster);

			Console.Write(sb.ToString());

		}


		[Test]
		public void TestIsotopeDict2()
		{
			Dictionary<string, int> formula = formBuild.FormulaToDictionary("C150H300N50O60");

			IsotopicProfile cluster = isotopicDistributionCalculator.GetIsotopePattern(formula);
			
            
            StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster);

			Console.Write(sb.ToString());
			
		}

		[Test]
		public void TestItAll()
		{

			Dictionary<string, int> formula = formBuild.ConvertToMolecularFormula("ANKYLSRRH");
			IsotopicProfile cluster = isotopicDistributionCalculator.GetIsotopePattern(formula);
			StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster);

			Console.Write(sb.ToString());

		}

		

		/// <summary>
		/// Shows how to apply labeling and get the averagine pattern.  Compares Tom's code to my code.
		/// </summary>
		[Test]
		public void GenerateTheoreticalProfileFromAve()
		{
			bool isIt =isotopicDistributionCalculator.IsSetToLabeled;
			isotopicDistributionCalculator.SetLabeling("N", 14, .02, 15, .98);
			IsotopicProfile cluster = isotopicDistributionCalculator.GetAvnPattern(1979);
			isotopicDistributionCalculator.ResetToUnlabled();
			StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster);

			Console.Write(sb.ToString());


			IsotopicProfile cluster2 = _tomIsotopicPatternGenerator.GetAvnPattern(2000, true);


			Console.WriteLine(cluster2.Peaklist.Count);


			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster2);

			Console.WriteLine(sb.ToString());
		}


		[Test]
		public void GenerateTheoreticalProfileFromAve2()
		{
			bool isIt = isotopicDistributionCalculator.IsSetToLabeled;
	//		isotopicDistributionCalculator.SetLabeling("N", 14, .02, 15, .98);
			IsotopicProfile cluster = isotopicDistributionCalculator.GetAvnPattern(1979);
	//		isotopicDistributionCalculator.ResetToUnlabled();
			StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster);

			Console.Write(sb.ToString());


			IsotopicProfile cluster2 = _tomIsotopicPatternGenerator.GetAvnPattern(1979, false);


			Console.WriteLine(cluster2.Peaklist.Count);


			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster2);

			Console.WriteLine(sb.ToString());
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


        //public static DataTable TextFileToDataTable(string filePath)
        //{
        //    string line = "";
        //    string[] fields = null;
        ////	string dataSetName = System.IO.Path.GetFileName(filePath).Substring(0, System.IO.Path.GetFileName(filePath).IndexOf("_fht", 0));
        //    DataTable dt = new DataTable();
        //    try
        //    {
        //        using (System.IO.StreamReader sr = new System.IO.StreamReader(filePath))
        //        {
        //            // first line has headers   
        //            if ((line = sr.ReadLine()) != null)
        //            {
        //                fields = line.Split('\t');
        //                foreach (string s in fields)
        //                {
        //                    dt.Columns.Add(s);
        //                }
        //            }
        //            else
        //            {
        //                throw new ApplicationException("The data provided is not in a valid format.");
        //            }
        //            // fill the rest of the table; positional   
        //            while ((line = sr.ReadLine()) != null)
        //            {
        //                DataRow row = dt.NewRow();
        //                fields = line.Split('\t');
        //                int i = 0;
        //                foreach (string s in fields)
        //                {
        //                    row[i] = s;
        //                    i++;
        //                }
        //                dt.Rows.Add(row);
        //            }
        //        }
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("The file failed to load" + ex.Message);
        //        return null;
        //    }
        //}

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
			TestUtilities.ReportIsotopicProfileData(sb, distcreator.IsotopicProfile);

			IsotopicProfile cluster = _tomIsotopicPatternGenerator.GetAvnPattern(monoMass, false);
			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster);

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
			TestUtilities.ReportIsotopicProfileData(sb, cluster);



			Dictionary<string, int> testAgain = formBuild.ConvertToMolecularFormula("P");
			IsotopicProfile cluster2 = isotopicDistributionCalculator.GetIsotopePattern(testAgain);
			sb.Append(Environment.NewLine);
			TestUtilities.ReportIsotopicProfileData(sb, cluster2);

			Console.Write(sb.ToString());
		}

        [Test]
		public void test3()
		{
            TargetBase target = new MassTag();
            target.Code = "TTPSIIAYTDDETIVGQPAKRTTPSIIAYTDDETIVGQPAKRTTPSIIAYTDDETIVGQPAKR";
            target.EmpiricalFormula = target.GetEmpiricalFormulaFromTargetCode();
			
            IsotopicProfile cluster = _tomIsotopicPatternGenerator.GetIsotopePattern(target.EmpiricalFormula, _tomIsotopicPatternGenerator.aafIsos);

			
			TestUtilities.DisplayIsotopicProfileData(cluster);

		}

	}
}
