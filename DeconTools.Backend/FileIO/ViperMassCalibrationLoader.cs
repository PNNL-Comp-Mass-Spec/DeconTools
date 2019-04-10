
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Data;
using DeconTools.Utilities;

namespace DeconTools.Backend.FileIO
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



        public ViperMassCalibrationData ImportMassCalibrationData()
        {

            var viperMassCalibrationData = new ViperMassCalibrationData();


            var massCalibrationDataItems = new List<ViperMassCalibrationDataItem>();

            using (var reader=new StreamReader(Filename))
            {
                var headerLine = reader.ReadLine();

                Check.Require(headerLine == "MassErrorPPM\tCount\tSmoothed_Count\tComment","Error reading Viper's mass calibration data. Header line is weird. Header= " + headerLine);

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    var parsedLine = line.Split('\t');

                    var rowData = new ViperMassCalibrationDataItem();
                    if (parsedLine.Length>2)
                    {
                        rowData.MassErrorPpm = double.Parse(parsedLine[0]);

                        rowData.Count = int.Parse(parsedLine[1]);

                        rowData.SmoothedCount = double.Parse(parsedLine[2]);
                    }

                    if (parsedLine.Length>3)
                    {
                        rowData.Comment = parsedLine[3];
                    }

                    massCalibrationDataItems.Add(rowData);
                }

            }

            viperMassCalibrationData.SetCalibrationDataItems(massCalibrationDataItems);
            viperMassCalibrationData.ExtractKeyCalibrationValues();

            return viperMassCalibrationData;


        }



        #endregion

        #region Private Methods

        #endregion

    }
}
