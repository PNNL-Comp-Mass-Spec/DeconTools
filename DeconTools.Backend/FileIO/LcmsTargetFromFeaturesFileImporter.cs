using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{

    public class LcmsTargetFromFeaturesFileImporter : ImporterBase<TargetCollection>
    {
        private string _fileName;

        public LcmsTargetFromFeaturesFileImporter(string fileName)
        {
            _fileName = fileName;
        }


        private string[] datasetHeaders = {"dataset"};
        private string[] chargeStateHeaders = { "chargestate", "z", "charge_state", "ClassStatsChargeBasis", "Charge", "Class_Stats_Charge_Basis" };
        private string[] chargeStateLowerHeaders = {"ChargeStateMin"};
        private string[] chargeStateUpperHeaders = {"ChargeStateMax"};
        private string[] targetIDHeaders = {"index", "UMCIndex","TargetID", "FeatureID", "UMC_Ind"};
        private string[] featureToMassTagIDHeaders = { "MassTagID", "MatchedMassTagID", "ReferenceID", "Mass_Tag_ID" };
        private string[] empiricalFormulaHeaders= {"formula","empirical_formula","empiricalFormula","molecular_formula","molecularFormula" };
        private string[] monomassHeaders = {"MonoisotopicMass", "UMCMonoMW", "MonoMassIso1"};
       
        private string[] mzHeaders = {"MonoMZ", "UMCMZForChargeBasis"};
        private string[] scanHeaders = { "scan", "scanClassRep", "Scan_Max_Abundance","ScanNum" };
        private string[] scanEndHeaders = {"scanEnd", "scan_end"};
        private string[] scanStartHeaders = {"scanStart", "scan_start"};
        private string[] netHeaders = {"net", "NETClassRep", "ElutionTime"};
        private string[] peptideSequenceHeaders = {"peptide", "peptidesequence", "sequence", "code"};

        public override TargetCollection Import()
        {
            var repos = new TargetCollection();

            StreamReader reader;

            try
            {
                reader = new StreamReader(_fileName);
            }
            catch (Exception)
            {
                throw new IOException("There was a problem importing from the file.");
            }

            using (var sr = reader)
            {
                if (sr.EndOfStream)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in the file we are trying to read.");

                }

                var headerLine = sr.ReadLine();
                CreateHeaderLookupTable(headerLine);

                var areHeadersValid = ValidateHeaders();

                if (!areHeadersValid)
                {
                    throw new InvalidDataException("There is a problem with the column headers.");
                }


                var lineCounter = 1;   //used for tracking which line is being processed. 

                //read and process each line of the file
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != m_columnHeaders.Count)
                    {
                        throw new InvalidDataException("Data in row #" + lineCounter.ToString() + "is invalid - \nThe number of columns does not match that of the header line");
                    }

                    var target = ConvertTextToDataObject(processedData);
                    repos.TargetList.Add(target);
                    lineCounter++;

                }
                sr.Close();

            }


            return repos;
        }

        private bool ValidateHeaders()
        {
            //TODO: actually validate!
            return true;
        }

        private TargetBase ConvertTextToDataObject(List<string> processedData)
        {
            var target = new LcmsFeatureTarget();
            target.DatabaseName = LookupData(processedData, datasetHeaders);
            target.ID = ParseIntField(LookupData(processedData, targetIDHeaders));
            target.ChargeState = (short)ParseIntField(LookupData(processedData, chargeStateHeaders));
            target.ChargeStateTargets = new List<int>();
            var lowerChargeState = ParseIntField(LookupData(processedData, chargeStateLowerHeaders));
            var upperChargeState = ParseIntField(LookupData(processedData, chargeStateUpperHeaders));

            if (lowerChargeState == -1 || upperChargeState == -1)
            {
                target.ChargeStateTargets.Add(target.ChargeState);
            }
            else
            {
                for (var i = lowerChargeState; i <= upperChargeState; i++)
                {
                    target.ChargeStateTargets.Add(i);

                }
            }

            target.ScanLCTarget = ParseIntField(LookupData(processedData, scanHeaders));
            target.ElutionTimeUnit = Globals.ElutionTimeUnit.ScanNum;

            target.MonoIsotopicMass = ParseDoubleField(LookupData(processedData, monomassHeaders));
            target.MZ = target.MonoIsotopicMass/target.ChargeState + Globals.PROTON_MASS;

            target.FeatureToMassTagID = ParseIntField(LookupData(processedData, featureToMassTagIDHeaders));
            target.EmpiricalFormula = LookupData(processedData, empiricalFormulaHeaders,string.Empty);
            target.Code = LookupData(processedData, peptideSequenceHeaders,string.Empty);
            
            //UMCIndex	ScanStart	ScanEnd	ScanClassRep	
            //NETClassRep	UMCMonoMW	UMCMWStDev	UMCMWMin	
            //UMCMWMax	UMCAbundance	ClassStatsChargeBasis
            //	ChargeStateMin	ChargeStateMax	UMCMZForChargeBasis	
            //UMCMemberCount	UMCMemberCountUsedForAbu	UMCAverageFit	
            //MassShiftPPMClassRep	PairIndex	PairMemberType	ExpressionRatio	//
            //ExpressionRatioStDev	ExpressionRatioChargeStateBasisCount	
            //ExpressionRatioMemberBasisCount	MultiMassTagHitCount	
            //MassTagID	MassTagMonoMW	MassTagNET	MassTagNETStDev	SLiC Score	
            //DelSLiC	MemberCountMatchingMassTag	IsInternalStdMatch	PeptideProphetProbability	Peptide

            return target;
        }
    }
}
