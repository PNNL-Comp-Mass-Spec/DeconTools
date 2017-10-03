using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.FileIO
{
    public struct PrsmData
    {
        public float EValue;
        public double ProteinMass;
        public string ProteinName;
    }

    public class MassTagFromMSAlignFileImporter
    {
        /********** Magic number, how much to extend charge range by **********/
        /* For example, if this value is 3, charge range [5,8] becomes [2,11] */
        private const int EXTEND_CHARGE_RANGE = 3;
        /**********************************************************************/

        private const string PRSM_ID_HEADER = "Prsm_ID";
        private const string SPECTRUM_ID_HEADER = "Spectrum_ID";
        private const string PROTEIN_SEQ_ID_HEADER = "Protein_Sequence_ID";
        private const string SCAN_HEADER = "Scan(s)";
        private const string NUM_PEAKS_HEADER = "#peaks";
        private const string CHARGE_HEADER = "Charge";
        private const string PRECURSOR_MASS_HEADER = "Precursor_mass";
        private const string ADJUSTED_PRECURSOR_MASS_HEADER = "Adjusted_precursor_mass";
        private const string PROTEIN_ID_HEADER = "Protein_ID";
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

        private readonly Regex _modAcetylation =
            new Regex(@"^(?<prefix>[A-Z]?\.)\((?<modified>[A-Z]*)\)\[42\.[0-9]*\](?<suffix>.*)");

        private readonly Regex _modPhosphorylation =
            new Regex(
                @"^(?<prefix>[A-Z]?\.[A-Z0-9\(\)\[\]\.]*)\((?<modified>[A-Z]*[ST][A-Z]*)\)\[(?:79|80)\.[0-9]*\](?<suffix>.*)");

        private readonly Regex _modPyroglutamate =
            new Regex(@"^(?<prefix>[A-Z]?\.)\((?<modified>Q[A-Z]*)\)\[-17\.[0-9]*\](?<suffix>.*)");

        private readonly PeptideUtils _peptideUtils = new PeptideUtils();

        public MassTagFromMSAlignFileImporter(string filename)
        {
            _filename = filename;
        }

        public int DataRowsProcessed { get; private set; }

        public int DataRowsSkippedUnknownMods { get; private set; }

        public TargetCollection Import()
        {
            return Import(out var garbage);
        }

        public TargetCollection Import(out Dictionary<int, PrsmData> prsmData)
        {
            StreamReader reader;
            prsmData = new Dictionary<int, PrsmData>();

            if (!File.Exists(_filename))
            {
                throw new FileNotFoundException("Input file not found: " + _filename);
            }

            try
            {
                reader = new StreamReader(_filename);
            }
            catch (Exception ex)
            {
                throw new IOException("There was a problem importing from the file.", ex);
            }

            var targets = new TargetCollection();

            // Group LcmsFeatureTargets by their code
            var proteinSpeciesGroups = new Dictionary<string, List<LcmsFeatureTarget>>();

            using (var sr = reader)
            {
                if (sr.EndOfStream)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in the file we are trying to read.");
                }

                var columnHeaders = sr.ReadLine().Split('\t').ToList();
                var columnMapping = GetColumnMapping(columnHeaders);

                var lineCounter = 0; //used for tracking which line is being processed.

                //read and process each line of the file
                while (!sr.EndOfStream)
                {
                    ++lineCounter;
                    DataRowsProcessed = lineCounter;

                    var processedData = sr.ReadLine().Split('\t').ToList();

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != columnHeaders.Count)
                    {
                        throw new InvalidDataException("Data in row #" + lineCounter.ToString(CultureInfo.InvariantCulture) +
                                                       "is invalid - \nThe number of columns does not match that of the header line");
                    }

                    // Get Prsm_ID
                    if (!int.TryParse(processedData[columnMapping[PRSM_ID_HEADER]], out var prsmId))
                    {
                        throw new InvalidDataException("Could not parse Prsm ID.");
                    }

                    // Get scan
                    if (!int.TryParse(processedData[columnMapping[SCAN_HEADER]], out var scanLcTarget))
                    {
                        throw new InvalidDataException("Could not parse scan number.");
                    }

                    // Get charge state
                    if (!short.TryParse(processedData[columnMapping[CHARGE_HEADER]], out var chargeState))
                    {
                        throw new InvalidDataException("Could not parse charge.");
                    }

                    // Get code
                    var code = processedData[columnMapping[PEPTIDE_HEADER]];

                    string empiricalFormula;
                    // Modified species, try to get empirical formula
                    if (code.Contains("("))
                    {
                        empiricalFormula = GetEmpiricalFormulaForSequenceWithMods(code);
                        // Unknown modification in sequence, skip
                        if (string.IsNullOrEmpty(empiricalFormula))
                        {
                            ++DataRowsSkippedUnknownMods;
                            continue;
                        }
                    }
                    else
                    {
                        empiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(code);
                    }

                    // Get monoisotopic mass
                    var monoisotopicMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);

                    // Get Protein_mass
                    if (!double.TryParse(processedData[columnMapping[PROTEIN_MASS_HEADER]], out var proteinMass))
                    {
                        throw new InvalidDataException("Could not parse protein mass.");
                    }

                    // Get protein name
                    var proteinName = processedData[columnMapping[PROTEIN_NAME_HEADER]];

                    // Get score
                    if (!double.TryParse(processedData[columnMapping[E_VALUE_HEADER]], out var eValueDbl))
                    {
                        if (processedData[columnMapping[E_VALUE_HEADER]].ToLower() == "Infinity")
                        {
                            eValueDbl = float.MaxValue;
                        }
                        else
                        {
                            throw new InvalidDataException("Could not parse e-value.");
                        }
                    }

                    float eValue;
                    if (eValueDbl > float.MaxValue)
                    {
                        eValue = float.MaxValue;
                    }
                    else
                    {
                        eValue = (float) eValueDbl;
                    }

                    // Make Prsm
                    prsmData.Add(prsmId, new PrsmData {ProteinMass = proteinMass, ProteinName = proteinName, EValue = eValue});

                    // Create target
                    var target = new LcmsFeatureTarget
                                    {
                                        FeatureToMassTagID = prsmId,
                                        ID = -1,
                                        ElutionTimeUnit = DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum,
                                        ScanLCTarget = scanLcTarget,
                                        Code = code,
                                        EmpiricalFormula = empiricalFormula,
                                        MonoIsotopicMass = monoisotopicMass,
                                        ChargeState = chargeState,
                                        MZ = monoisotopicMass/chargeState + DeconTools.Backend.Globals.PROTON_MASS,
                                    };

                    if (!proteinSpeciesGroups.ContainsKey(code))
                    {
                        proteinSpeciesGroups.Add(code, new List<LcmsFeatureTarget>());
                    }
                    proteinSpeciesGroups[code].Add(target);

                    /*
                    // Create range
                    const int maxOffset = 0;

                    for (short offset = -maxOffset; offset <= maxOffset; offset++)
                    {
                        // Create new target
                        var newCharge = (short) (chargeState + offset);
                        var targetCopy = new LcmsFeatureTarget(target)
                        {
                            ID = ++idCounter,
                            ChargeState = newCharge,
                            MZ = target.MonoIsotopicMass / newCharge + DeconTools.Backend.Globals.PROTON_MASS
                        };
                        // Add target
                        targets.TargetIDList.Add(idCounter);
                        targets.TargetList.Add(targetCopy);
                    }
                    */
                }
                sr.Close();
            }

            // Loop through each protein species group and add in the missing charge states
            var idCounter = 0;
            foreach (var keyValuePair in proteinSpeciesGroups)
            {
                var targetGroup = AddChargeStates(keyValuePair.Value, EXTEND_CHARGE_RANGE);

                var chargeStates = new List<short> {0};
                // Add all targets in group and IDs to list
                foreach (var target in targetGroup)
                {
                    if (!chargeStates.Contains(target.ChargeState))
                    {
                        chargeStates.Add(target.ChargeState);
                        target.ID = ++idCounter;
                        targets.TargetList.Add(target);
                        targets.TargetIDList.Add(idCounter);
                    }
                }
            }

            return targets;
        }

        private IEnumerable<LcmsFeatureTarget> AddChargeStates(List<LcmsFeatureTarget> targets, int offsetMax)
        {
            if (targets == null || targets.Count == 0)
            {
                return new List<LcmsFeatureTarget>();
            }

            var minChargeTarget = targets[0];
            var maxChargeTarget = targets[0];

            foreach (var target in targets)
            {
                if (target.ChargeState < minChargeTarget.ChargeState)
                {
                    minChargeTarget = target;
                }
                if (target.ChargeState > maxChargeTarget.ChargeState)
                {
                    maxChargeTarget = target;
                }
            }

            // Extend charge range
            for (var i = 1; i <= offsetMax; i++)
            {
                var newCharge = (short) (minChargeTarget.ChargeState - i);
                targets.Add(new LcmsFeatureTarget(minChargeTarget)
                                {
                                    ChargeState = newCharge,
                                    MZ = minChargeTarget.MonoIsotopicMass/newCharge + DeconTools.Backend.Globals.PROTON_MASS
                                });
                newCharge = (short) (maxChargeTarget.ChargeState + i);
                targets.Add(new LcmsFeatureTarget(maxChargeTarget)
                                {
                                    ChargeState = newCharge,
                                    MZ = maxChargeTarget.MonoIsotopicMass/newCharge + DeconTools.Backend.Globals.PROTON_MASS
                                });
            }

            // Fill in gaps
            targets = targets.OrderBy(target => target.ChargeState).ToList();
            for (var i = 1; i < targets.Count; i++)
            {
                var prevTarget = targets[i - 1];
                var expectedCharge = (short) (prevTarget.ChargeState + 1);
                if (targets[i].ChargeState > expectedCharge)
                {
                    // Create and insert new target to fill gap
                    targets.Insert(i, new LcmsFeatureTarget(prevTarget)
                                        {
                                            ChargeState = expectedCharge,
                                            MZ = prevTarget.MonoIsotopicMass/expectedCharge + DeconTools.Backend.Globals.PROTON_MASS
                                        });
                }
            }

            // Return
            return targets;
        }

        public string GetEmpiricalFormulaForSequenceWithMods(string code)
        {
            // Check for pyroglutamate modification
            var containsPyroglutamateMod = false;
            if (_modPyroglutamate.IsMatch(code))
            {
                containsPyroglutamateMod = true;
                // Remove the modification from the string
                var pieces = _modPyroglutamate.Match(code).Groups;
                code = pieces["prefix"].Value + pieces["modified"].Value + pieces["suffix"].Value;
            }

            // Check for acetylation modification
            var containsAcetylationMod = false;
            if (_modAcetylation.IsMatch(code))
            {
                containsAcetylationMod = true;
                // Remove the modification from the string
                var pieces = _modAcetylation.Match(code).Groups;
                code = pieces["prefix"].Value + pieces["modified"].Value + pieces["suffix"].Value;
            }

            // Check for phosphorylation modification
            // TODO: this function does not account for the fact that phosphorylation could occur multiple times
            var containsPhosphorylationMod = false;
            if (_modPhosphorylation.IsMatch(code))
            {
                containsPhosphorylationMod = true;
                // Remove the modification from the string
                var pieces = _modPhosphorylation.Match(code).Groups;
                code = pieces["prefix"].Value + pieces["modified"].Value + pieces["suffix"].Value;
            }

            // If the peptide sequence still has modifications, we can't get the empirical formula
            if (code.Contains("("))
            {
                return string.Empty;
            }

            // Get the empirical formula as if the peptide sequence was unmodified
            var empiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(code);
            // Apply modifications to empirical formula
            if (containsPyroglutamateMod)
            {
                empiricalFormula = EmpiricalFormulaUtilities.SubtractFormula(empiricalFormula, "H3N1");
            }
            if (containsAcetylationMod)
            {
                empiricalFormula = EmpiricalFormulaUtilities.AddFormula(empiricalFormula, "C2H2O");
            }
            if (containsPhosphorylationMod)
            {
                empiricalFormula = EmpiricalFormulaUtilities.AddFormula(empiricalFormula, "HPO3");
            }

            return empiricalFormula;
        }

        private Dictionary<string, int> GetColumnMapping(IReadOnlyList<string> columnHeaders)
        {
            var columnMapping = new Dictionary<string, int>();
            for (var i = 0; i < columnHeaders.Count; i++)
            {
                var header = columnHeaders[i];
                if (!columnMapping.ContainsKey(header))
                {
                    columnMapping.Add(header, i);
                }
            }
            return columnMapping;
        }
    }
}