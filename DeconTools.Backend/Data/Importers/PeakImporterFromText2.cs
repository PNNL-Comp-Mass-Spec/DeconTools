namespace DeconTools.Backend.Data.Importers
{
    //this will replace PeakImporterFromText
    //public class PeakImporterFromText2 : PNNLOmics.Utilities.Importers.ImporterBase<List<DeconTools.Backend.DTO.MSPeakResult>>
    //{

    //    public string FileName { get; set; }

    //    public PeakImporterFromText2(string sourceFileName)
    //    {
    //        bool fileExists = File.Exists(sourceFileName);
    //        Check.Require(fileExists, string.Format("{0} failed. File does not exist.", this.Name));

    //        this.FileName = sourceFileName;
    //        this.m_delimiter = '\t';

    //    }

    //    public override List<DeconTools.Backend.DTO.MSPeakResult> Import()
    //    {
    //        List<MSPeakResult> peakList = new List<MSPeakResult>();

    //        using (StreamReader sr = new StreamReader(this.FileName))
    //        {
    //            if (sr.EndOfStream)
    //            {
    //                throw new IOException(string.Format("{0} failed. File contains no data", this.Name));
    //            }

    //            string headerLine = sr.ReadLine();
    //            m_columnHeaders = ProcessLine(headerLine);

    //            bool areHeadersValid = validateHeaders();

    //            if (!areHeadersValid)
    //            {
    //                throw new InvalidDataException("There is a problem with the column headers in the data");
    //            }

    //            string line;
    //            int lineCounter = 1;   //used for tracking which line is being processed.

    //            //read and process each line of the file
    //            while (!sr.EndOfStream)
    //            {
    //                line = sr.ReadLine();
    //                List<string> processedData = ProcessLine(line);

    //                //ensure that processed line is the same size as the header line
    //                if (processedData.Count != m_columnHeaders.Count)
    //                {
    //                    throw new InvalidDataException("Data in row #" + lineCounter.ToString() + "is invalid - \nThe number of columns does not match that of the header line");
    //                }

    //                MSPeakResult peakResult = convertTextToPeakData(processedData);
    //                peakList.Add(peakResult);

    //                //increase counter that keeps track of what line we are on... for use in error reporting.
    //                lineCounter++;

    //            }
    //            sr.Close();

    //        }

    //    }

    //    private MSPeakResult convertTextToPeakData(List<string> processedData)
    //    {
    //        MSPeakResult peakResult = new MSPeakResult();

    //        var mz = ParseDoubleField(LookupData(processedData, "mz"));
    //        MSPeak msPeak = new MSPeak(mz);

    //    }

    //    private bool validateHeaders()
    //    {
    //        //TODO: finish this.  Will assume they are valid for now
    //        return true;
    //    }
    //}
}
