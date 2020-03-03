using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Data
{
    public class UMCFileImporter : Importer<UMCCollection>
    {
        private readonly StreamReader reader;

        #region Constructors
        public UMCFileImporter(string filePath, char colDelimiter)
        {
            delimiter = colDelimiter;
            FilePath = filePath;

            try
            {
                reader = new StreamReader(filePath);
            }
            catch (Exception ex)
            {

                throw new IOException("There was a problem reading UMC data file " + FilePath + ": " + ex.Message);
            }
        }
        #endregion

        #region Properties

        public string FilePath { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override UMCCollection Import()
        {
            var umcCollection = new UMCCollection {
                UMCList = getUMCs()
            };

            return umcCollection;

        }

        private List<UMC> getUMCs()
        {
            var umcList = new List<UMC>();

            using (var sr = reader)
            {
                if (sr.EndOfStream)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in UMC data file " + FilePath);

                }
                var headerLine = sr.ReadLine();
                var headers = processLine(headerLine);

                if (!validateHeaders(headers))
                {
                    throw new InvalidDataException("There is a problem with the column headers in UMC data file " + FilePath);

                }


                var counter = 1;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var processedData = processLine(line);
                    if (processedData.Count != headers.Count) // new line is in the wrong format... could be blank
                    {
                        throw new InvalidDataException("Data in UMC row #" + counter + "is invalid - \nThe number of columns does not match that of the header line; see " + FilePath);
                    }

                    var row = convertTextToUMCData(processedData, headers);
                    umcList.Add(row);
                    counter++;

                }
                sr.Close();

            }
            return umcList;
        }

        private UMC convertTextToUMCData(List<string> processedData, List<string> headers)
        {
            var row = new UMC
            {
                UMCIndex = parseIntField(lookup(processedData, headers, "UMCIndex")),
                ScanStart = parseIntField(lookup(processedData, headers, "ScanStart")),
                ScanEnd = parseIntField(lookup(processedData, headers, "ScanEnd")),
                NETClassRep = parseDoubleField(lookup(processedData, headers, "NETClassRep")),
                UMCMonoMW = parseDoubleField(lookup(processedData, headers, "UMCMonoMW")),
                PairIndex = parseIntField(lookup(processedData, headers, "PairIndex")),
                PairMemberType = parseIntField(lookup(processedData, headers, "PairMemberType")),
                ScanClassRep = parseIntField(lookup(processedData, headers, "ScanClassRep")),
                UMCMWStDev = parseDoubleField(lookup(processedData, headers, "UMCMWStDev")),
                UMCMWMin = parseDoubleField(lookup(processedData, headers, "UMCMWMin")),
                UMCMWMax = parseDoubleField(lookup(processedData, headers, "UMCMWMax")),
                UMCAbundance = parseDoubleField(lookup(processedData, headers, "UMCAbundance")),
                ClassStatsChargeBasis = parseShortField(lookup(processedData, headers, "ClassStatsChargeBasis")),
                ChargeStateMin = parseShortField(lookup(processedData, headers, "ChargeStateMin")),
                ChargeStateMax = parseShortField(lookup(processedData, headers, "ChargeStateMax")),
                UMCMZForChargeBasis = parseDoubleField(lookup(processedData, headers, "UMCMZForChargeBasis")),
                UMCMemberCount = parseIntField(lookup(processedData, headers, "UMCMemberCount")),
                UMCMemberCountUsedForAbu = parseIntField(lookup(processedData, headers, "UMCMemberCountUsedForAbu")),
                UMCAverageFit = parseDoubleField(lookup(processedData, headers, "UMCAverageFit")),
                ExpressionRatio = parseDoubleField(lookup(processedData, headers, "ExpressionRatio")),
                ExpressionRatioStDev = parseDoubleField(lookup(processedData, headers, "ExpressionRatioStDev")),
                ExpressionRatioChargeStateBasisCount = parseIntField(lookup(processedData, headers, "ExpressionRatioChargeStateBasisCount")),
                ExpressionRatioMemberBasisCount = parseIntField(lookup(processedData, headers, "ExpressionRatioMemberBasisCount")),
                MultiMassTagHitCount = parseShortField(lookup(processedData, headers, "MultiMassTagHitCount")),
                MassTagID = parseIntField(lookup(processedData, headers, "MassTagID")),
                MassTagMonoMW = parseDoubleField(lookup(processedData, headers, "MassTagMonoMW")),
                MassTagNET = parseDoubleField(lookup(processedData, headers, "MassTagNET")),
                MassTagNETStDev = parseDoubleField(lookup(processedData, headers, "MassTagNETStDev")),
                StacScore = parseDoubleField(lookup(processedData, headers, "STAC Score")),
                StacUniquenessProbability = parseDoubleField(lookup(processedData, headers, "Uniqueness Probability")),
                SLiCScore = parseDoubleField(lookup(processedData, headers, "SLiC Score")),
                DelSLiC = parseDoubleField(lookup(processedData, headers, "DelSLiC")),
                MemberCountMatchingMassTag = parseIntField(lookup(processedData, headers, "MemberCountMatchingMassTag")),
                IsInternalStdMatch = parseBoolField(lookup(processedData, headers, "IsInternalStdMatch")),
                PeptideProphetProbability = parseDoubleField(lookup(processedData, headers, "PeptideProphetProbability")),
                Peptide = lookup(processedData, headers, "Peptide")
            };

            return row;
        }

        private bool validateHeaders(IReadOnlyList<string> headers)
        {
            if (headers.Count < 10) return false;
            if (headers[0] != "UMCIndex") return false;
            if (headers[1] != "ScanStart") return false;
            return true;
        }
    }
}
