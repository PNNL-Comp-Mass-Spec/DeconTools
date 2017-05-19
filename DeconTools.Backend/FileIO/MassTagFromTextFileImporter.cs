using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;


namespace DeconTools.Backend.FileIO
{
    public class MassTagFromTextFileImporter : IMassTagImporter
    {
        

        #region Constructors

        string m_filename;
        private List<string> _headers;

        public MassTagFromTextFileImporter(string filename)
        {
            if (!File.Exists(filename)) throw new System.IO.IOException("MassTagImporter failed. File doesn't exist: " + DiagnosticUtilities.GetFullPathSafe(filename));
            this.m_filename = filename;
            this.delimiter = '\t';
            
        }




        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override TargetCollection Import()
        {
            return Import(new List<int>());

        }

        public TargetCollection Import(List<int>TargetIDsToFilterOn)
        {

            var filterOnTargetIDs = TargetIDsToFilterOn != null && TargetIDsToFilterOn.Count > 0;

            var data = new TargetCollection();

            using (var reader = new StreamReader(m_filename))
            {
                var headerLine = reader.ReadLine();    //first line is the header line.   

                _headers = processLine(headerLine);

                var lineCounter = 1;
                while (reader.Peek() != -1)
                {
                    var line = reader.ReadLine();
                    lineCounter++;



                    var lineData = processLine(line);

                    PeptideTarget massTag;
                    try
                    {
                        massTag = convertTextToMassTag(lineData);


                        if (filterOnTargetIDs)
                        {
                            if (!TargetIDsToFilterOn.Contains(massTag.ID))
                            {
                                continue;

                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        var msg = "Importer failed. Error reading line: " + lineCounter.ToString() + "\nDetails: " + ex.Message;
                        throw new Exception(msg);
                    }


                    if (!massTag.ContainsMods && String.IsNullOrEmpty(massTag.EmpiricalFormula))
                    {
                        massTag.EmpiricalFormula = massTag.GetEmpiricalFormulaFromTargetCode();
                    }


                    var massTagMassInfoMissing = (Math.Abs(massTag.MonoIsotopicMass - 0) < double.Epsilon);

                    var chargeStateInfoIsAvailable = massTag.ChargeState != 0;
                    if (massTagMassInfoMissing)
                    {
                        if (!String.IsNullOrEmpty(massTag.EmpiricalFormula))
                        {
                            massTag.MonoIsotopicMass =
                                EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(
                                    massTag.EmpiricalFormula);

                            if (chargeStateInfoIsAvailable)
                            {
                                massTag.MZ = massTag.MonoIsotopicMass / massTag.ChargeState + Globals.PROTON_MASS;
                            }
                        }
                    }


                    if (!chargeStateInfoIsAvailable)
                    {
                        double minMZToConsider = 400;
                        double maxMZToConsider = 1500;

                        var targetList = new List<PeptideTarget>();

                        for (var chargeState = 1; chargeState < 100; chargeState++)
                        {
                            var calcMZ = massTag.MonoIsotopicMass / chargeState + Globals.PROTON_MASS;
                            if (calcMZ > minMZToConsider && calcMZ < maxMZToConsider)
                            {
                                var copiedMassTag = new PeptideTarget(massTag);    //we need to create multiple mass tags 
                                copiedMassTag.ChargeState = (short)chargeState;
                                copiedMassTag.MZ = calcMZ;

                                targetList.Add(copiedMassTag);
                            }

                        }

                        data.TargetList.AddRange(targetList.Take(3));

                    }
                    else
                    {

                        data.TargetList.Add(massTag);
                    }





                }


            }

            foreach (PeptideTarget peptideTarget in data.TargetList)
            {
                //bool noNormalizedElutionTimeInfoAvailable = Math.Abs(peptideTarget.NormalizedElutionTime - -1) < Single.Epsilon;
                //if (noNormalizedElutionTimeInfoAvailable)
                //{
                //    peptideTarget.NormalizedElutionTime = 0.5f;
                //}   
            }




            return data;
        }

        private PeptideTarget convertTextToMassTag(List<string> lineData)
        {

            var mt = new PeptideTarget();
            mt.ChargeState = (short)parseIntField(getValue(new string[] { "z", "charge_state" ,"charge"}, lineData, "0"));

            mt.ID = parseIntField(getValue(new string[] { "id", "targetid", "target_id", "mass_tag_id", "massTagid" }, lineData, "-1"));
            mt.Code = getValue(new string[] { "peptide", "sequence" }, lineData, "");

            var scanNum = parseIntField(getValue(new string[] { "scannum", "scan" ,"scanNum"}, lineData, "-1"));
            mt.NormalizedElutionTime = parseFloatField(getValue(new string[] { "net", "avg_ganet", "ElutionTimeTheor" }, lineData, "-1"));

            var neitherScanOrNETIsProvided = mt.NormalizedElutionTime == -1 && scanNum == -1;
            if (neitherScanOrNETIsProvided)
            {
                mt.ElutionTimeUnit = Globals.ElutionTimeUnit.NormalizedElutionTime;
                mt.NormalizedElutionTime = 0.5f;  // set the NET mid-way between 0 and 1. 
            }
            
            var useScanNum = ((int)mt.NormalizedElutionTime == -1 && scanNum != -1);
            if (useScanNum)
            {
                mt.ScanLCTarget = scanNum;
                //mt.NormalizedElutionTime = scanNum / ;
                mt.ElutionTimeUnit = Globals.ElutionTimeUnit.ScanNum;
            }



            mt.ObsCount = parseIntField(getValue(new string[] { "obs", "obscount" }, lineData, "-1"));
            mt.MonoIsotopicMass = parseDoubleField(getValue(new string[] { "mass", "monoisotopicmass", "monoisotopic_mass" }, lineData, "0"));
            mt.EmpiricalFormula = getValue(new string[] { "formula", "empirical_formula", "empiricalformula"}, lineData, "");
            mt.ModCount = parseShortField(getValue(new string[] { "modCount", "mod_count" }, lineData, "0"));
            mt.ModDescription = getValue(new string[] { "mod", "mod_description" }, lineData, "");
            if (mt.ChargeState == 0)
            {

            }
            else
            {
                mt.MZ = parseDoubleField(getValue(new string[] { "mz" }, lineData, "0"));
                if (mt.MZ == 0 || mt.MZ == double.NaN)
                {
                    mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
                }
            }

            mt.GeneReference = getValue(new string[] {"reference"}, lineData, "");

            mt.RefID = parseIntField(getValue(new string[] { "ref_id" }, lineData, "-1"));
            mt.ProteinDescription = getValue(new string[] { "description" ,"protein" }, lineData, "");

            return mt;

        }

        private string getValue(string[] possibleHeaders, List<string> lineData, string defaultVal)
        {
            foreach (var possibleHeader in possibleHeaders)
            {


                var indexOfHeader = getIndexForTableHeader(_headers, possibleHeader, true);
                var foundHeader = (indexOfHeader != -1);

                if (foundHeader)
                {
                    return lineData[indexOfHeader];
                }

            }
            return defaultVal;

        }

        private List<string> getHeaders(string headerLine)
        {
            var processedLine = headerLine.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return processedLine;
        }
    }
}
