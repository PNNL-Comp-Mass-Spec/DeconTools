using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
	public abstract class IqTargetImporter : ImporterBase<List<IqTarget>>
	{
		private int _lineCounter;

		public enum HeaderSection
		{
			DatasetName,
			EmpiricalFormula,
			PeptideSequence,
			TargetID,
			MonoMass,
			AlternateID,
			MZ,
			Scan,
			NET,
			Quality,
			Charge
		}

		// This dictionary keeps track of which header sections are defined in the import file
		// That allows for calling functions to confirm that required sections are present
		private Dictionary<HeaderSection, bool> _HeaderSections;

		#region Constructors

		protected IqTargetImporter()
		{
			//note that case does not matter in the header
			DatasetHeaders = new[] { "dataset" };
			EmpiricalFormulaHeaders = new[] { "formula", "empirical_formula", "empiricalFormula", "molecular_formula", "molecularFormula" };
			CodeHeaders = new[] { "code", "sequence", "peptide" };
			TargetIDHeaders = new[] { "id", "mass_tag_id", "massTagid", "targetid", "mtid" };
			MonomassHeaders = new[] { "MonoMass", "MonoisotopicMass", "UMCMonoMW", "MonoMassIso1" };
			AlternateIDHeader = new[] { "MatchedMassTagID", "AlternateID" };
			MzHeaders = new[] { "MonoMZ", "MonoisotopicMZ", "UMCMZForChargeBasis" };
			ScanHeaders = new[] { "ScanLC", "LCScan", "Scan", "scanClassRep", "ScanNum" };
			NETHeaders = new[] { "net", "ElutionTime", "RT", "NETElutionTime", "ElutionTimeTheor" };
			QualityScoreHeaders = new[] { "QValue", "QualityScore", "EValue" };
			ChargeStateHeaders = new[] { "ChargeState", "Z", "Charge" };

			_HeaderSections = new Dictionary<HeaderSection, bool>();
		}


		#endregion

		#region Properties

		public string[] DatasetHeaders { get; set; }

		public string[] EmpiricalFormulaHeaders { get; set; }

		public string[] CodeHeaders { get; set; }

		public string[] TargetIDHeaders { get; set; }

		public string[] MonomassHeaders { get; set; }

		public string[] AlternateIDHeader { get; set; }

		public string[] MzHeaders { get; set; }

		public string[] ScanHeaders { get; set; }

		public string[] NETHeaders { get; set; }

		public string[] QualityScoreHeaders { get; set; }

		public string[] ChargeStateHeaders { get; set; }

		public Dictionary<HeaderSection, bool> HeaderSectionsFound
		{
			get
			{
				return _HeaderSections;
			}
		}

		// Full path to the file to import
		protected string Filename { get; set; }

		#endregion

		#region Public Methods

		public override List<IqTarget> Import()
		{
			List<IqTarget> allTargets = new List<IqTarget>();

			StreamReader reader;

			if (!File.Exists(Filename))
			{
				throw new System.IO.IOException("Cannot import. File does not exist.");
			}

			try
			{
				reader = new StreamReader(new FileStream(this.Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
			}
			catch (Exception ex)
			{
				throw new System.IO.IOException("There was a problem importing from the file: " + ex.Message);
			}

			using (StreamReader sr = reader)
			{
				if (sr.Peek() == -1)
				{
					sr.Close();
					throw new InvalidDataException("There is no data in the file we are trying to read.");

				}

				string headerLine = sr.ReadLine();
				CreateHeaderLookupTable(headerLine);

				bool areHeadersValid = ValidateHeaders();

				if (!areHeadersValid)
				{
					throw new InvalidDataException("There is a problem with the column headers.");
				}


				string line;
				_lineCounter = 1;   //used for tracking which line is being processed. 

				//read and process each line of the file
				while (sr.Peek() > -1)
				{
					line = sr.ReadLine();
					List<string> processedData = ProcessLine(line);

					//ensure that processed line is the same size as the header line
					if (processedData.Count != m_columnHeaders.Count)
					{
						throw new InvalidDataException("In File: " + Path.GetFileName(Filename) + "; Data in row # " + _lineCounter + " is NOT valid - \nThe number of columns does not match that of the header line");
					}

					IqTarget target = ConvertTextToIqTarget(processedData);

					allTargets.Add(target);
					_lineCounter++;

				}
				sr.Close();

			}

			//remove duplicates:
			var filteredTargets = (from n in allTargets
								   group n by new
												  {
													  n.ID
												  }
									   into grp
									   select grp.First()).ToList();

			return filteredTargets;
		}

		protected abstract IqTarget ConvertTextToIqTarget(List<string> processedRowOfText);

		protected void EnumerateSections()
		{
			var sectionsToCheck = new Dictionary<HeaderSection, string[]>();

			sectionsToCheck.Add(HeaderSection.DatasetName, DatasetHeaders);
			sectionsToCheck.Add(HeaderSection.EmpiricalFormula, EmpiricalFormulaHeaders);
			sectionsToCheck.Add(HeaderSection.PeptideSequence, CodeHeaders);
			sectionsToCheck.Add(HeaderSection.TargetID, TargetIDHeaders);
			sectionsToCheck.Add(HeaderSection.MonoMass, MonomassHeaders);
			sectionsToCheck.Add(HeaderSection.AlternateID, AlternateIDHeader);
			sectionsToCheck.Add(HeaderSection.MZ, MzHeaders);
			sectionsToCheck.Add(HeaderSection.Scan, ScanHeaders);
			sectionsToCheck.Add(HeaderSection.NET, NETHeaders);
			sectionsToCheck.Add(HeaderSection.Quality, QualityScoreHeaders);
			sectionsToCheck.Add(HeaderSection.Charge, ChargeStateHeaders);

			var dummyData = new List<string>();
			for (int i = 0; i < m_columnHeaders.Count; i++)
			{
				dummyData.Add("Defined");
			}

			_HeaderSections.Clear();
			foreach (var item in sectionsToCheck)
			{
				string value = LookupData(dummyData, item.Value, "Missing");

				if (value == "Defined")
					_HeaderSections.Add(item.Key, true);
				else
					_HeaderSections.Add(item.Key, false);
			}

		}

		protected virtual bool ValidateHeaders()
		{
			EnumerateSections();

			// Assure the necessary column headers are present
			if (!(_HeaderSections[HeaderSection.EmpiricalFormula] ||
				  _HeaderSections[HeaderSection.PeptideSequence]))
			{
				if (_HeaderSections[HeaderSection.MonoMass])
				{
					Console.WriteLine("Warning: empirical formula or peptide sequence is not defined, but monomass is defined; IQ may not work properly");
				}
				else if (_HeaderSections[HeaderSection.MZ])
				{
					Console.WriteLine("Warning: empirical formula or peptide sequence is not defined, but m/z is defined; IQ may not work properly");
				}
				else
				{
					throw new System.IO.IOException("Cannot import: must either have an empirical formula column or a peptide sequence column. " +
						"Acceptable column names: " + string.Join(", ", EmpiricalFormulaHeaders) + ", " + string.Join(", ", CodeHeaders));
				}

			}

			return true;
		}

		protected int GetAutoIncrementForTargetID()
		{
			return _lineCounter;
		}


		#endregion

		#region Private Methods

		#endregion


	}
}
