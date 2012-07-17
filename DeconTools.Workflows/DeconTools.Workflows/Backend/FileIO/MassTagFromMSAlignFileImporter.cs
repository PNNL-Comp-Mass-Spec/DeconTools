using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.FileIO
{
	public class MassTagFromMSAlignFileImporter
	{
		private const string PRSM_ID_HEADER = "Prsm_ID";
		private const string SPECTRUM_ID_HEADER = "Spectrum_ID";
		private const string PROTEIN_SEQ_ID_HEADER = "Protein_Sequence_ID";
		private const string SCAN_HEADER = "Scan(s)";
		private const string NUM_PEAKS_HEADER = "#peaks";
		private const string CHARGE_HEADER = "Charge";
		private const string PRECURSOR_MASS_HEADER = "Precursor_mass";
		private const string PROTEIN_NAME_HEADER = "Protein_name";
		private const string PROTEIN_MASS_HEADER = "Protein_mass";
		private const string FIRST_RESIDUE_HEADER = "First_residue";
		private const string LAST_RESIDUE_HEADER = "Last_residue";
		private const string PEPTIDE_HEADER = "Peptide";
		private const string NUM_UNEXPECTED_MODS_HEADER = "#unexpected_modifications";
		private const string NUM_MATCHED_PEAKS_HEADER = "#matched_peaks";
		private const string NUM_MATCHED_FRAGMENT_IONS_HEADER = "#matched_fragment_ions";
		private const string E_VALUE_HEADER = "E-value";

		private readonly string _filename;

		public MassTagFromMSAlignFileImporter(string filename)
		{
			_filename = filename;
		}

		public TargetCollection Import()
		{
			StreamReader reader;

			try
			{
				reader = new StreamReader(_filename);
			}
			catch (Exception)
			{
				throw new IOException("There was a problem importing from the file.");
			}

			var targets = new TargetCollection();

			using (StreamReader sr = reader)
			{
				if (sr.Peek() == -1)
				{
					sr.Close();
					throw new InvalidDataException("There is no data in the file we are trying to read.");
				}

				List<string> columnHeaders = sr.ReadLine().Split('\t').ToList();
				Dictionary<string, int> columnMapping = GetColumnMapping(columnHeaders);
				var peptideUtils = new PeptideUtils();
				
				int lineCounter = 0;   //used for tracking which line is being processed.

				int idCounter = 0;

				//read and process each line of the file
				while (sr.Peek() > -1)
				{
					++lineCounter;

					List<string> processedData = sr.ReadLine().Split('\t').ToList();

					//ensure that processed line is the same size as the header line
					if (processedData.Count != columnHeaders.Count)
					{
						throw new InvalidDataException("Data in row #" + lineCounter.ToString(CultureInfo.InvariantCulture) + "is invalid - \nThe number of columns does not match that of the header line");
					}

					// Get scan
					int scanLcTarget;
					if (!int.TryParse(processedData[columnMapping[SCAN_HEADER]], out scanLcTarget))
					{
						throw new InvalidDataException("Could not parse scan number.");
					}
					
					// Get charge state
					short chargeState;
					if (!short.TryParse(processedData[columnMapping[CHARGE_HEADER]], out chargeState))
					{
						throw new InvalidDataException("Could not parse charge.");
					}
					
					// Get code
					string code = processedData[columnMapping[PEPTIDE_HEADER]];

					/***** skip modified species *****/
					if (code.Contains("(")) continue;
					/*********************************/

					// Get empirical formula
					string empiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(code);
					
					// Get monoisotopic mass
					double monoisotopicMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);
					
					// Create base for targets in charge range
					var targetBase = new LcmsFeatureTarget
						{
							ElutionTimeUnit = DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum,
							ScanLCTarget = scanLcTarget,
							Code = code,
							EmpiricalFormula = empiricalFormula,
							MonoIsotopicMass = monoisotopicMass,
						};

					// Create range
					const int maxOffset = 1;

					for (short offset = -maxOffset; offset <= maxOffset; offset++)
					{
						// Create new target
						var newCharge = (short) (chargeState + offset);
						var targetCopy = new LcmsFeatureTarget(targetBase)
						{
							ID = ++idCounter,
							ChargeState = newCharge,
							MZ = targetBase.MonoIsotopicMass / newCharge + DeconTools.Backend.Globals.PROTON_MASS
						};
						// Add target
						targets.TargetIDList.Add(idCounter);
						targets.TargetList.Add(targetCopy);
					}

					/*
					if (!chargeStateInfoIsAvailable)
					{
						const double minMZToConsider = 400;
						const double maxMZToConsider = 1300;

						var targetList = new List<LcmsFeatureTarget>();

						for (int chargeState = 1; chargeState < 100; chargeState++)
						{
							double calcMZ = target.MonoIsotopicMass / chargeState + DeconTools.Backend.Globals.PROTON_MASS;
							if (calcMZ > minMZToConsider && calcMZ < maxMZToConsider)
							{
								var copiedMassTag = new LcmsFeatureTarget(target)
									{
										ChargeState = (short) chargeState,
										MZ = calcMZ
									};
									//we need to create multiple mass tags 

								targetList.Add(copiedMassTag);
							}
						}
						targets.TargetList.AddRange(targetList.Take(3));
					}
					else
					{
						targets.TargetList.Add(target);
					}
					*/
				}
				sr.Close();
			}

			return targets;
		}

		private Dictionary<string, int> GetColumnMapping(List<string> columnHeaders)
		{
			var columnMapping = new Dictionary<string, int>();
			for (int i = 0; i < columnHeaders.Count(); i++)
			{
				string header = columnHeaders[i];
				if (!columnMapping.ContainsKey(header)) columnMapping.Add(header, i);
			}
			return columnMapping;
		}
	}
}
