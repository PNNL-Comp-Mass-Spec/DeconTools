using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Data
{
    public class IsosImporter : Importer<List<IsosResult>>
    {
        private TextReader reader;

        private int minVal;
        private int maxVal;

        private string importFilename;
        private DeconTools.Backend.Globals.MSFileType fileType;

        public IsosImporter(string importFilename, DeconTools.Backend.Globals.MSFileType filetype)
        {
            this.importFilename = importFilename;
            this.delimiter = ',';
            this.fileType = filetype;
            this.minVal = int.MinValue;
            this.maxVal = int.MaxValue;

            try
            {
                this.reader = new StreamReader(importFilename);
            }
            catch (Exception)
            {

                throw new System.IO.IOException("There was a problem reading the _isos data file\n");
            }
        }

        public IsosImporter(string importFilename, DeconTools.Backend.Globals.MSFileType filetype, int minValue, int maxValue)
            : this(importFilename, filetype)
        {
            minVal = minValue;
            maxVal = maxValue;

        }


        public override List<IsosResult> Import()
        {

            var results = new List<IsosResult>();


            if (reader.Peek() == -1)
            {
                reader.Close();
                throw new InvalidDataException("There is no data in the file reader object");

            }

            var headerLine = reader.ReadLine();
            var headers = processLine(headerLine);

            string line;
            var counter = 1;
            while (reader.Peek() > -1)
            {
                line = reader.ReadLine();
                var processedData = processLine(line);
                if (processedData.Count != headers.Count) // new line is in the wrong format... could be blank
                {
                    throw new InvalidDataException("Data in row #" + counter.ToString() + "is invalid - \nThe number of columns does not match that of the header line");
                }

                var result = convertTextToIsosResult(processedData, headers);
                if (result is UIMFIsosResult)   //GORD:  see if I can remove this conditional
                {
                    if (result.ScanSet.PrimaryScanNumber >= minVal)
                    {
                        results.Add(result);
                        if (result.ScanSet.PrimaryScanNumber > maxVal)
                        {
                            break;
                        }


                    }
                }
                else
                {
                    if (result.ScanSet.PrimaryScanNumber >= minVal && result.ScanSet.PrimaryScanNumber <= maxVal)
                    {
                        results.Add(result);
                    }
                }

                counter++;

            }

            reader.Close();
            return results;

        }

        private IsosResult convertTextToIsosResult(List<string> processedData, List<string> headers)
        {
            IsosResult result;
            if (fileType == Globals.MSFileType.PNNL_UIMF)
            {
                result = new UIMFIsosResult();

            }
            else
            {
                result = new StandardIsosResult();
            }

            result.IsotopicProfile = new IsotopicProfile();
            //result.Run = getRunFromIsosFilename(this.importFilename, this.fileType); AM commenting this out since this gives me an error

            //get scanset number from file
            var scan_num = 0;
            if (this.fileType == Globals.MSFileType.PNNL_UIMF)
            {
                var imsScanNum = parseIntField(lookup(processedData, headers, "ims_scan_num"));
                var frame_num = parseIntField(lookup(processedData, headers, "frame_num"));
                ((UIMFIsosResult)result).DriftTime = parseDoubleField(lookup(processedData, headers, "drift_time"));
                result.ScanSet = new LCScanSetIMS(frame_num);
                ((UIMFIsosResult) result).IMSScanSet = new IMSScanSet(imsScanNum);
            }
            else
            {
                scan_num = parseIntField(lookup(processedData, headers, "scan_num"));
                result.ScanSet = new ScanSet(scan_num);
            }
            

            result.IsotopicProfile.ChargeState = parseIntField(lookup(processedData, headers, "charge"));
            result.IsotopicProfile.MonoIsotopicMass = parseDoubleField(lookup(processedData, headers, "monoisotopic_mw"));
            result.IsotopicProfile.Score = parseDoubleField(lookup(processedData, headers, "fit"));
            result.IntensityAggregate = parseFloatField(lookup(processedData, headers, "abundance"));

            //result.IsotopicProfile.IntensityMostAbundant = parseFloatField(lookup(processedData, headers, "abundance"));
            


            result.IsotopicProfile.MonoPeakMZ = parseDoubleField(lookup(processedData, headers, "mz"));
            result.InterferenceScore = parseDoubleField(lookup(processedData, headers, "interference_score"));
            result.IsotopicProfile.OriginalIntensity = parseDoubleField(lookup(processedData, headers, "unsummed_intensity"));
            
            var saturationFlagString = lookup(processedData, headers, "saturation_flag");
            result.IsotopicProfile.IsSaturated = saturationFlagString == "1";
            

            var peak = new MSPeak();
            peak.Height = parseIntField(lookup(processedData, headers, "mono_abundance"));
            peak.Width = parseFloatField(lookup(processedData, headers, "fwhm"));
            peak.XValue = parseFloatField(lookup(processedData, headers, "mz"));    //mono mz isn't available from _isos file AM modification, while this is true, we still need this.
            peak.SignalToNoise = parseFloatField(lookup(processedData, headers, "signal_noise"));

            var flagString= lookup(processedData, headers, "flag");

            if (flagString == null || flagString == string.Empty)
            {
            }
            else
            {
                var flagNum = parseIntField(flagString);
                if (flagNum == 1) result.Flags.Add(new DeconTools.Backend.Core.PeakToTheLeftResultFlag());     // TODO: it'll be good to make a factory class for creating flags.
            }



            result.IsotopicProfile.Peaklist.Add(peak);

            return result;

        }

        private Run getRunFromIsosFilename(string p, DeconTools.Backend.Globals.MSFileType filetype)
        {
            var runFilename = p.Replace("_isos", "");
            var replacementExtension = "";

            switch (filetype)
            {
                case Globals.MSFileType.Undefined:
                    break;
                case Globals.MSFileType.Agilent_WIFF:
                    replacementExtension = ".WIFF";
                    break;
                case Globals.MSFileType.Agilent_D:
                    replacementExtension = ".D";
                    break;
                case Globals.MSFileType.Ascii:
                    break;
                case Globals.MSFileType.Bruker:
                    break;
                case Globals.MSFileType.Bruker_Ascii:
                    break;
                case Globals.MSFileType.Finnigan:
                    replacementExtension = ".RAW";
                    break;
                case Globals.MSFileType.ICR2LS_Rawdata:
                    break;
                case Globals.MSFileType.Micromass_Rawdata:
                    break;
                case Globals.MSFileType.MZXML_Rawdata:
                    break;
                case Globals.MSFileType.PNNL_IMS:
                    replacementExtension = ".IMF";
                    break;
                case Globals.MSFileType.PNNL_UIMF:
                    replacementExtension = ".UIMF";
                    break;
                case Globals.MSFileType.SUNEXTREL:
                    break;
                default:
                    replacementExtension = ".undefined";
                    break;
            }

            runFilename.Replace(".csv", replacementExtension);

            var factory = new RunFactory();
            Run run;
            try
            {
                run = factory.CreateRun(runFilename);


            }
            catch (Exception)
            {

                run = null;
            }

            return run;

        }
    }
}
