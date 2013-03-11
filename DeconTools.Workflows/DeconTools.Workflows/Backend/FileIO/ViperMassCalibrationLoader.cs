using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.Data;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class ViperMassCalibrationLoader
    {


        

        #region Constructors

        public ViperMassCalibrationLoader(string filename)
        {
            Filename = filename;
        }

        
        #endregion

        #region Properties
        protected string Filename { get; set; }

        #endregion

        #region Public Methods



        public List<ViperMassCalibrationDataItem> ImportMassCalibrationData()
        {
            List<ViperMassCalibrationDataItem> calibrationData = new List<ViperMassCalibrationDataItem>();

            using (var reader=new StreamReader(Filename))
            {
                string headerLine = reader.ReadLine();

                Check.Require(headerLine == "MassErrorPPM\tCount\tSmoothed_Count\tComment","Error reading Viper's mass calibration data. Header line is weird. Header= " + headerLine);

                while (reader.Peek()!=-1)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    var parsedLine = line.Split('\t');

                    var rowData = new ViperMassCalibrationDataItem();
                    if (parsedLine.Length>2)
                    {
                        rowData.MassErrorPpm = double.Parse(parsedLine[0]);

                        rowData.Count = Int32.Parse(parsedLine[1]);

                        rowData.SmoothedCount = double.Parse(parsedLine[2]);
                    }

                    if (parsedLine.Length>3)
                    {
                        rowData.Comment = parsedLine[3];
                    }

                    calibrationData.Add(rowData);
                }

            }

            return calibrationData;


        }



        #endregion

        #region Private Methods

        #endregion

    }
}
