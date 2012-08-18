using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public abstract class TargetedResultFromTextImporter : ImporterBase<TargetedResultRepository>
    {

        //note that case does not matter in the header
        protected string[] datasetHeaders = { "dataset" };
        protected string[] chargeStateHeaders = { "chargestate", "z", "charge_state", "ClassStatsChargeBasis" };
        protected string[] empiricalFormulaHeaders = { "formula", "empirical_formula", "empiricalFormula", "molecular_formula", "molecularFormula" };
        protected string[] codeHeaders = { "code", "sequence" };
        protected string[] fitScoreHeaders = { "fitScore", "UMCAverageFit","iso1fit" };
        protected string[] intensityRepHeaders = { "intensityRep", "intensity", "abundance", "UMCAbundance","AbundanceIso1" };
        protected string[] intensityI0Headers = { "intensityI0", "i0", "UMCAbundance" };
        protected string[] iscoreHeaders = { "iscore", "iscore1" };
        protected string[] targetIDHeaders = { "id", "mass_tag_id", "massTagid", "targetid", "mtid" };
        protected string[] monomassHeaders = { "MonoisotopicMass", "UMCMonoMW", "MonoMassIso1" };
        protected string[] monomassCalibratedHeaders = { "MonoisotopicMassCalibrated" };
        protected string[] massErrorHeaders = { "MassErrorInPPM" };
        protected string[] matchedMassTagIDHeaders = { "MatchedMassTagID" };
        protected string[] mzHeaders = { "MonoMZ", "UMCMZForChargeBasis" };
        protected string[] scanHeaders = { "scan", "scanClassRep" };
        protected string[] scanEndHeaders = { "scanEnd", "scan_end" };
        protected string[] scanStartHeaders = { "scanStart", "scan_start" };
        protected string[] netHeaders = { "net", "NETClassRep" };
        protected string[] netErrorHeaders =  {"netError"};
        protected string[] numchromPeaksWithinTolHeaders = { "NumChromPeaksWithinTol", "ChromPeaksWithinTol" };
        protected string[] numQualitychromPeaksWithinTolHeaders = { "NumQualityChromPeaksWithinTol" };

        protected string[] validationCodeHeaders = {"ValidationCode"};

        protected string _filename { get; set; }

        #region Constructors
        public TargetedResultFromTextImporter(string filename)
        {
            this._filename = filename;

        }

        #endregion

        public override TargetedResultRepository Import()
        {
            TargetedResultRepository repos = new TargetedResultRepository();

              StreamReader reader;

            try
            {
                reader = new StreamReader(this._filename);
            }
            catch (Exception)
            {
                throw new System.IO.IOException("There was a problem importing from the file.");
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
                int lineCounter = 1;   //used for tracking which line is being processed. 

                //read and process each line of the file
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();
                    List<string> processedData = ProcessLine(line);

                    //ensure that processed line is the same size as the header line
                    if (processedData.Count != m_columnHeaders.Count)
                    {
                        throw new InvalidDataException("In File: " + Path.GetFileName(_filename)+   "; Data in row # " + lineCounter.ToString() + " is NOT valid - \nThe number of columns does not match that of the header line");
                    }

                    TargetedResultDTO result = ConvertTextToDataObject(processedData);
                    repos.Results.Add(result);
                    lineCounter++;

                }
                sr.Close();

            }


            return repos;
        }

        protected abstract TargetedResultDTO ConvertTextToDataObject(List<string> processedData);

        protected virtual bool ValidateHeaders()
        {
            return true;    //TODO: actually do some validation
        }


        protected virtual void GetBasicResultDTOData(List<string> rowData, TargetedResultDTO result)
        {
            result.DatasetName = LookupData(rowData, datasetHeaders);
            result.ChargeState = ParseIntField(LookupData(rowData, chargeStateHeaders));
            result.FitScore = ParseFloatField(LookupData(rowData, fitScoreHeaders));
            result.Intensity = ParseFloatField(LookupData(rowData, intensityRepHeaders));
            result.IntensityI0 = ParseFloatField(LookupData(rowData, intensityI0Headers));
            result.IScore = ParseFloatField(LookupData(rowData, iscoreHeaders));
            result.TargetID = ParseLongField(LookupData(rowData, targetIDHeaders));
            result.MonoMass = ParseDoubleField(LookupData(rowData, monomassHeaders));
            result.MassErrorInPPM = ParseDoubleField(LookupData(rowData, massErrorHeaders));
            result.MonoMZ = ParseDoubleField(LookupData(rowData, mzHeaders));
            result.NET = ParseFloatField(LookupData(rowData, netHeaders));
            result.NETError = ParseFloatField(LookupData(rowData, netErrorHeaders));
            result.NumChromPeaksWithinTol = ParseIntField(LookupData(rowData, numchromPeaksWithinTolHeaders));
            result.NumQualityChromPeaksWithinTol = ParseIntField(LookupData(rowData, numQualitychromPeaksWithinTolHeaders));
            result.ScanLC = ParseIntField(LookupData(rowData, scanHeaders));
            result.ScanLCEnd = ParseIntField(LookupData(rowData, scanEndHeaders));
            result.ScanLCStart = ParseIntField(LookupData(rowData, scanStartHeaders));

            result.EmpiricalFormula = LookupData(rowData, empiricalFormulaHeaders);
            result.Code = LookupData(rowData, codeHeaders);
            


            string validationCode = LookupData(rowData, validationCodeHeaders);
            if (String.IsNullOrEmpty(validationCode))
            {
                result.ValidationCode = ValidationCode.None;
                
            }
            else
            {
                if (Enum.IsDefined(typeof(ValidationCode),validationCode))
                {
                    result.ValidationCode = (ValidationCode)Enum.Parse(typeof (ValidationCode), validationCode, true);

                }
                else
                {
                    result.ValidationCode = ValidationCode.None;
                }
            }

            
        }


        protected string TryGetDatasetNameFromFileName()
        {
            string datasetName = Path.GetFileName(_filename).Replace("_UMCs.txt", String.Empty);

            datasetName = datasetName.Replace("_LCMSFeatures.txt", String.Empty);
            datasetName = datasetName.Replace("_TargetedFeatures.txt", String.Empty);


            return datasetName;
        }
    }
}
