using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiAlignEngine.Alignment;
using System.IO;

namespace DeconTools.Backend.FileIO
{
    public class MassAlignmentInfoFromTextImporter : ImporterBase<clsAlignmentFunction>
    {

        List<MassAndTimePPMCorrectionItem> _massAndTimeCorrectionData = new List<MassAndTimePPMCorrectionItem>();
        private string[] mzHeaders = { "mz" };
        private string[] mzPPMCorrectionHeaders = { "mzPPMCorrection" };
        private string[] scanHeaders= { "scan" };
        private string[] scanPPMCorrectionHeaders = { "scanPPMCorrection" };
        private string _filename;

        private class MassAndTimePPMCorrectionItem
        {
            internal float mz;
            internal float mzPPMCorrection;
            internal float scan;
            internal float scanPPMCorrection;
           

            public MassAndTimePPMCorrectionItem(float mz, float mzPPMCorrection, float scan, float scanPPMCorrection)
            {
                this.mz = mz;
                this.mzPPMCorrection = mzPPMCorrection;
                this.scan = scan;
                this.scanPPMCorrection = scanPPMCorrection;
            }
        }


        #region Constructors
        public MassAlignmentInfoFromTextImporter(string filename)
        {
            _filename = filename;
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override clsAlignmentFunction Import()
        {
            clsAlignmentFunction alignmentInfo = new clsAlignmentFunction(enmCalibrationType.HYBRID_CALIB, enmAlignmentType.NET_MASS_WARP);


            ImportIntoAlignmentInfo(alignmentInfo);
            
            return alignmentInfo;
        }

        public void ImportIntoAlignmentInfo(clsAlignmentFunction alignmentInfo)
        {
            GetMassAndTimePPMCorrectionsFromFile();

            float[] mzVals = _massAndTimeCorrectionData.Select(p => p.mz).ToArray();
            float[] mzPPMCorrVals = _massAndTimeCorrectionData.Select(p => p.mzPPMCorrection).ToArray();
            float[] scanVals = _massAndTimeCorrectionData.Select(p => p.scan).ToArray();
            float[] scanPPMCorrVals = _massAndTimeCorrectionData.Select(p => p.scanPPMCorrection).ToArray();

            alignmentInfo.marrMassFncMZInput = new float[mzVals.Length];
            alignmentInfo.marrMassFncMZPPMOutput = new float[mzVals.Length];
            alignmentInfo.marrMassFncTimeInput = new float[mzVals.Length];
            alignmentInfo.marrMassFncTimePPMOutput= new float[mzVals.Length];

            alignmentInfo.SetMassCalibrationFunctionWithMZ(ref mzVals, ref mzPPMCorrVals);
            alignmentInfo.SetMassCalibrationFunctionWithTime(ref scanVals, ref scanPPMCorrVals);

        }

        private void GetMassAndTimePPMCorrectionsFromFile()
        {
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

                    MassAndTimePPMCorrectionItem massAndTimePPMCorrItem = ConvertTextToDataObject(processedData);
                    _massAndTimeCorrectionData.Add(massAndTimePPMCorrItem);
                    lineCounter++;

                }
            }
        }

        private MassAndTimePPMCorrectionItem ConvertTextToDataObject(List<string> processedData)
        {
            float mz = ParseFloatField(LookupData(processedData, mzHeaders));
            float mzPPMCorrection = ParseFloatField(LookupData(processedData, mzPPMCorrectionHeaders));
            float scan = ParseFloatField(LookupData(processedData, scanHeaders));
            float scanPPMCorrection = ParseFloatField(LookupData(processedData, scanPPMCorrectionHeaders));

            MassAndTimePPMCorrectionItem item = new MassAndTimePPMCorrectionItem(mz, mzPPMCorrection, scan, scanPPMCorrection);
            return item;
        }

        private bool ValidateHeaders()
        {
            return true;
        }
    }
}
