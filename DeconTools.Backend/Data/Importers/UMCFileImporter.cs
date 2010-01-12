using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.IO;

namespace DeconTools.Backend.Data
{
    public class UMCFileImporter : Importer<UMCCollection>
    {
        StreamReader reader;

        #region Constructors
        public UMCFileImporter(string filename, char delimiter)
        {
            this.delimiter = delimiter;
            this.filename = filename;

            try
            {
                this.reader = new StreamReader(filename);
            }
            catch (Exception)
            {

                throw new System.IO.IOException("There was a problem reading the UMC data file");
            }
        }
        #endregion

        #region Properties
        private string filename;

        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Import(UMCCollection umcCollection)
        {
            umcCollection.UMCList= getUMCs();
        }

        private List<UMC> getUMCs()
        {
            List<UMC> umcList = new List<UMC>();

            using (StreamReader sr = reader)
            {
                if (sr.Peek() == -1)
                {
                    sr.Close();
                    throw new InvalidDataException("There is no data in the UMC data object");

                }
                string headerLine = sr.ReadLine();
                List<string> headers = processLine(headerLine);

                if (!validateHeaders(headers))
                {
                    throw new InvalidDataException("There is a problem with the column headers in the UMC data");

                }


                string line;
                int counter = 1;
                while (sr.Peek() > -1)
                {
                    line = sr.ReadLine();
                    List<string> processedData = processLine(line);
                    if (processedData.Count != headers.Count) // new line is in the wrong format... could be blank
                    {
                        throw new InvalidDataException("Data in UMC row #" + counter.ToString() + "is invalid - \nThe number of columns does not match that of the header line");
                    }

                    UMC row = convertTextToUMCData(processedData, headers);
                    umcList.Add(row);
                    counter++;

                }
                sr.Close();

            }
            return umcList;
        }

        private UMC convertTextToUMCData(List<string> processedData, List<string> headers)
        {
            UMC row = new UMC();

            row.UMCIndex = parseIntField(lookup(processedData, headers, "UMCIndex"));
            row.ScanStart = parseIntField(lookup(processedData, headers, "ScanStart"));
            row.ScanEnd = parseIntField(lookup(processedData, headers, "ScanEnd"));
            row.NETClassRep = parseDoubleField(lookup(processedData, headers, "NETClassRep"));
            row.UMCMonoMW = parseDoubleField(lookup(processedData, headers, "UMCMonoMW"));
            row.PairIndex = parseIntField(lookup(processedData, headers, "PairIndex"));
            row.PairMemberType = parseIntField(lookup(processedData, headers, "PairMemberType"));
            row.UMCIndex = parseIntField(lookup(processedData, headers, "UMCIndex"));
            row.ScanStart = parseIntField(lookup(processedData, headers, "ScanStart"));
            row.ScanEnd = parseIntField(lookup(processedData, headers, "ScanEnd"));
            row.ScanClassRep = parseIntField(lookup(processedData, headers, "ScanClassRep"));
            row.NETClassRep = parseDoubleField(lookup(processedData, headers, "NETClassRep"));
            row.UMCMonoMW = parseDoubleField(lookup(processedData, headers, "UMCMonoMW"));
            row.UMCMWStDev = parseDoubleField(lookup(processedData, headers, "UMCMWStDev"));
            row.UMCMWMin = parseDoubleField(lookup(processedData, headers, "UMCMWMin"));
            row.UMCMWMax = parseDoubleField(lookup(processedData, headers, "UMCMWMax"));
            row.UMCAbundance = parseDoubleField(lookup(processedData, headers, "UMCAbundance"));
            row.ClassStatsChargeBasis = parseShortField(lookup(processedData, headers, "ClassStatsChargeBasis"));
            row.ChargeStateMin = parseShortField(lookup(processedData, headers, "ChargeStateMin"));
            row.ChargeStateMax = parseShortField(lookup(processedData, headers, "ChargeStateMax"));
            row.UMCMZForChargeBasis = parseDoubleField(lookup(processedData, headers, "UMCMZForChargeBasis"));
            row.UMCMemberCount = parseIntField(lookup(processedData, headers, "UMCMemberCount"));
            row.UMCMemberCountUsedForAbu = parseIntField(lookup(processedData, headers, "UMCMemberCountUsedForAbu"));
            row.UMCAverageFit = parseDoubleField(lookup(processedData, headers, "UMCAverageFit"));
            row.PairIndex = parseIntField(lookup(processedData, headers, "PairIndex"));
            row.ExpressionRatio = parseDoubleField(lookup(processedData, headers, "ExpressionRatio"));
            row.ExpressionRatioStDev = parseDoubleField(lookup(processedData, headers, "ExpressionRatioStDev"));
            row.ExpressionRatioChargeStateBasisCount = parseIntField(lookup(processedData, headers, "ExpressionRatioChargeStateBasisCount"));
            row.ExpressionRatioMemberBasisCount = parseIntField(lookup(processedData, headers, "ExpressionRatioMemberBasisCount"));
            row.MultiMassTagHitCount = parseShortField(lookup(processedData, headers, "MultiMassTagHitCount"));
            row.MassTagID = parseIntField(lookup(processedData, headers, "MassTagID"));
            row.MassTagMonoMW = parseDoubleField(lookup(processedData, headers, "MassTagMonoMW"));
            row.MassTagNET = parseDoubleField(lookup(processedData, headers, "MassTagNET"));
            row.MassTagNETStDev = parseDoubleField(lookup(processedData, headers, "MassTagNETStDev"));
            row.SLiCScore = parseDoubleField(lookup(processedData, headers, "SLiC Score"));
            row.DelSLiC = parseDoubleField(lookup(processedData, headers, "DelSLiC"));
            row.MemberCountMatchingMassTag = parseIntField(lookup(processedData, headers, "MemberCountMatchingMassTag"));
            row.IsInternalStdMatch = parseBoolField(lookup(processedData, headers, "IsInternalStdMatch"));
            row.PeptideProphetProbability = parseDoubleField(lookup(processedData, headers, "PeptideProphetProbability"));
            row.Peptide = lookup(processedData, headers, "Peptide");





            return row;
        }

        private bool validateHeaders(List<string> headers)
        {
            if (headers.Count < 10) return false;
            if (headers[0] != "UMCIndex") return false;
            if (headers[1] != "ScanStart") return false;
            return true;
        }
    }
}
