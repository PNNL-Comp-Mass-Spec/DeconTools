using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Utilities.IqCodeParser;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class MSAlignIqTargetImporter : IqTargetImporter
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

        #endregion

        #region public methods

        public MSAlignIqTargetImporter(string filename)
        {
            Filename = filename;
        }


        public override List<IqTarget> Import()
        {
            var allTargets = new List<IqTarget>();

            StreamReader reader;

            if (!File.Exists(Filename))
            {
                throw new IOException("Cannot import. File does not exist: " + Filename);
            }

            try
            {
                reader = new StreamReader(Filename);
            }
            catch (Exception ex)
            {
                throw new IOException("There was a problem importing from file " + PRISM.PathUtils.CompactPathString(Filename, 60) + ": " + ex.Message);
            }

            //Sequence is the key and processed line is the value
            var parentTargetGroup = new Dictionary<string, List<List<string>>>();

            using (var sr = reader)
            {
                if (sr.EndOfStream)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in file " + PRISM.PathUtils.CompactPathString(Filename, 60));

                }

                var headerLine = sr.ReadLine();
                CreateHeaderLookupTable(headerLine);

                var lineCounter = 1;   //used for tracking which line is being processed.

                //read and process each line of the file
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != m_columnHeaders.Count)
                    {
                        throw new InvalidDataException("In File: " + PRISM.PathUtils.CompactPathString(Filename, 60) +
                                                       "; Data in row # " + lineCounter + " is NOT valid - \n" +
                                                       "The number of columns does not match that of the header line");
                    }

                    //Implement EValueCutoff
                    var EValueCutoff = 1.0E-3;
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

            var target_id = 1;

            foreach (var keyValuePair in parentTargetGroup)
            {
                IqTarget target = CreateParentTarget(keyValuePair.Value);
                target.ID = target_id;
                allTargets.Add(target);
                target_id++;
            }
            return allTargets;
        }

        #endregion

        protected override IqTarget ConvertTextToIqTarget(List<string> processedRowOfText)
        {
            var child = new IqChargeStateTarget
            {
                ChargeState = ParseIntField(processedRowOfText, ChargeHeader),
                ObservedScan = ParseIntField(processedRowOfText, ScansHeader),
                AlternateID = ParseIntField(processedRowOfText, PRSMIdHeader),
                DatabaseReference = ParseStringField(processedRowOfText, ProteinNameHeader)
            };
            return child;
        }

        protected TopDownIqTarget CreateParentTarget(List<List<string>> processedGroup)
        {
            var target = new TopDownIqTarget();
            var parser = new IqCodeParser();

            target.DatabaseReference = ParseStringField(processedGroup[0], ProteinNameHeader);
            target.Code = ParseStringField(processedGroup[0] ,PeptideHeader);
            target.EmpiricalFormula = parser.GetEmpiricalFormulaFromSequence(target.Code);
            target.PTMList = parser.GetPTMList(target.Code);
            target.MonoMassTheor = ParseDoubleField(processedGroup[0], AdjustedPrecursorMassHeader);
            var children = new List<IqTarget>();

            foreach (var line in processedGroup)
            {
                children.Add(ConvertTextToIqTarget(line));
            }

            target.AddTargetRange(children);
            return target;
        }

    }
}
