using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using PRISM;

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
        // Ignore Spelling: Acetyl, proteoform, Prsm

        /********** Magic number, how much to extend charge range by **********/
        /* For example, if this value is 3, charge range [5,8] becomes [2,11] */
        private const int EXTEND_CHARGE_RANGE = 3;
        /**********************************************************************/

        // Header names in MSAlign result files:
        // Data_file_name    Prsm_ID    Spectrum_ID    Scan(s)    #peaks    Charge    Precursor_mass    Adjusted_precursor_mass    Protein_ID    Protein_name    Protein_mass    First_residue    Last_residue    Peptide    #unexpected_modifications    #matched_peaks    #matched_fragment_ions    P-value    E-value    FDR

        // The following constants are the columns that this class uses
        private const string MSALIGN_PRSM_ID_HEADER = "Prsm_ID";
        private const string SCAN_HEADER = "Scan(s)";
        private const string CHARGE_HEADER = "Charge";
        private const string MSALIGN_PROTEIN_NAME_HEADER = "Protein_name";
        private const string MSALIGN_PROTEIN_MASS_HEADER = "Protein_mass";
        private const string PEPTIDE_HEADER = "Peptide";
        private const string E_VALUE_HEADER = "E-value";

        // Header names in TopPIC result files
        // Data file name    Prsm ID    Spectrum ID    Fragmentation    Scan(s)    #peaks    Charge    Precursor mass    Adjusted precursor mass    Proteoform ID    Feature intensity    Protein name    First residue    Last residue    Proteoform    #unexpected modifications    #matched peaks    #matched fragment ions    P-value    E-value    Q-value (spectral FDR)    Proteoform FDR    #Variable PTMs

        // The following constants are the columns that this class uses
        private const string TopPIC_PRSM_ID_HEADER = "Prsm ID";
        private const string TopPIC_SCAN_HEADER = SCAN_HEADER;
        private const string TopPIC_CHARGE_HEADER = CHARGE_HEADER;
        private const string TopPIC_PROTEIN_NAME_HEADER = "Protein accession";
        private const string TopPIC_PROTEIN_MASS_HEADER = "Adjusted precursor mass";
        private const string TopPIC_PEPTIDE_HEADER = "Proteoform";
        private const string TopPIC_E_VALUE_HEADER = E_VALUE_HEADER;

        private readonly string _filename;

        // ReSharper disable CommentTypo

        /// <summary>
        /// RegEx for matching peptides of the form:
        /// K.(K)[42.010565]TAEKVDAKTEAVKKEVK.
        /// .(K)[42.010565]TAEKVDAKTEAVK.K
        /// M.AAKIRRQDEVIV(LAGKDK)[42.010565]GKRAKVAQV(LP)[109.10955].T
        /// </summary>
        private readonly Regex _modAcetylationMass =
            new Regex(@"^(?<Prefix>[A-Z]?\.)(?<StartingChars>[^(]*)\((?<ModifiedResidues>[A-Z]*)\)\[42\.[0-9]*\](?<RemainingChars>.*)", RegexOptions.Compiled);

        /// <summary>
        /// RegEx for matching peptides of the form
        /// K.(K)[Acetyl]TAEKVDAKTEAVKKEVK.
        /// M.AAKIRRQDEVIV(LAGKDK)[Acetyl]GKRAKVAQV(LP)[109.10955].T
        /// </summary>
        private readonly Regex _modAcetylationName =
            new Regex(@"^(?<Prefix>[A-Z]?\.)(?<StartingChars>[^(]*)\((?<ModifiedResidues>[A-Z]*)\)\[Acetyl\](?<RemainingChars>.*)", RegexOptions.Compiled);

        /// <summary>
        /// RegEx for matching peptides of the form
        /// A.IRGATGLGLKEAKAMSEAAPVAVKEGV(S)[79.966]KEEAEALKKELVEAGASVEIK.
        /// A.IRGATGLGLKEAKAMSEAAPVAVKEGV(S)[80.04]KEEAEALKKELVEAGASVEIK.
        /// A.IRGATGLGLKEAKAMSEAAPVAVKEGV(S)[80]KEEAEALKKELVEAGASVEIK.
        /// </summary>
        /// <remarks>In this RegEx, ?: is used for a non-capturing subgroup</remarks>
        private readonly Regex _modPhosphorylation =
            new Regex(@"^(?<Prefix>[A-Z]?\.)(?<StartingChars>[^(]*)\((?<ModifiedResidues>[A-Z]*[ST][A-Z]*)\)\[(?:79\.9[0-9]*|80\.0[0-9]*|80)\](?<RemainingChars>.*)", RegexOptions.Compiled);

        /// <summary>
        /// RegEx for matching peptides of the form:
        /// S.(QSRVTSNG)[-17.02918]YGITKPLVAGNSKEAHAANRRIEAIVT.T
        /// A.(Q)[-17.03004]GVVHEGTVVDTMNGGGYTYVQIK.E
        /// </summary>
        /// <remarks>In this RegEx, ?: is used for a non-capturing subgroup</remarks>
        private readonly Regex _modPyroglutamate =
            new Regex(@"^(?<Prefix>[A-Z]?\.)(?<StartingChars>[^(]*)\((?<ModifiedResidues>Q[A-Z]*)\)\[(?:-17\.0[0-9]*|-17)\](?<RemainingChars>.*)", RegexOptions.Compiled);

        // ReSharper restore CommentTypo

        private readonly Dictionary<Regex, KnownModIDs> _modRegExes;

        private readonly PeptideUtils _peptideUtils = new PeptideUtils();

        private enum ResultColumnIDs
        {
            PrsmID = 0,
            Scan = 1,
            Charge = 2,
            ProteinName = 3,
            ProteinMass = 4,
            Peptide = 5,
            EValue = 6
        }

        private enum KnownModIDs
        {
            Acetyl = 0,
            Phospho = 1,
            PyroGlu = 2
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename"></param>
        public MassTagFromMSAlignFileImporter(string filename)
        {
            _filename = filename;
            _modRegExes = new Dictionary<Regex, KnownModIDs>
            {
                {_modAcetylationMass, KnownModIDs.Acetyl},
                {_modAcetylationName, KnownModIDs.Acetyl},
                {_modPhosphorylation, KnownModIDs.Phospho},
                {_modPyroglutamate, KnownModIDs.PyroGlu}
            };
        }

        public int DataRowsProcessed { get; private set; }

        public int DataRowsSkippedBadData { get; private set; }

        public int DataRowsSkippedUnknownMods { get; private set; }

        public TargetCollection Import()
        {
            return Import(out _);
        }

        public TargetCollection Import(out Dictionary<int, PrsmData> prsmData)
        {
            DataRowsSkippedBadData = 0;
            DataRowsSkippedUnknownMods = 0;

            prsmData = new Dictionary<int, PrsmData>();

            if (!File.Exists(_filename))
            {
                throw new FileNotFoundException("Input file not found: " + _filename);
            }

            // Group LcmsFeatureTargets by their sequence
            var proteinSpeciesGroups = new Dictionary<string, List<LcmsFeatureTarget>>();

            using (var reader = new StreamReader(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                if (reader.EndOfStream)
                {
                    reader.Close();
                    throw new InvalidDataException("There is no data in file " + _filename);
                }

                // Keys are column IDs, values are the index of the column in the input file
                var columnMapping = new Dictionary<ResultColumnIDs, int>();

                // Keys are column IDs, values are the name of the column in the input file (for reporting warnings)
                var headerColumnNames = new Dictionary<ResultColumnIDs, string>();

                var headersParsed = false;
                var lineCounter = 0;

                // Read and process each line of the file
                while (!reader.EndOfStream)
                {
                    var dataLine = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(dataLine))
                        continue;

                    if (!headersParsed)
                    {
                        var success = GetColumnMapping(dataLine, columnMapping, headerColumnNames);
                        if (!success)
                            throw new InvalidDataException("The input file is missing required headers " + _filename);

                        headersParsed = true;
                        continue;
                    }

                    ++lineCounter;
                    DataRowsProcessed = lineCounter;

                    var processedData = dataLine.Split('\t').ToList();

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count < columnMapping.Count)
                    {
                        ConsoleMsgUtils.ShowWarning("Error: Data in row #{0} is invalid: the number of columns does not match the number of headers", lineCounter);
                        continue;
                    }

                    // Get Prsm_ID
                    if (!GetInt(processedData, columnMapping, headerColumnNames, ResultColumnIDs.PrsmID, out var prsmId, out var prsmWarning))
                    {
                        ConsoleMsgUtils.ShowWarning(prsmWarning);
                        continue;
                    }

                    // Get scan
                    var validScan = GetInt(
                        processedData, columnMapping, headerColumnNames, ResultColumnIDs.Scan,
                        out var scanLcTarget, out var scanWarning);

                    // Get charge state
                    var validCharge = GetInt(
                        processedData, columnMapping, headerColumnNames, ResultColumnIDs.Charge,
                        out var chargeState, out var chargeWarning);

                    // Get the peptide (with mod masses)
                    var peptideWithMods = processedData[columnMapping[ResultColumnIDs.Peptide]];

                    if (string.IsNullOrWhiteSpace(peptideWithMods))
                    {
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        if (!validScan && !validCharge)
                        {
                            // Starting with TopPIC v1.5, additional proteins for each proteoform are listed on their own line, with several empty columns (including Scan and Charge)
                            // Silently ignore this line
                            continue;
                        }

                        if (!validScan)
                        {
                            ConsoleMsgUtils.ShowWarning(scanWarning);
                        }
                        else if (!validCharge)
                        {
                            ConsoleMsgUtils.ShowWarning(chargeWarning);
                        }
                        else
                        {
                            ConsoleMsgUtils.ShowWarning("Peptide column is empty");
                        }

                        continue;
                    }

                    string empiricalFormula;

                    // Modified species, try to get empirical formula
                    if (peptideWithMods.Contains("("))
                    {
                        empiricalFormula = GetEmpiricalFormulaForSequenceWithMods(peptideWithMods);

                        // Unknown modification in sequence, skip this peptide
                        if (string.IsNullOrEmpty(empiricalFormula))
                        {
                            ++DataRowsSkippedUnknownMods;
                            continue;
                        }
                    }
                    else
                    {
                        empiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideWithMods);
                    }

                    // Get monoisotopic mass
                    var monoisotopicMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);

                    // Get Protein_mass
                    if (!GetDouble(processedData, columnMapping, headerColumnNames, ResultColumnIDs.ProteinMass, out var proteinMass))
                        continue;

                    // Get protein name
                    var proteinName = processedData[columnMapping[ResultColumnIDs.ProteinName]];

                    // Get score
                    if (!GetDouble(processedData, columnMapping, headerColumnNames, ResultColumnIDs.EValue, out var eValueDbl))
                        continue;

                    float eValue;
                    if (eValueDbl > float.MaxValue)
                    {
                        eValue = float.MaxValue;
                    }
                    else
                    {
                        eValue = (float)eValueDbl;
                    }

                    // Make Prsm
                    prsmData.Add(prsmId, new PrsmData { ProteinMass = proteinMass, ProteinName = proteinName, EValue = eValue });

                    // Create target
                    var target = new LcmsFeatureTarget
                    {
                        FeatureToMassTagID = prsmId,
                        ID = -1,
                        ElutionTimeUnit = DeconTools.Backend.Globals.ElutionTimeUnit.ScanNum,
                        ScanLCTarget = scanLcTarget,
                        Code = peptideWithMods,
                        EmpiricalFormula = empiricalFormula,
                        MonoIsotopicMass = monoisotopicMass,
                        ChargeState = (short)chargeState,
                        MZ = monoisotopicMass / chargeState + DeconTools.Backend.Globals.PROTON_MASS
                    };

                    if (!proteinSpeciesGroups.ContainsKey(peptideWithMods))
                    {
                        proteinSpeciesGroups.Add(peptideWithMods, new List<LcmsFeatureTarget>());
                    }

                    proteinSpeciesGroups[peptideWithMods].Add(target);

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
            }

            var targets = new TargetCollection();

            // Loop through each protein species group and add in the missing charge states
            var idCounter = 0;
            foreach (var keyValuePair in proteinSpeciesGroups)
            {
                var targetGroup = AddChargeStates(keyValuePair.Value, EXTEND_CHARGE_RANGE);

                var chargeStates = new List<short> { 0 };
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
                var newCharge = (short)(minChargeTarget.ChargeState - i);
                targets.Add(new LcmsFeatureTarget(minChargeTarget)
                {
                    ChargeState = newCharge,
                    MZ = minChargeTarget.MonoIsotopicMass / newCharge + DeconTools.Backend.Globals.PROTON_MASS
                });
                newCharge = (short)(maxChargeTarget.ChargeState + i);
                targets.Add(new LcmsFeatureTarget(maxChargeTarget)
                {
                    ChargeState = newCharge,
                    MZ = maxChargeTarget.MonoIsotopicMass / newCharge + DeconTools.Backend.Globals.PROTON_MASS
                });
            }

            // Fill in gaps
            targets = targets.OrderBy(target => target.ChargeState).ToList();
            for (var i = 1; i < targets.Count; i++)
            {
                var prevTarget = targets[i - 1];
                var expectedCharge = (short)(prevTarget.ChargeState + 1);
                if (targets[i].ChargeState > expectedCharge)
                {
                    // Create and insert new target to fill gap
                    targets.Insert(i, new LcmsFeatureTarget(prevTarget)
                    {
                        ChargeState = expectedCharge,
                        MZ = prevTarget.MonoIsotopicMass / expectedCharge + DeconTools.Backend.Globals.PROTON_MASS
                    });
                }
            }

            // Return
            return targets;
        }

        private bool DefineMapping(
            IReadOnlyDictionary<string, int> columnHeaderToIndexMap,
            string columnNameToFind,
            ResultColumnIDs resultColumn,
            IDictionary<ResultColumnIDs, int> columnMapping,
            IDictionary<ResultColumnIDs, string> headerColumnNames)
        {
            if (!columnHeaderToIndexMap.TryGetValue(columnNameToFind, out var columnIndex))
                return false;

            columnMapping.Add(resultColumn, columnIndex);
            headerColumnNames.Add(resultColumn, columnNameToFind);

            return true;
        }

        public string GetEmpiricalFormulaForSequenceWithMods(string peptideWithMods)
        {
            var peptideResidues = string.Copy(peptideWithMods);

            var modCountsByType = new Dictionary<KnownModIDs, int>();

            // Use a while loop to keep searching for all known mods in _modRegExes
            // The loop is exited once no more matches are found
            while (true)
            {
                var matchCount = 0;
                foreach (var modRegEx in _modRegExes)
                {
                    var modMatched = RemoveModViaRegEx(modRegEx, ref peptideResidues, modCountsByType);
                    if (modMatched)
                    {
                        matchCount++;
                    }
                }

                if (matchCount == 0)
                    break;
            }

            // If the peptide sequence still has modifications, we can't get the empirical formula
            if (peptideResidues.Contains("("))
            {
                return string.Empty;
            }

            // Get the empirical formula as if the peptide sequence was unmodified
            var empiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideResidues);

            // Apply modifications to empirical formula
            foreach (var modCount in modCountsByType)
            {
                string empiricalFormulaForMod;

                switch (modCount.Key)
                {
                    case KnownModIDs.Acetyl:
                        empiricalFormulaForMod = "C2H2O";
                        break;
                    case KnownModIDs.Phospho:
                        empiricalFormulaForMod = "HPO3";
                        break;
                    case KnownModIDs.PyroGlu:
                        empiricalFormulaForMod = "H3N1";
                        break;
                    default:
                        continue;
                }

                for (var i = 0; i < modCount.Value; i++)
                {
                    empiricalFormula = EmpiricalFormulaUtilities.SubtractFormula(empiricalFormula, empiricalFormulaForMod);
                }
            }

            return empiricalFormula;
        }

        /// <summary>
        /// Compare the header names in the data line to known column names to construct a mapping from column enum to column index
        /// </summary>
        /// <param name="dataLine">Line of headers from the input file</param>
        /// <param name="columnMapping">Dictionary mapping from result column enum to column index</param>
        /// <param name="headerColumnNames">Dictionary mapping from result column enum to column name (for reporting warnings to the user)</param>
        /// <returns>True if valid header names, false if missing one or more columns</returns>
        private bool GetColumnMapping(
            string dataLine,
            IDictionary<ResultColumnIDs, int> columnMapping,
            IDictionary<ResultColumnIDs, string> headerColumnNames)
        {
            var columnHeaders = dataLine.Split('\t').ToList();

            var columnHeaderToIndexMap = new Dictionary<string, int>();

            for (var i = 0; i < columnHeaders.Count; i++)
            {
                var headerName = columnHeaders[i];
                if (!columnHeaderToIndexMap.ContainsKey(headerName))
                {
                    columnHeaderToIndexMap.Add(headerName, i);
                }
            }

            columnMapping.Clear();
            headerColumnNames.Clear();

            var knownColumns = new Dictionary<ResultColumnIDs, List<string>>
            {
                {ResultColumnIDs.PrsmID, new List<string> {TopPIC_PRSM_ID_HEADER, MSALIGN_PRSM_ID_HEADER}},
                {ResultColumnIDs.Scan, new List<string> {SCAN_HEADER}},
                {ResultColumnIDs.Charge, new List<string> {CHARGE_HEADER}},
                {ResultColumnIDs.ProteinName, new List<string> {TopPIC_PROTEIN_NAME_HEADER, MSALIGN_PROTEIN_NAME_HEADER}},
                {ResultColumnIDs.ProteinMass, new List<string> {TopPIC_PROTEIN_MASS_HEADER, MSALIGN_PROTEIN_MASS_HEADER}},
                {ResultColumnIDs.Peptide, new List<string> {TopPIC_PEPTIDE_HEADER, PEPTIDE_HEADER}},
                {ResultColumnIDs.EValue, new List<string> {E_VALUE_HEADER}}
            };

            foreach (var knownColumn in knownColumns)
            {
                var success = false;
                foreach (var knownColumnName in knownColumn.Value)
                {
                    success = DefineMapping(columnHeaderToIndexMap, knownColumnName, knownColumn.Key, columnMapping, headerColumnNames);
                    if (success)
                        break;
                }

                if (success)
                    continue;

                if (knownColumn.Value.Count == 1)
                {
                    ConsoleMsgUtils.ShowError("Header line of the input file does not contain {0}", knownColumn.Value.First());
                }
                else if (knownColumn.Value.Count > 1)
                {
                    ConsoleMsgUtils.ShowError("Header line of the input file does not contain {0} or {1}", knownColumn.Value[0], knownColumn.Value[1]);
                }
                else
                {
                    ConsoleMsgUtils.ShowError("Programming bug: no column names defined for column Id {0}", knownColumn.Key);
                }

                return false;
            }

            return true;
        }

        private bool GetDouble(
            IReadOnlyList<string> processedData,
            IReadOnlyDictionary<ResultColumnIDs, int> columnMapping,
            IReadOnlyDictionary<ResultColumnIDs, string> headerColumnNames,
            ResultColumnIDs columnId,
            out double value)
        {
            var valueText = processedData[columnMapping[columnId]];

            if (double.TryParse(valueText, out value))
                return true;

            if (valueText.ToLower() == "infinity")
            {
                value = float.MaxValue;
                return true;
            }

            var columnName = headerColumnNames[columnId];
            ConsoleMsgUtils.ShowWarning("Could not parse a double from '{0}' in column {1}", valueText, columnName);
            return false;
        }

        private bool GetInt(
            IReadOnlyList<string> processedData,
            IReadOnlyDictionary<ResultColumnIDs, int> columnMapping,
            IReadOnlyDictionary<ResultColumnIDs, string> headerColumnNames,
            ResultColumnIDs columnId,
            out int value,
            out string warning)
        {
            var valueText = processedData[columnMapping[columnId]];

            if (int.TryParse(valueText, out value))
            {
                warning = string.Empty;
                return true;
            }

            var columnName = headerColumnNames[columnId];
            warning = string.Format("Could not parse an integer from '{0}' in column {1}", valueText, columnName);
            return false;
        }

        public static List<string> GetRequiredMSAlignColumns()
        {
            return new List<string>
            {
                MSALIGN_PRSM_ID_HEADER,
                SCAN_HEADER,
                CHARGE_HEADER,
                MSALIGN_PROTEIN_NAME_HEADER,
                MSALIGN_PROTEIN_MASS_HEADER,
                PEPTIDE_HEADER,
                E_VALUE_HEADER
            };
        }

        public static List<string> GetRequiredTopPICColumns()
        {
            return new List<string>
            {
                TopPIC_PRSM_ID_HEADER,
                TopPIC_SCAN_HEADER,
                TopPIC_CHARGE_HEADER,
                TopPIC_PROTEIN_NAME_HEADER,
                TopPIC_PROTEIN_MASS_HEADER,
                TopPIC_PEPTIDE_HEADER,
                TopPIC_E_VALUE_HEADER
            };
        }

        private bool RemoveModViaRegEx(
            KeyValuePair<Regex, KnownModIDs> modRegEx,
            ref string peptideWithMods,
            IDictionary<KnownModIDs, int> modCountsByType)
        {
            var match = modRegEx.Key.Match(peptideWithMods);

            if (!match.Success)
                return false;

            var modType = modRegEx.Value;
            if (modCountsByType.TryGetValue(modType, out var modMatchCount))
            {
                modCountsByType[modType] = modMatchCount + 1;
            }
            else
            {
                modCountsByType[modType] = 1;
            }

            // Remove the modification from the string
            var pieces = match.Groups;
            var updatedPeptideWithMods = pieces["Prefix"].Value + pieces["StartingChars"] + pieces["ModifiedResidues"].Value + pieces["RemainingChars"].Value;

            if (string.Equals(updatedPeptideWithMods, peptideWithMods))
                return false;

            peptideWithMods = string.Copy(updatedPeptideWithMods);
            return true;
        }
    }
}