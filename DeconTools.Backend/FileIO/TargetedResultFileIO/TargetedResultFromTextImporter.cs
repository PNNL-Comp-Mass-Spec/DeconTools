using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core.Results;
using System.IO;

namespace DeconTools.Backend.FileIO.TargetedResultFileIO
{
    public abstract class TargetedResultFromTextImporter : ImporterBase<TargetedResultRepository>
    {

        //note that case does not matter in the header
        protected string[] datasetHeaders = { "dataset" };
        protected string[] chargeStateHeaders = { "chargestate", "z", "charge_state", "ClassStatsChargeBasis" };
        protected string[] fitScoreHeaders = { "fitScore", "UMCAverageFit" };
        protected string[] intensityRepHeaders = { "intensityRep", "intensity", "abundance", "UMCAbundance" };
        protected string[] intensityI0Headers = { "intensityI0", "i0", "UMCAbundance" };
        protected string[] iscoreHeaders = { "iscore" };
        protected string[] targetIDHeaders = { "id", "mass_tag_id", "massTagid", "targetid" };
        protected string[] monomassHeaders = { "MonoisotopicMass", "UMCMonoMW" };
        protected string[] mzHeaders = { "MonoMZ", "UMCMZForChargeBasis" };
        protected string[] scanHeaders = { "scan", "scanClassRep" };
        protected string[] scanEndHeaders = { "scanEnd", "scan_end" };
        protected string[] scanStartHeaders = { "scanStart", "scan_start" };
        protected string[] netHeaders = { "net", "NETClassRep" };
        protected string[] numchromPeaksWithinTolHeaders = { "NumChromPeaksWithinTol" };


        protected string _filename { get; set; }

        #region Constructors
        public TargetedResultFromTextImporter(string filename)
        {
            this._filename = filename;

        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

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
                        throw new InvalidDataException("Data in row #" + lineCounter.ToString() + "is invalid - \nThe number of columns does not match that of the header line");
                    }

                    TargetedResult result = ConvertTextToDataObject(processedData);
                    repos.Results.Add(result);
                    lineCounter++;

                }
                sr.Close();

            }


            return repos;
        }

        protected abstract TargetedResult ConvertTextToDataObject(List<string> processedData);

        protected virtual bool ValidateHeaders()
        {
            return true;    //TODO: actually do some validation
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
