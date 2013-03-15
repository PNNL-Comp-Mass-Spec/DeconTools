using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities.IqCodeParser;

namespace DeconTools.Workflows.Backend.FileIO
{
	public class MSAlignIqTargetImporter : ImporterBase<List<IqTarget>>
	{
		#region properties

		protected string[] DataFileNameHeader = {"data_file_name"};
		protected string[] PRSMIdHeader = {"prsm_id"};
		protected string[] SpectrumIdHeader = {"spectrum_id"};
		protected string[] ScansHeader = {"scan", "scan(s)"};
		protected string[] NumPeaksHeader = {"#peaks"};
		protected string[] ChargeHeader = {"charge"};
		protected string[] PrecursorMassHeader = {"precursor_mass"};
		protected string[] AdjustedPrecursorMassHeader = {"adjusted_precursor_mass"};
		protected string[] ProteinIdHeader = {"protein_id"};
		protected string[] ProteinNameHeader = {"protein_name"};
		protected string[] ProteinMassHeader = {"protein_mass"};
		protected string[] FirstResidueHeader = {"first_residue"};
		protected string[] LastResidueHeader = {"last_residue"};
		protected string[] PeptideHeader = {"peptide", "protein"};
		protected string[] NumUnexpectedModifications = {"#unexpected_modifications"};
		protected string[] NumMatchedPeaksHeader = {"#matched_peaks"};
		protected string[] NumMatchedFragmentIons = {"#matched_fragment_ions"};
		protected string[] PValueHeader = {"p-value"};
		protected string[] EValueHeader = {"e-value"};
		protected string[] FDRHeader = {"fdr"};

		protected string Filename { get; set; }

		#endregion

		#region public methods

		public MSAlignIqTargetImporter(string filename)
		{
			Filename = filename;
		}


		public override List<IqTarget> Import()
		{
			List<IqTarget> allTargets = new List<IqTarget>();

			StreamReader reader;

			if (!File.Exists(Filename))
			{
				throw new IOException("Cannot import. File does not exist.");
			}

			try
			{
				reader = new StreamReader(this.Filename);
			}
			catch (Exception)
			{
				throw new IOException("There was a problem importing from the file.");
			}

			//Sequence is the key and processed line is the value
			var parentTargetGroup = new Dictionary<string, List<List<string>>>();

			using (StreamReader sr = reader)
			{
				if (sr.Peek() == -1)
				{
					sr.Close();
					throw new InvalidDataException("There is no data in the file we are trying to read.");

				}

				string headerLine = sr.ReadLine();
				CreateHeaderLookupTable(headerLine);

				string line;
				int lineCounter = 1;   //used for tracking which line is being processed. 

				//read and process each line of the file
				while (sr.Peek() > -1)
				{
					line = sr.ReadLine();
					List<string> processedData = ProcessLine(line);

					//ensure that processed line is the same size as the header line
					if (processedData.Count != m_columnHeaders.Count)
					{
						throw new InvalidDataException("In File: " + Path.GetFileName(Filename) + "; Data in row # " + lineCounter.ToString() + " is NOT valid - \nThe number of columns does not match that of the header line");
					}

					//Implement EValueCutoff
					double EValueCutoff = 1.0E-3;
					if (ParseDoubleField(processedData, EValueHeader) < EValueCutoff)
					{
						if (!parentTargetGroup.ContainsKey(ParseStringField(processedData, PeptideHeader)))
						{
							parentTargetGroup.Add(ParseStringField(processedData, PeptideHeader), new List<List<string>>());
						}
						parentTargetGroup[ParseStringField(processedData, PeptideHeader)].Add(processedData);
					}

					lineCounter++;
				}
				sr.Close();
			}

			int target_id = 1;

			foreach (KeyValuePair<string, List<List<string>>> keyValuePair in parentTargetGroup)
			{
				IqTarget target = ConvertTextToIqTarget(keyValuePair.Value);
				target.ID = target_id;
				allTargets.Add(target);
				target_id++;
			}
			return allTargets;
		}

		#endregion

		protected TopDownIqTarget ConvertTextToIqTarget(List<List<string>> processedGroup)
		{
			TopDownIqTarget target = new TopDownIqTarget();
			MSAlignCodeParser parser = new MSAlignCodeParser(); 

			target.Code = ParseStringField(processedGroup[0] ,PeptideHeader);
			target.EmpiricalFormula = parser.GetEmpiricalFormulaFromSequence(target.Code);
			target.MonoMassTheor = ParseDoubleField(processedGroup[0], AdjustedPrecursorMassHeader);
			List<IqTarget> children = new List<IqTarget>();

			foreach (List<string> line in processedGroup)
			{
				var child =new IqChargeStateTarget();
				child.ChargeState = ParseIntField(line, ChargeHeader);
				child.ObservedScan = ParseIntField(line, ScansHeader);
				children.Add(child);
			}

			target.AddTargetRange(children);
			return target;
		}

	}
}
