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
        protected string[] monomassHeaders = { "MonoisotopicMass", "UMCMonoMW", "MonoMassIso1", "MonomassTheor"};
        protected string[] monomassCalibratedHeaders = { "MonoisotopicMassCalibrated" };
        protected string[] massErrorBeforeCalibrationHeaders = { "MassErrorBefore" ,"MassErrorInPPM"};
        protected string[] massErrorAfterCalibrationHeaders = { "MassErrorAfter" };
        protected string[] matchedMassTagIDHeaders = { "MatchedMassTagID" };
        protected string[] mzHeaders = { "MonoMZ", "UMCMZForChargeBasis" , "MZTheor"};
        protected string[] scanHeaders = { "scan", "scanClassRep" , "targetScan"};
        protected string[] scanEndHeaders = { "scanEnd", "scan_end" };
        protected string[] scanStartHeaders = { "scanStart", "scan_start" };
        protected string[] netHeaders = { "net", "NETClassRep" , "elutionTimeTheor"};
        protected string[] netErrorHeaders =  {"netError"};
        protected string[] numchromPeaksWithinTolHeaders = { "NumChromPeaksWithinTol", "ChromPeaksWithinTol" };
        protected string[] numQualitychromPeaksWithinTolHeaders = { "NumQualityChromPeaksWithinTol" };

        protected string[] validationCodeHeaders = {"ValidationCode"};
        protected string[] failureTypeHeaders = {"FailureType"};

        protected string _filename { get; set; }

        #region Constructors
        public TargetedResultFromTextImporter(string filename)
        {
            _filename = filename;

        }

        #endregion

        public override TargetedResultRepository Import()
        {
            var repos = new TargetedResultRepository();

              StreamReader reader;

            if (!File.Exists(_filename))
            {
                throw new IOException("Cannot import. File does not exist: " + _filename);
            }

            try
            {
                reader = new StreamReader(_filename);
            }
            catch (Exception ex)
            {
                throw new IOException("There was a problem importing from file " + PRISM.PathUtils.CompactPathString(_filename, 60) + ": " + ex.Message);
            }

            using (var sr = reader)
            {
                if (sr.EndOfStream)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in file " + PRISM.PathUtils.CompactPathString(_filename, 60));

                }

                var headerLine = sr.ReadLine();
                CreateHeaderLookupTable(headerLine);

                var areHeadersValid = ValidateHeaders();

                if (!areHeadersValid)
                {
                    throw new InvalidDataException("There is a problem with the column headers in file " + PRISM.PathUtils.CompactPathString(_filename, 60));
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
                        throw new InvalidDataException("In File: " + PRISM.PathUtils.CompactPathString(_filename, 60) +
                                                       "; Data in row # " + lineCounter + " is NOT valid - \n" +
                                                       "The number of columns does not match that of the header line");
                    }



                    var result = ConvertTextToDataObject(processedData);
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
            result.MonoMassCalibrated = ParseDoubleField(LookupData(rowData, monomassCalibratedHeaders));
            result.MassErrorBeforeCalibration = ParseDoubleField(LookupData(rowData, massErrorBeforeCalibrationHeaders));
            result.MassErrorAfterCalibration = ParseDoubleField(LookupData(rowData, massErrorAfterCalibrationHeaders));
            result.MonoMZ = ParseDoubleField(LookupData(rowData, mzHeaders));
            result.NET = ParseFloatField(LookupData(rowData, netHeaders));
            result.NETError = ParseFloatField(LookupData(rowData, netErrorHeaders));
            result.NumChromPeaksWithinTol = ParseIntField(LookupData(rowData, numchromPeaksWithinTolHeaders));
            result.NumQualityChromPeaksWithinTol = ParseIntField(LookupData(rowData, numQualitychromPeaksWithinTolHeaders));
            result.ScanLC = ParseIntField(LookupData(rowData, scanHeaders));
            result.ScanLCEnd = ParseIntField(LookupData(rowData, scanEndHeaders));
            result.ScanLCStart = ParseIntField(LookupData(rowData, scanStartHeaders));
            result.ErrorDescription = LookupData(rowData, failureTypeHeaders);
            result.EmpiricalFormula = LookupData(rowData, empiricalFormulaHeaders);
            result.Code = LookupData(rowData, codeHeaders);



            var validationCode = LookupData(rowData, validationCodeHeaders);
            if (string.IsNullOrEmpty(validationCode))
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
            var datasetName = Path.GetFileName(_filename).Replace("_UMCs.txt", string.Empty);

            datasetName = datasetName.Replace("_LCMSFeatures.txt", string.Empty);
            datasetName = datasetName.Replace("_TargetedFeatures.txt", string.Empty);


            return datasetName;
        }
    }
}
