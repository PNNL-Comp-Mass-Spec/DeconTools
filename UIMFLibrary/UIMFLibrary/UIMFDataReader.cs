/////////////////////////////////////////////////////////////////////////////////////
// This file includes a library of functions to retrieve data from a UIMF format file
// Author: Yan Shi, PNNL, December 2008
// Updates by:
//          Anuj Shaw
//          William Danielson
//          Yan Shi
//          Gordon Slysz
//          Matthew Monroe
//			Kevin Crowell
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Lzf;

namespace UIMFLibrary
{
    public class DataReader : IDisposable
    {
        #region "Constants and Enums"
            public enum FrameType
            {
                MS1 = 1,
                MS2 = 2,
                Calibration = 3,
                Prescan = 4
            }

            private const int DATASIZE = 4; //All intensities are stored as 4 byte quantities

            // No longer used: private const int MAXMZ = 5000;

            private const string BPI = "BPI";
            private const string TIC = "TIC";

        #endregion

        #region "Class-wide variables"

            private SQLiteConnection m_uimfDatabaseConnection;
			private SQLiteDataReader m_sqliteDataReader;

            // v1.2 prepared statements
			private SQLiteCommand m_getCountPerSpectrumCommand;
			private SQLiteCommand m_getCountPerFrameCommand;
			private SQLiteCommand m_getFileBytesCommand;
			private SQLiteCommand m_getFrameNumbers;
			private SQLiteCommand m_getFrameParametersCommand;
			private SQLiteCommand m_getFramesAndScanByDescendingIntensityCommand;
			private SQLiteCommand m_getSpectrumCommand;
			private SQLiteCommand m_sumVariableScansPerFrameCommand;
			private SQLiteCommand m_preparedStatement;

			private MZ_Calibrator m_mzCalibration;
			
			private Dictionary<int, FrameParameters> m_frameParametersCache;
            private GlobalParameters m_globalParameters;
            private double[] m_calibrationTable;
			private string m_uimfFilePath;
        
        
    		private int m_frameTypeMs;

			private static int m_errMessageCounter;


        #endregion

        #region "Properties"

        public bool PressureIsMilliTorr { get; set; }


		public double TenthsOfNanoSecondsPerBin
			{
				get { return m_globalParameters.BinWidth * 10.0; }
			}

        #endregion

		public DataReader(string fileName)
		{
			m_errMessageCounter = 0;
			m_calibrationTable = new double[0];

            FileSystemInfo uimfFileInfo = new FileInfo(fileName);

			if (uimfFileInfo.Exists)
			{
				string connectionString = "Data Source = " + uimfFileInfo.FullName + "; Version=3; DateTimeFormat=Ticks;";
				m_uimfDatabaseConnection = new SQLiteConnection(connectionString);

				try
				{
					m_uimfDatabaseConnection.Open();
					m_uimfFilePath = uimfFileInfo.FullName;

					// Populate the global parameters object
					m_globalParameters = GetGlobalParametersFromTable(m_uimfDatabaseConnection);

					// Initialize the frame parameters cache
					m_frameParametersCache = new Dictionary<int, FrameParameters>(m_globalParameters.NumFrames);

					LoadPrepStmts();

					// Lookup whether the pressure columns are in torr or mTorr
					DeterminePressureUnits();

					// Find out if the MS1 Frames are labeled as 0 or 1.
					DetermineFrameTypes();
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to open UIMF file: " + ex);
				}
			}
			else
			{
				throw new FileNotFoundException("UIMF file not found: " + uimfFileInfo.FullName);
			}
		}

		public int[][] AccumulateFrameData(int frameNumber, bool flagTOF, int startScan, int startBin, int minMZBin, int maxMZBin, int[][] frameData, int yCompression)
		{
			int dataWidth = frameData.Length;
			int dataHeight = frameData[0].Length;

			byte[] compressedBinIntensity;
			byte[] streamBinIntensity = new byte[m_globalParameters.Bins * 4];
			int endBin;

			if (yCompression > 0)
			{
				endBin = startBin + (dataHeight*yCompression);
			}
			else if (yCompression < 0)
			{
				endBin = startBin + dataHeight - 1;
			}
			else
			{
				throw new Exception("UIMFLibrary accumulate_PlotData: Compression == 0");
			}

			// Create a calibration lookup table -- for speed
			m_calibrationTable = new double[dataHeight];
			if (flagTOF)
			{
				for (int i = 0; i < dataHeight; i++)
				{
					m_calibrationTable[i] = startBin + (i * (double) (endBin - startBin) / dataHeight);
				}
			}
			else
			{
				double mzMin = m_mzCalibration.TOFtoMZ((float)((startBin / m_globalParameters.BinWidth) * TenthsOfNanoSecondsPerBin));
				double mzMax = m_mzCalibration.TOFtoMZ((float)((endBin / m_globalParameters.BinWidth) * TenthsOfNanoSecondsPerBin));

				for (int i = 0; i < dataHeight; i++)
				{
					m_calibrationTable[i] = m_mzCalibration.MZtoTOF(mzMin + (i*(mzMax - mzMin)/dataHeight))*m_globalParameters.BinWidth/TenthsOfNanoSecondsPerBin;
				}
			}

			// This function extracts intensities from selected scans and bins in a single frame 
			// and returns a two-dimensional array intensities[scan][bin]
			// frameNum is mandatory and all other arguments are optional
			m_preparedStatement = m_uimfDatabaseConnection.CreateCommand();
			m_preparedStatement.CommandText = "SELECT ScanNum, Intensities FROM Frame_Scans WHERE FrameNum = " + frameNumber + " AND ScanNum >= " + startScan + " AND ScanNum <= " + (startScan + dataWidth - 1);

			m_sqliteDataReader = m_preparedStatement.ExecuteReader();
			m_preparedStatement.Dispose();

			// accumulate the data into the plot_data
			if (yCompression < 0)
			{
				//MessageBox.Show(start_bin.ToString() + " " + end_bin.ToString());

				for (int scansData = 0; ((scansData < dataWidth) && m_sqliteDataReader.Read()); scansData++)
				{
					int currentScan = Convert.ToInt32(m_sqliteDataReader["ScanNum"]) - startScan;
					compressedBinIntensity = (byte[])(m_sqliteDataReader["Intensities"]);

					if (compressedBinIntensity.Length == 0)
						continue;

					int indexCurrentBin = 0;
					int decompressLength = LZFCompressionUtil.Decompress(ref compressedBinIntensity, compressedBinIntensity.Length, ref streamBinIntensity, m_globalParameters.Bins * 4);

					for (int binData = 0; (binData < decompressLength) && (indexCurrentBin <= endBin); binData += 4)
					{
						int intBinIntensity = BitConverter.ToInt32(streamBinIntensity, binData);

						if (intBinIntensity < 0)
						{
							indexCurrentBin += -intBinIntensity;   // concurrent zeros
						}
						else if ((indexCurrentBin < minMZBin) || (indexCurrentBin < startBin))
							indexCurrentBin++;
						else if (indexCurrentBin > maxMZBin)
							break;
						else
						{
							frameData[currentScan][indexCurrentBin - startBin] += intBinIntensity;
							indexCurrentBin++;
						}
					}
				}
			}
			else    // each pixel accumulates more than 1 bin of data
			{
				for (int scansData = 0; ((scansData < dataWidth) && m_sqliteDataReader.Read()); scansData++)
				{
					int currentScan = Convert.ToInt32(m_sqliteDataReader["ScanNum"]) - startScan;
					// if (current_scan >= data_width)
					//     break;

					compressedBinIntensity = (byte[])(m_sqliteDataReader["Intensities"]);

					if (compressedBinIntensity.Length == 0)
						continue;

					int indexCurrentBin = 0;
					int decompressLength = LZFCompressionUtil.Decompress(ref compressedBinIntensity, compressedBinIntensity.Length, ref streamBinIntensity, m_globalParameters.Bins * 4);

					int pixelY = 1;

					for (int binValue = 0; (binValue < decompressLength) && (indexCurrentBin < endBin); binValue += 4)
					{
						int intBinIntensity = BitConverter.ToInt32(streamBinIntensity, binValue);

						if (intBinIntensity < 0)
						{
							indexCurrentBin += -intBinIntensity; // concurrent zeros
						}
						else if ((indexCurrentBin < minMZBin) || (indexCurrentBin < startBin))
						{
							indexCurrentBin++;
						}
						else if (indexCurrentBin > maxMZBin)
						{
							break;
						}
						else
						{
							double calibratedBin = indexCurrentBin;

							for (int i = pixelY; i < dataHeight; i++)
							{
								if (m_calibrationTable[i] > calibratedBin)
								{
									pixelY = i;
									frameData[currentScan][pixelY] += intBinIntensity;
									break;
								}
							}
							indexCurrentBin++;
						}
					}
				}
			}

			m_sqliteDataReader.Close();
			return frameData;
		}

    	/// <summary>
    	/// Clones this database, but doesn't copy data in tables sTablesToSkipCopyingData.
    	/// If a table is skipped, data will still copy for the frame types specified in eFrameScanFrameTypeDataToAlwaysCopy.
    	/// </summary>
    	/// <param name="targetDBPath">The desired path of the newly cloned UIMF file.</param>
    	/// <param name="tablesToSkip">A list of table names (e.g. Frame_Scans) that should not be copied.</param>
		/// <param name="frameTypesToAlwaysCopy">
		/// A list of FrameTypes that should ALWAYS be copied. 
		///		e.g. If "Frame_Scans" is passed into tablesToSkip, data will still be inserted into "Frame_Scans" for these Frame Types.
		/// </param>
    	/// <returns>True if success, false if a problem</returns>
    	public bool CloneUIMF(string targetDBPath, List<string> tablesToSkip, List<FrameType> frameTypesToAlwaysCopy)
        {
            string sCurrentTable = string.Empty;
			
            try
            {
                // Get list of tables in source DB
                Dictionary<string, string> dctTableInfo = CloneUIMFGetObjects("table");

                // Delete the "sqlite_sequence" database from dctTableInfo if present
				if (dctTableInfo.ContainsKey("sqlite_sequence"))
				{
					dctTableInfo.Remove("sqlite_sequence");
				}

            	// Get list of indices in source DB
                Dictionary<string, string> dctIndexInfo = CloneUIMFGetObjects("index");

				if (File.Exists(targetDBPath))
				{
					File.Delete(targetDBPath);
				}

            	try
                {
                    string sTargetConnectionString = "Data Source = " + targetDBPath + "; Version=3; DateTimeFormat=Ticks;";
                    SQLiteConnection cnTargetDB = new SQLiteConnection(sTargetConnectionString);
	           
		            cnTargetDB.Open();
                    SQLiteCommand cmdTargetDB = cnTargetDB.CreateCommand();
                        
                    // Create each table
					foreach (KeyValuePair<string, string> kvp in dctTableInfo)
                	{
						if (!String.IsNullOrEmpty(kvp.Value))
						{
							sCurrentTable = string.Copy(kvp.Key);
							cmdTargetDB.CommandText = kvp.Value;
							cmdTargetDB.ExecuteNonQuery();
						}
                	}

					foreach (KeyValuePair<string, string> kvp in dctIndexInfo)
					{
						if (!String.IsNullOrEmpty(kvp.Value))
						{
							sCurrentTable = kvp.Key + " (create index)";
							cmdTargetDB.CommandText = kvp.Value;
							cmdTargetDB.ExecuteNonQuery();
						}
					}

                    try
                    {
                        cmdTargetDB.CommandText = "ATTACH DATABASE '" + m_uimfFilePath + "' AS SourceDB;";
                        cmdTargetDB.ExecuteNonQuery();

                        // Populate each table
						foreach (KeyValuePair<string, string> kvp in dctTableInfo)
						{
							sCurrentTable = string.Copy(kvp.Key);

                            if (!tablesToSkip.Contains(sCurrentTable))
                            {
                                string sSql = "INSERT INTO main." + sCurrentTable + " SELECT * FROM SourceDB." + sCurrentTable + ";";

                                cmdTargetDB.CommandText = sSql;
                                cmdTargetDB.ExecuteNonQuery();
                            }
                            else
                            {
                                if (sCurrentTable.ToLower() == "Frame_Scans".ToLower() && 
                                    frameTypesToAlwaysCopy != null && 
                                    frameTypesToAlwaysCopy.Count > 0)
                                {
                                    // Explicitly copy data for the frame types defined in eFrameScanFrameTypeDataToAlwaysCopy
                                    for (int i = 0; i < frameTypesToAlwaysCopy.Count; i++)
                                    {
                                        string sSql = "INSERT INTO main." + sCurrentTable + 
                                                      " SELECT * FROM SourceDB." + sCurrentTable +
                                                      " WHERE FrameNum IN (SELECT FrameNum FROM Frame_Parameters " +
													  "WHERE FrameType = " + (frameTypesToAlwaysCopy[i].Equals(FrameType.MS1) ? m_frameTypeMs : (int)frameTypesToAlwaysCopy[i]) + ");";

                                        cmdTargetDB.CommandText = sSql;
                                        cmdTargetDB.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        sCurrentTable = "(DETACH DATABASE)";

                        // Detach the source DB
                        cmdTargetDB.CommandText = "DETACH DATABASE 'SourceDB';";
                        cmdTargetDB.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error copying data into cloned database, table " + sCurrentTable, ex);
                    }

                    cmdTargetDB.Dispose();
                    cnTargetDB.Close();

                }
                catch (Exception ex)
                {
                    throw new Exception("Error initializing cloned database, table " + sCurrentTable, ex);
                }
                      
            }
            catch (Exception ex)
            {
                throw new Exception("Error cloning database: " + ex.Message, ex);
            }

            return true;
        }

		public static double ConvertBinToMZ(double slope, double intercept, double binWidth, double correctionTimeForTOF, int bin)
		{
			double t = bin * binWidth / 1000;
			double term1 = slope * ((t - correctionTimeForTOF / 1000 - intercept));
			return term1 * term1;
		}

        public string FrameTypeDescription(FrameType frameType)
        {
            switch (frameType)
            {
				case FrameType.MS1:
                    return "MS";
				case FrameType.MS2:
                    return "MS/MS";
				case FrameType.Calibration:
                    return "Calibration";
				case FrameType.Prescan:
                    return "Prescan";
                default:
                    throw new InvalidCastException("Invalid frame type: " + frameType);
            }
        }

		/// <summary>
		/// Returns the x,y,z arrays needed for a surface plot for the elution of the species in both the LC and drifttime dimensions
		/// </summary>
		/// <param name="startFrameNumber"></param>
		/// <param name="endFrameNumber"></param>
		/// <param name="frameType"></param>
		/// <param name="startScan"></param>
		/// <param name="endScan"></param>
		/// <param name="targetMZ"></param>
		/// <param name="toleranceInMZ"></param>
		/// <param name="frameValues"></param>
		/// <param name="scanValues"></param>
		/// <param name="intensities"></param>
		public void Get3DElutionProfile(int startFrameNumber, int endFrameNumber, FrameType frameType, int startScan, int endScan, double targetMZ, double toleranceInMZ, out int[] frameValues, out int[] scanValues, out int[] intensities)
		{

			if ((startFrameNumber > endFrameNumber) || (startFrameNumber < 0))
			{
				throw new ArgumentException("Failed to get LCProfile. Input startFrame was greater than input endFrame. start_frame=" + startFrameNumber.ToString() + ", end_frame=" + endFrameNumber.ToString());
			}

			if (startScan > endScan)
			{
				throw new ArgumentException("Failed to get 3D profile. Input startScan was greater than input endScan");
			}

			int lengthOfOutputArrays = (endFrameNumber - startFrameNumber + 1) * (endScan - startScan + 1);

			frameValues = new int[lengthOfOutputArrays];
			scanValues = new int[lengthOfOutputArrays];
			intensities = new int[lengthOfOutputArrays];


			int[] lowerUpperBins = GetUpperLowerBinsFromMz(startFrameNumber, targetMZ, toleranceInMZ);

			int[][][] frameIntensities = GetIntensityBlock(startFrameNumber, endFrameNumber, frameType, startScan, endScan, lowerUpperBins[0], lowerUpperBins[1]);

			int counter = 0;

			for (int frameNumber = startFrameNumber; frameNumber <= endFrameNumber; frameNumber++)
			{
				for (int scan = startScan; scan <= endScan; scan++)
				{
					int sumAcrossBins = 0;
					for (int bin = lowerUpperBins[0]; bin <= lowerUpperBins[1]; bin++)
					{
						int binIntensity = frameIntensities[frameNumber - startFrameNumber][scan - startScan][bin - lowerUpperBins[0]];
						sumAcrossBins += binIntensity;
					}
					frameValues[counter] = frameNumber;
					scanValues[counter] = scan;
					intensities[counter] = sumAcrossBins;
					counter++;
				}
			}
		}

        /// <summary>
        /// Extracts BPI from startFrame to endFrame and startScan to endScan and returns an array
        /// </summary>
        /// <param name="frameType"></param>
        /// <param name="startFrameNumber"></param>
        /// <param name="endFrameNumber"></param>
        /// <param name="startScan"></param>
        /// <param name="endScan"></param>
        public double[] GetBPI(FrameType frameType, int startFrameNumber, int endFrameNumber, int startScan, int endScan)
        {
            return GetTicOrBpi(frameType, startFrameNumber, endFrameNumber, startScan, endScan, BPI);
        }

        public List<string> GetCalibrationTableNames()
        {
        	SQLiteCommand cmd = new SQLiteCommand(m_uimfDatabaseConnection) {CommandText = "SELECT NAME FROM Sqlite_master WHERE type='table' ORDER BY NAME"};
        	List<string> calibrationTableNames = new List<string>();
            try
            {
				using (SQLiteDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string tableName = Convert.ToString(reader["Name"]);
						if (tableName.StartsWith("Calib_"))
						{
							calibrationTableNames.Add(tableName);
						}
					}
				}
            }
            catch (Exception ex)
            {
                throw new Exception("Exception finding calibration table names: " + ex);
            }

            return calibrationTableNames;
        }

        public int GetCountPerFrame(int frameNumber)
        {
            int countPerFrame = 0;
			m_getCountPerFrameCommand.Parameters.Add(new SQLiteParameter(":FrameNum", frameNumber));

            try
            {
                SQLiteDataReader reader = m_getCountPerFrameCommand.ExecuteReader();
                while (reader.Read())
                {
					countPerFrame = reader.IsDBNull(0) ? 1 : Convert.ToInt32(reader[0]);
                }
                m_getCountPerFrameCommand.Parameters.Clear();
                reader.Dispose();
                reader.Close();
            }
            catch (Exception ex)
            {
				Console.WriteLine("Exception looking up sum(nonzerocount) for frame " + frameNumber + ": " + ex.Message);
                countPerFrame = 1;
            }

            return countPerFrame;
        }

		public void GetDriftTimeProfile(int startFrameNumber, int endFrameNumber, FrameType frameType, int startScan, int endScan, double targetMZ, double toleranceInMZ, ref int[] imsScanValues, ref int[] intensities)
		{
			if ((startFrameNumber > endFrameNumber) || (startFrameNumber < 0))
			{
				throw new ArgumentException("Failed to get DriftTime profile. Input startFrame was greater than input endFrame. start_frame=" + startFrameNumber.ToString() + ", end_frame=" + endFrameNumber.ToString());
			}

			if ((startScan > endScan) || (startScan < 0))
			{
				throw new ArgumentException("Failed to get LCProfile. Input startScan was greater than input endScan. startScan=" + startScan + ", endScan=" + endScan);
			}

			int lengthOfScanArray = endScan - startScan + 1;
			imsScanValues = new int[lengthOfScanArray];
			intensities = new int[lengthOfScanArray];

			int[] lowerAndUpperBinBoundaries = GetUpperLowerBinsFromMz(startFrameNumber, targetMZ, toleranceInMZ);

			int[][][] intensityBlock = GetIntensityBlock(startFrameNumber, endFrameNumber, frameType, startScan, endScan, lowerAndUpperBinBoundaries[0], lowerAndUpperBinBoundaries[1]);

			for (int scanIndex = startScan; scanIndex <= endScan; scanIndex++)
			{
				int frameSum = 0;
				for (int frameIndex = startFrameNumber; frameIndex <= endFrameNumber; frameIndex++)
				{
					int binSum = 0;
					for (int bin = lowerAndUpperBinBoundaries[0]; bin <= lowerAndUpperBinBoundaries[1]; bin++)
					{
						binSum += intensityBlock[frameIndex - startFrameNumber][scanIndex - startScan][bin - lowerAndUpperBinBoundaries[0]];
					}
					frameSum += binSum;

				}

				intensities[scanIndex - startScan] = frameSum;
				imsScanValues[scanIndex - startScan] = scanIndex;
			}
		}

        /// <summary>
        /// Method to provide the bytes from tables that store metadata files 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public byte[] GetFileBytesFromTable(string tableName)
        {
            SQLiteDataReader reader = null;
            byte[] byteBuffer = null;

            try
            {
                m_getFileBytesCommand.CommandText = "SELECT FileText from " + tableName;

                if (TableExists(tableName))
                {
                    reader = m_getFileBytesCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        byteBuffer = (byte[])(reader["FileText"]);
                    }
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return byteBuffer;
        }
        
        public Stack<int[]> GetFrameAndScanListByDescendingIntensity()
        {
            FrameParameters fp = GetFrameParameters(0);
            Stack<int[]> tuples = new Stack<int[]>(m_globalParameters.NumFrames * fp.Scans);

        	m_sqliteDataReader = m_getFramesAndScanByDescendingIntensityCommand.ExecuteReader();
            while (m_sqliteDataReader.Read())
            {
                int[] values = new int[3];
                values[0] = Convert.ToInt32(m_sqliteDataReader[0]);
                values[1] = Convert.ToInt32(m_sqliteDataReader[1]);
                values[2] = Convert.ToInt32(m_sqliteDataReader[2]);

                tuples.Push(values);
            }
            m_sqliteDataReader.Close();
            return tuples;
        }

        /// <summary>
        /// Returns the frame numbers for the specified frame_type
        /// </summary>
        /// <returns></returns>
        public int[] GetFrameNumbers(FrameType frameType)
        {
			List<int> frameNumberList = new List<int>();

			using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
			{
				dbCmd.CommandText = "SELECT DISTINCT(FrameNum) FROM Frame_Parameters WHERE FrameType = :FrameType ORDER BY FrameNum";
				dbCmd.Parameters.Add(new SQLiteParameter("FrameType", (frameType.Equals(FrameType.MS1) ? m_frameTypeMs : (int)frameType)));
				dbCmd.Prepare();
				using (SQLiteDataReader reader = dbCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						frameNumberList.Add(Convert.ToInt32(reader["FrameNum"]));
					}
				}
			}

        	return frameNumberList.ToArray();
        }

		public FrameParameters GetFrameParameters(int frameNumber)
		{
			if (frameNumber < 0)
			{
				throw new ArgumentOutOfRangeException("FrameNumber should be greater than or equal to zero.");
			}

			FrameParameters fp = new FrameParameters();

			// Check in cache first
			if (m_frameParametersCache.ContainsKey(frameNumber))
			{
				// Frame parameters object is cached, retrieve it and return
				fp = m_frameParametersCache[frameNumber];
			}
			else
			{
				// Parameters are not yet cached; retrieve and cache them
				if (m_uimfDatabaseConnection != null)
				{
					m_getFrameParametersCommand.Parameters.Add(new SQLiteParameter("FrameNum", frameNumber));

					SQLiteDataReader reader = m_getFrameParametersCommand.ExecuteReader();
					if (reader.Read())
					{
						PopulateFrameParameters(fp, reader);
					}

					// Store the frame parameters in the cache
					m_frameParametersCache.Add(frameNumber, fp);
					m_getFrameParametersCommand.Parameters.Clear();

					reader.Close();
				}
			}

			m_mzCalibration = new MZ_Calibrator(fp.CalibrationSlope / 10000.0, fp.CalibrationIntercept * 10000.0);

			return fp;
		}

        /// <summary>
        /// Returns the key frame pressure value that is used in the calculation of drift time 
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <returns>Frame pressure used in drift time calc</returns>
        public double GetFramePressureForCalculationOfDriftTime(int frameIndex)
        {

            /*
             * [gord, April 2011] A little history..
             * Earlier UIMF files have the column 'PressureBack' but not the 
             * newer 'RearIonFunnelPressure' or 'IonFunnelTrapPressure'
             * 
             * So, will first check for old format
             * if there is a value there, will use it.  If not,
             * look for newer columns and use these values. 
             */

            FrameParameters fp = GetFrameParameters(frameIndex);
            double pressure = fp.PressureBack;

            if (Math.Abs(pressure - 0) < float.Epsilon) pressure = fp.RearIonFunnelPressure;
			if (Math.Abs(pressure - 0) < float.Epsilon) pressure = fp.IonFunnelTrapPressure;

            return pressure;

        }

		public int[][] GetFramesAndScanIntensitiesForAGivenMz(int startFrameNumber, int endFrameNumber, FrameType frameType, int startScan, int endScan, double targetMZ, double toleranceInMZ)
		{
			if ((startFrameNumber > endFrameNumber) || (startFrameNumber < 0))
			{
				throw new ArgumentException("Failed to get 3D profile. Input startFrame was greater than input endFrame");
			}

			if (startScan > endScan || startScan < 0)
			{
				throw new ArgumentException("Failed to get 3D profile. Input startScan was greater than input endScan");
			}

			int[][] intensityValues = new int[endFrameNumber - startFrameNumber + 1][];
			int[] lowerUpperBins = GetUpperLowerBinsFromMz(startFrameNumber, targetMZ, toleranceInMZ);

			int[][][] frameIntensities = GetIntensityBlock(startFrameNumber, endFrameNumber, frameType, startScan, endScan, lowerUpperBins[0], lowerUpperBins[1]);


			for (int frameNumber = startFrameNumber; frameNumber <= endFrameNumber; frameNumber++)
			{
				intensityValues[frameNumber - startFrameNumber] = new int[endScan - startScan + 1];
				for (int scan = startScan; scan <= endScan; scan++)
				{
					int sumAcrossBins = 0;
					for (int bin = lowerUpperBins[0]; bin <= lowerUpperBins[1]; bin++)
					{
						int binIntensity = frameIntensities[frameNumber - startFrameNumber][scan - startScan][bin - lowerUpperBins[0]];
						sumAcrossBins += binIntensity;
					}

					intensityValues[frameNumber - startFrameNumber][scan - startScan] = sumAcrossBins;
				}
			}

			return intensityValues;
		}

        /// <summary>
        /// Populate the global parameters object, m_globalParameters
        /// </summary>
        /// <remarks>We want to make sure that this method is only called once</remarks>
        /// <returns></returns>
        public GlobalParameters GetGlobalParameters()
        {
            if (m_globalParameters == null)
            {
                //Retrieve it from the database
                if (m_uimfDatabaseConnection == null)
                {
                    //this means that you've called this method without opening the UIMF file.
                    //should throw an exception saying UIMF file not opened here
                    //for now, let's just set an error flag
                    //success = false;
                    //the custom exception class has to be defined as yet
                }
                else
                {
                    m_globalParameters = GetGlobalParametersFromTable(m_uimfDatabaseConnection);                    
                }
            }

            return m_globalParameters;
        }

        public static GlobalParameters GetGlobalParametersFromTable(SQLiteConnection oUimfDatabaseConnection)
        {
            GlobalParameters oGlobalParameters = new GlobalParameters();

            SQLiteCommand dbCmd = oUimfDatabaseConnection.CreateCommand();
            dbCmd.CommandText = "SELECT * FROM Global_Parameters";

            SQLiteDataReader reader = dbCmd.ExecuteReader();
            while (reader.Read())
            {
                try
                {

                    oGlobalParameters.BinWidth = Convert.ToDouble(reader["BinWidth"]);
                    oGlobalParameters.DateStarted = Convert.ToString(reader["DateStarted"]);
                    oGlobalParameters.NumFrames = Convert.ToInt32(reader["NumFrames"]);
                    oGlobalParameters.TimeOffset = Convert.ToInt32(reader["TimeOffset"]);
                    oGlobalParameters.BinWidth = Convert.ToDouble(reader["BinWidth"]);
                    oGlobalParameters.Bins = Convert.ToInt32(reader["Bins"]);
                    try
                    {
                        oGlobalParameters.TOFCorrectionTime = Convert.ToSingle(reader["TOFCorrectionTime"]);
                    }
                    catch
                    {
                        m_errMessageCounter++;
                        Console.WriteLine("Warning: this UIMF file is created with an old version of IMF2UIMF (TOFCorrectionTime is missing from the Global_Parameters table), please get the newest version from \\\\floyd\\software");
                    }
                    oGlobalParameters.Prescan_TOFPulses = Convert.ToInt32(reader["Prescan_TOFPulses"]);
                    oGlobalParameters.Prescan_Accumulations = Convert.ToInt32(reader["Prescan_Accumulations"]);
                    oGlobalParameters.Prescan_TICThreshold = Convert.ToInt32(reader["Prescan_TICThreshold"]);
                    oGlobalParameters.Prescan_Continuous = Convert.ToBoolean(reader["Prescan_Continuous"]);
                    oGlobalParameters.Prescan_Profile = Convert.ToString(reader["Prescan_Profile"]);
                    oGlobalParameters.FrameDataBlobVersion = (float)Convert.ToDouble((reader["FrameDataBlobVersion"]));
                    oGlobalParameters.ScanDataBlobVersion = (float)Convert.ToDouble((reader["ScanDataBlobVersion"]));
                    oGlobalParameters.TOFIntensityType = Convert.ToString(reader["TOFIntensityType"]);
                    oGlobalParameters.DatasetType = Convert.ToString(reader["DatasetType"]);
                    try
                    {
                        oGlobalParameters.InstrumentName = Convert.ToString(reader["Instrument_Name"]);
                    }
                    catch (Exception)
                    {
                        //ignore since it may not be present in all previous versions
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to get global parameters " + ex);
                }
            }

            dbCmd.Dispose();
            reader.Close();

            return oGlobalParameters;
        }

		public double[][] GetIntensityBlockForDemultiplexing(int frameNumber, FrameType frameType, int segmentLength, Dictionary<int, int> scanToIndexMap)
		{
			FrameParameters frameParameters = GetFrameParameters(frameNumber);

			int numBins = m_globalParameters.Bins;
			int numScans = frameParameters.Scans;

			// The number of scans has to be divisible by the given segment length
			if (numScans % segmentLength != 0)
			{
				throw new Exception("Number of scans of " + numScans + " is not divisible by the given segment length of " + segmentLength);
			}

			// Initialize the intensities 2-D array
			double[][] intensities = new double[numBins][];
			for (int i = 0; i < numBins; i++)
			{
				intensities[i] = new double[numScans];
			}

			// Now setup queries to retrieve data
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameNum1", frameParameters.FrameNum));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameNum2", frameParameters.FrameNum));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("ScanNum1", -1));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("ScanNum2", numScans));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameType", (frameType.Equals(FrameType.MS1) ? m_frameTypeMs : (int)frameType)));

			byte[] decompSpectraRecord = new byte[m_globalParameters.Bins * DATASIZE];

			using (m_sqliteDataReader = m_getSpectrumCommand.ExecuteReader())
			{
				while (m_sqliteDataReader.Read())
				{
					int binIndex = 0;

					byte[] spectra = (byte[])(m_sqliteDataReader["Intensities"]);
					int scanNumber = Convert.ToInt32(m_sqliteDataReader["ScanNum"]);

					if (spectra.Length > 0)
					{
						int outputLength = LZFCompressionUtil.Decompress(ref spectra, spectra.Length, ref decompSpectraRecord, m_globalParameters.Bins * DATASIZE);
						int numReturnedBins = outputLength / DATASIZE;
						for (int i = 0; i < numReturnedBins; i++)
						{
							int decodedIntensityValue = BitConverter.ToInt32(decompSpectraRecord, i * DATASIZE);

							if (decodedIntensityValue < 0)
							{
								binIndex += -decodedIntensityValue;
							}
							else
							{
								intensities[binIndex][scanToIndexMap[scanNumber]] = decodedIntensityValue;
								binIndex++;
							}
						}
					}
				}
			}

			m_getSpectrumCommand.Parameters.Clear();

			return intensities;
		}

		public void GetLCProfile(int startFrameNumber, int endFrameNumber, FrameType frameType, int startScan, int endScan, double targetMZ, double toleranceInMZ, out int[] frameValues, out int[] intensities)
		{
			if ((startFrameNumber > endFrameNumber) || (startFrameNumber < 0))
			{
				throw new ArgumentException("Failed to get LCProfile. Input startFrame was greater than input endFrame. start_frame=" + startFrameNumber.ToString() + ", end_frame=" + endFrameNumber.ToString());
			}

			frameValues = new int[endFrameNumber - startFrameNumber + 1];

			int[] lowerAndUpperBinBoundaries = GetUpperLowerBinsFromMz(startFrameNumber, targetMZ, toleranceInMZ);
			intensities = new int[endFrameNumber - startFrameNumber + 1];

			int[][][] frameIntensities = GetIntensityBlock(startFrameNumber, endFrameNumber, frameType, startScan, endScan, lowerAndUpperBinBoundaries[0], lowerAndUpperBinBoundaries[1]);
			for (int frameNumber = startFrameNumber; frameNumber <= endFrameNumber; frameNumber++)
			{
				int scanSum = 0;
				for (int scan = startScan; scan <= endScan; scan++)
				{
					int binSum = 0;
					for (int bin = lowerAndUpperBinBoundaries[0]; bin <= lowerAndUpperBinBoundaries[1]; bin++)
					{
						binSum += frameIntensities[frameNumber - startFrameNumber][scan - startScan][bin - lowerAndUpperBinBoundaries[0]];
					}
					scanSum += binSum;
				}

				intensities[frameNumber - startFrameNumber] = scanSum;
				frameValues[frameNumber - startFrameNumber] = frameNumber;
			}
		}

        public SortedList<int, LogEntry> GetLogEntries(string entryType, string postedBy)
        {
			SortedList<int, LogEntry> lstLogEntries = new SortedList<int, LogEntry>();

            if (TableExists("Log_Entries"))
            {
				using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
				{

					string sSql = "SELECT Entry_ID, Posted_By, Posting_Time, Type, Message FROM Log_Entries";
					string sWhere = String.Empty;

					if (!String.IsNullOrEmpty(entryType))
					{
						sWhere = "WHERE Type = '" + entryType + "'";
					}

					if (!String.IsNullOrEmpty(postedBy))
					{
						if (sWhere.Length == 0)
						{
							sWhere = "WHERE";
						}
						else
						{
							sWhere += " AND";
						}

						sWhere += " Posted_By = '" + postedBy + "'";
					}

					if (sWhere.Length > 0)
					{
						sSql += " " + sWhere;
					}

					sSql += " ORDER BY Entry_ID;";

					dbCmd.CommandText = sSql;

					using (SQLiteDataReader reader = dbCmd.ExecuteReader())
					{
						while (reader.Read())
						{
							try
							{
								LogEntry logEntry = new LogEntry();

								int iEntryID = Convert.ToInt32(reader["Entry_ID"]);
								logEntry.PostedBy = Convert.ToString(reader["Posted_By"]);

								string sPostingTime = Convert.ToString(reader["Posting_Time"]);
								DateTime postingTime;
								DateTime.TryParse(sPostingTime, out postingTime);
								logEntry.PostingTime = postingTime;

								logEntry.Type = Convert.ToString(reader["Type"]);
								logEntry.Message = Convert.ToString(reader["Message"]);

								lstLogEntries.Add(iEntryID, logEntry);

							}
							catch (Exception ex)
							{
								throw new Exception("Failed to get global parameters " + ex);
							}
						}
					}
				}
            }

            return lstLogEntries;
        }

        /// <summary>
        /// Constructs a dictionary that has the frame numbers as the key and the frame type as the value.
        /// </summary>
        /// <returns>Returns a dictionary object that has frame number as the key and frame type as the value.</returns>
		public Dictionary<int, FrameType> GetMasterFrameList()
        {
			Dictionary<int, FrameType> masterFrameDictionary = new Dictionary<int, FrameType>();

			using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
			{
				dbCmd.CommandText = "SELECT DISTINCT(FrameNum), FrameType FROM Frame_Parameters";
				dbCmd.Prepare();
				using (SQLiteDataReader reader = dbCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						int frameNumber = Convert.ToInt32(reader["FrameNum"]);
						int frameType = Convert.ToInt32(reader["FrameType"]);

						// If the frame type is 0, then we are dealing with an old UIMF file where the MS1 frames were labeled as 0
						if (frameType == 0) frameType = 1;

						masterFrameDictionary.Add(frameNumber, (FrameType) frameType);
					}
				}
			}

        	return masterFrameDictionary;
        }

        /// <summary>
        /// Utility method to return the Frame Type for a particular frame number
        /// </summary>
        /// <param name="frameNumber"></param>
        /// <returns></returns>
        public FrameType GetFrameTypeForFrame(int frameNumber)
        {
        	int frameTypeInt = -1;

			using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
			{
				dbCmd.CommandText = "SELECT FrameType FROM Frame_Parameters WHERE FrameNum = :FrameNumber";
				dbCmd.Parameters.Add(new SQLiteParameter("FrameNumber", frameNumber));
				using (SQLiteDataReader reader = dbCmd.ExecuteReader())
				{
					if (reader.Read())
					{
						frameTypeInt = Convert.ToInt32(reader["FrameType"]);
					}
				}
			}

			// If the frame type is 0, then this is an older UIMF file where the MS1 frames were labeled as 0
			if(frameTypeInt == 0)
			{
				return FrameType.MS1;
			}
			
			return (FrameType) frameTypeInt;
        }

		/// <summary>
		/// </summary>
		/// <param name="frameType"></param>
		/// <returns></returns>
		public int GetNumberOfFrames(FrameType frameType)
		{
			int count = 0;

			using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
			{
				dbCmd.CommandText = "SELECT COUNT(DISTINCT(FrameNum)) AS FrameCount FROM Frame_Parameters WHERE FrameType IN (:FrameType)";
				dbCmd.Parameters.Add(new SQLiteParameter("FrameType", (frameType.Equals(FrameType.MS1) ? "0,1" : ((int)frameType).ToString())));
				dbCmd.Prepare();
				using (SQLiteDataReader reader = dbCmd.ExecuteReader())
				{
					if (reader.Read())
					{
						count = Convert.ToInt32(reader["FrameCount"]);
					}
				}
			}

			return count;
		}

		public double GetPixelMZ(int bin)
		{
			if ((m_calibrationTable != null) && (bin < m_calibrationTable.Length))
			{
				return m_calibrationTable[bin];
			}

			return -1;
		}

    	/// <summary>
    	/// Extracts m/z values and intensities from given frame number and scan number.
    	/// Each entry into mzArray will be the m/z value that contained a non-zero intensity value.
    	/// The index of the m/z value in mzArray will match the index of the corresponding intensity value in intensityArray.
    	/// </summary>
    	/// <param name="frameNumber">The frame number of the desired spectrum.</param>
    	/// <param name="frameType">The frame type to consider.</param>
    	/// <param name="scanNumber">The scan number of the desired spectrum.</param>
    	/// <param name="mzArray">The m/z values that contained non-zero intensity values.</param>
    	/// <param name="intensityArray">The corresponding intensity values of the non-zero m/z value.</param>
    	/// <returns>The number of non-zero m/z values found in the resulting spectrum.</returns>
    	public int GetSpectrum(int frameNumber, FrameType frameType, int scanNumber, out double[] mzArray, out int[] intensityArray)
		{
			return GetSpectrum(frameNumber, frameNumber, frameType, scanNumber, scanNumber, out mzArray, out intensityArray);
		}

		/// <summary>
		/// Extracts m/z values and intensities from given frame range and scan range.
		/// The intensity values of each m/z value are summed across the frame range. The result is a spectrum for a single frame.
		/// Each entry into mzArray will be the m/z value that contained a non-zero intensity value.
		/// The index of the m/z value in mzArray will match the index of the corresponding intensity value in intensityArray.
		/// </summary>
		/// <param name="startFrameNumber">The start frame number of the desired spectrum.</param>
		/// <param name="endFrameNumber">The end frame number of the desired spectrum.</param>
		/// <param name="frameType">The frame type to consider.</param>
		/// <param name="startScanNumber">The start scan number of the desired spectrum.</param>
		/// <param name="endScanNumber">The end scan number of the desired spectrum.</param>
		/// <param name="mzArray">The m/z values that contained non-zero intensity values.</param>
		/// <param name="intensityArray">The corresponding intensity values of the non-zero m/z value.</param>
		/// <returns>The number of non-zero m/z values found in the resulting spectrum.</returns>
		public int GetSpectrum(int startFrameNumber, int endFrameNumber, FrameType frameType, int startScanNumber, int endScanNumber, out double[] mzArray, out int[] intensityArray)
		{
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameNum1", startFrameNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameNum2", endFrameNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("ScanNum1", startScanNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("ScanNum2", endScanNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameType", (frameType.Equals(FrameType.MS1) ? m_frameTypeMs : (int)frameType)));

			int nonZeroCount = 0;

			// Allocate the maximum possible for these arrays. Later on we will strip out the zeros.
			// Adding 1 to the size to fix a bug in some old IMS data where the bin index could exceed the maximum bins by 1
			mzArray = new double[m_globalParameters.Bins + 1];
			intensityArray = new int[m_globalParameters.Bins + 1];

			using (SQLiteDataReader reader = m_getSpectrumCommand.ExecuteReader())
			{
				byte[] decompSpectraRecord = new byte[m_globalParameters.Bins * DATASIZE];

				while (reader.Read())
				{
					int binIndex = 0;
					byte[] spectraRecord = (byte[])(reader["Intensities"]);
					if (spectraRecord.Length > 0)
					{
						int frameNumber = Convert.ToInt32(reader["FrameNum"]);
						FrameParameters frameParameters = GetFrameParameters(frameNumber);

						int outputLength = LZFCompressionUtil.Decompress(ref spectraRecord, spectraRecord.Length, ref decompSpectraRecord, m_globalParameters.Bins * DATASIZE);
						int numBins = outputLength / DATASIZE;

						for (int i = 0; i < numBins; i++)
						{
							int decodedSpectraRecord = BitConverter.ToInt32(decompSpectraRecord, i * DATASIZE);
							if (decodedSpectraRecord < 0)
							{
								binIndex += -decodedSpectraRecord;
							}
							else
							{
								// Only need to set the m/z array or update the nonZeroCount if we have not previously seen this m/z value
								if(intensityArray[binIndex] == 0)
								{
									double mz = ConvertBinToMZ(frameParameters.CalibrationSlope, frameParameters.CalibrationIntercept, m_globalParameters.BinWidth, m_globalParameters.TOFCorrectionTime, binIndex);
									mzArray[binIndex] = mz;
									nonZeroCount++;
								}

								intensityArray[binIndex] += decodedSpectraRecord;
								binIndex++;
							}
						}
					}
				}

				StripZerosFromArrays(nonZeroCount, ref mzArray, ref intensityArray);
			}

			m_getSpectrumCommand.Parameters.Clear();

			return nonZeroCount;
		}

		/// <summary>
		/// Extracts intensities from given frame range and scan range.
		/// The intensity values of each bin are summed across the frame range. The result is a spectrum for a single frame.
		/// </summary>
		/// <param name="frameNumber">The frame number of the desired spectrum.</param>
		/// <param name="frameType">The frame type to consider.</param>
		/// <param name="scanNumber">The scan number of the desired spectrum.</param>
		/// <returns>The number of non-zero bins found in the resulting spectrum.</returns>
		public int[] GetSpectrumAsBins(int frameNumber, FrameType frameType, int scanNumber)
		{
			return GetSpectrumAsBins(frameNumber, frameNumber, frameType, scanNumber, scanNumber);
		}

    	/// <summary>
    	/// Extracts intensities from given frame range and scan range.
    	/// The intensity values of each bin are summed across the frame range. The result is a spectrum for a single frame.
    	/// </summary>
    	/// <param name="startFrameNumber">The start frame number of the desired spectrum.</param>
		/// <param name="endFrameNumber">The end frame number of the desired spectrum.</param>
		/// <param name="frameType">The frame type to consider.</param>
    	/// <param name="startScanNumber">The start scan number of the desired spectrum.</param>
		/// <param name="endScanNumber">The end scan number of the desired spectrum.</param>
    	/// <returns>An array containing an intensity value for each bin location, even if the intensity value is 0.</returns>
    	public int[] GetSpectrumAsBins(int startFrameNumber, int endFrameNumber, FrameType frameType, int startScanNumber, int endScanNumber)
		{
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameNum1", startFrameNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameNum2", endFrameNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("ScanNum1", startScanNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("ScanNum2", endScanNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameType", (frameType.Equals(FrameType.MS1) ? m_frameTypeMs : (int)frameType)));

			// Adding 1 to the number of bins to fix a bug in some old IMS data where the bin index could exceed the maximum bins by 1
			int[] intensityArray = new int[m_globalParameters.Bins + 1];

			using (SQLiteDataReader reader = m_getSpectrumCommand.ExecuteReader())
			{
				byte[] decompSpectraRecord = new byte[m_globalParameters.Bins * DATASIZE];
				
				while (reader.Read())
				{
					int binIndex = 0;
					byte[] spectraRecord = (byte[])(reader["Intensities"]);

					if (spectraRecord.Length > 0)
					{
						int outputLength = LZFCompressionUtil.Decompress(ref spectraRecord, spectraRecord.Length, ref decompSpectraRecord, m_globalParameters.Bins * DATASIZE);
						int numBins = outputLength / DATASIZE;

						for (int i = 0; i < numBins; i++)
						{
							int decodedSpectraRecord = BitConverter.ToInt32(decompSpectraRecord, i * DATASIZE);
							if (decodedSpectraRecord < 0)
							{
								binIndex += -decodedSpectraRecord;
							}
							else
							{
								intensityArray[binIndex] += decodedSpectraRecord;
								binIndex++;
							}
						}
					}
				}
			}

			m_getSpectrumCommand.Parameters.Clear();

			return intensityArray;
		}

    	/// <summary>
    	/// Extracts TIC from startFrame to endFrame and startScan to endScan and returns an array
    	/// </summary>
    	/// <param name="frameType"></param>
    	/// <param name="startFrameNumber"></param>
    	/// <param name="endFrameNumber"></param>
    	/// <param name="startScan"></param>
    	/// <param name="endScan"></param>
    	public double[] GetTIC(FrameType frameType, int startFrameNumber, int endFrameNumber, int startScan, int endScan)
        {
            return GetTicOrBpi(frameType, startFrameNumber, endFrameNumber, startScan, endScan, TIC);
        }

        /// <summary>
        /// Extracts TIC from frameNum and scanNum
        /// </summary>
        /// <param name="frameNumber"></param>
        /// <param name="scanNum"></param>
        /// <returns></returns>
        public double GetTIC(int frameNumber, int scanNum)
        {
            double tic = 0;

			using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
			{
				dbCmd.CommandText = "SELECT TIC FROM Frame_Scans WHERE FrameNum = " + frameNumber + " AND ScanNum = " + scanNum;
				using (SQLiteDataReader reader = dbCmd.ExecuteReader())
				{
					if (reader.Read())
					{
						tic = Convert.ToDouble(reader["TIC"]);
					}
				}
			}

            return tic;
        }

        /// <summary>
        /// Method to check if this dataset has any MSMS data
        /// </summary>
        /// <returns>True if MSMS frames are present</returns>
        public bool HasMSMSData()
        {
			int count = 0;

			using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
			{
				dbCmd.CommandText = "SELECT COUNT(DISTINCT(FrameNum)) AS FrameCount FROM Frame_Parameters WHERE FrameType = :FrameType";
				dbCmd.Parameters.Add(new SQLiteParameter("FrameType", (int) FrameType.MS2));
				dbCmd.Prepare();
				using (SQLiteDataReader reader = dbCmd.ExecuteReader())
				{
					if (reader.Read())
					{
						count = Convert.ToInt32(reader["FrameCount"]);
					}
				}
			}

        	return count > 0;
        }

        /// <summary>
        /// Returns True if all frames with frame types 0 through 3 have CalibrationDone > 0 in frame_parameters
        /// </summary>
        /// <returns></returns>
        public bool IsCalibrated()
        {
            return IsCalibrated(FrameType.Calibration);
        }

        /// <summary>
        /// Returns True if all frames have CalibrationDone > 0 in frame_parameters
        /// </summary>
        /// <param name="iMaxFrameTypeToExamine">Maximum frame type to examine when checking for calibrated frames</param>
        /// <returns></returns>
		public bool IsCalibrated(FrameType iMaxFrameTypeToExamine)
        {
            bool bIsCalibrated = false;

			int iFrameTypeCount = -1;
			int iFrameTypeCountCalibrated = -2;

			using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
			{
				dbCmd.CommandText = "SELECT FrameType, COUNT(*)  AS FrameCount, SUM(IFNULL(CalibrationDone, 0)) AS FramesCalibrated " +
				                    "FROM frame_parameters " +
				                    "GROUP BY FrameType;";
				using (SQLiteDataReader reader = dbCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						int iFrameType = -1;
						try
						{
							iFrameType = Convert.ToInt32(reader[0]);
							int iFrameCount = Convert.ToInt32(reader[1]);
							int iCalibratedFrameCount = Convert.ToInt32(reader[2]);

							if (iMaxFrameTypeToExamine < 0 || iFrameType <= (int) iMaxFrameTypeToExamine)
							{
								iFrameTypeCount += 1;
								if (iFrameCount == iCalibratedFrameCount)
									iFrameTypeCountCalibrated += 1;
							}

						}
						catch (Exception ex)
						{
							Console.WriteLine("Exception determing if all frames are calibrated; error occurred with FrameType " + iFrameType + ": " + ex.Message);
						}
					}
				}
			}

			if (iFrameTypeCount == iFrameTypeCountCalibrated)
			{
				bIsCalibrated = true;
			}

        	return bIsCalibrated;
        }

		/// <summary>
		/// Post a new log entry to table Log_Entries
		/// </summary>
		/// <param name="entryType">Log entry type (typically Normal, Error, or Warning)</param>
		/// <param name="message">Log message</param>
		/// <param name="postedBy">Process or application posting the log message</param>
		/// <remarks>The Log_Entries table will be created if it doesn't exist</remarks>
		public void PostLogEntry(string entryType, string message, string postedBy)
		{
			DataWriter.PostLogEntry(m_uimfDatabaseConnection, entryType, message, postedBy);
		}

		public void ResetFrameParameters()
		{
			m_frameParametersCache.Clear();
		}
      
        public bool TableExists(string tableName)
        {
            return TableExists(m_uimfDatabaseConnection, tableName);
        }

        public static bool TableExists(SQLiteConnection oConnection, string tableName)
        {
			bool hasRows;

			using (SQLiteCommand cmd = new SQLiteCommand(oConnection) { CommandText = "SELECT name FROM sqlite_master WHERE type='table' And name = '" + tableName + "'" })
			{
				using (SQLiteDataReader rdr = cmd.ExecuteReader())
				{
					hasRows = rdr.HasRows;
				}
			}

        	return hasRows;
        }

        public bool TableHasColumn(string tableName, string columnName)
        {
            return TableHasColumn(m_uimfDatabaseConnection, tableName, columnName);
        }

        public static bool TableHasColumn(SQLiteConnection oConnection, string tableName, string columnName)
        {
			bool hasColumn;

			using (SQLiteCommand cmd = new SQLiteCommand(oConnection) { CommandText = "Select * From '" + tableName + "' Limit 1;" })
			{
				using (SQLiteDataReader rdr = cmd.ExecuteReader())
				{
					hasColumn = rdr.GetOrdinal(columnName) >= 0;
				}
			}

        	return hasColumn;
        }

    	/// <summary>
    	/// /// Update the calibration coefficients for all frames
    	/// </summary>
    	/// <param name="slope">The slope value for the calibration.</param>
    	/// <param name="intercept">The intercept for the calibration.</param>
		/// <param name="isAutoCalibrating">Optional argument that should be set to true if calibration is automatic. Defaults to false.</param>
    	public void UpdateAllCalibrationCoefficients(float slope, float intercept, bool isAutoCalibrating = false)
		{
			m_preparedStatement = m_uimfDatabaseConnection.CreateCommand();
			m_preparedStatement.CommandText = "UPDATE Frame_Parameters " +
											 "SET CalibrationSlope = " + slope + ", " +
												 "CalibrationIntercept = " + intercept;
			if (isAutoCalibrating)
			{
				m_preparedStatement.CommandText += ", CalibrationDone = 1";
			}

    		m_preparedStatement.ExecuteNonQuery();
			m_preparedStatement.Dispose();

			ResetFrameParameters();
		}

		/// <summary>
		/// Update the calibration coefficients for a single frame
		/// </summary>
		/// <param name="frameNumber">The frame number to update.</param>
		/// <param name="slope">The slope value for the calibration.</param>
		/// <param name="intercept">The intercept for the calibration.</param>
		/// <param name="isAutoCalibrating">Optional argument that should be set to true if calibration is automatic. Defaults to false.</param>
		public void UpdateCalibrationCoefficients(int frameNumber, float slope, float intercept, bool isAutoCalibrating = false)
		{
			m_preparedStatement = m_uimfDatabaseConnection.CreateCommand();
			m_preparedStatement.CommandText = "UPDATE Frame_Parameters " +
											 "SET CalibrationSlope = " + slope + ", " +
												 "CalibrationIntercept = " + intercept;
			if (isAutoCalibrating)
			{
				m_preparedStatement.CommandText += ", CalibrationDone = 1";
			}

			m_preparedStatement.CommandText += " WHERE FrameNum = " + frameNumber.ToString();

			m_preparedStatement.ExecuteNonQuery();
			m_preparedStatement.Dispose();

			// Make sure the m_mzCalibration object is up-to-date
			// These values will likely also get updated via the call to reset_FrameParameters (which then calls GetFrameParameters)
			m_mzCalibration.k = slope / 10000.0;
			m_mzCalibration.t0 = intercept * 10000.0;

			ResetFrameParameters();
		}

		/// <summary>
        /// Convert the array of bytes defining a fragmentation sequence to an array of doubles
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        private static double[] ArrayFragmentationSequence(byte[] blob)
        {
            double[] frag = new double[blob.Length / 8];

			for (int i = 0; i < frag.Length; i++)
			{
				frag[i] = BitConverter.ToDouble(blob, i*8);
			}

			return frag;
        }

		private Dictionary<string, string> CloneUIMFGetObjects(string sObjectType)
		{
			Dictionary<string, string> sObjects = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

			SQLiteCommand cmd = new SQLiteCommand(m_uimfDatabaseConnection) {CommandText = "SELECT name, sql FROM main.sqlite_master WHERE type='" + sObjectType + "' ORDER BY NAME"};

			using (SQLiteDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					sObjects.Add(Convert.ToString(reader["Name"]), Convert.ToString(reader["sql"]));
				}
			}

			return sObjects;
		}

    	private static bool ColumnIsMilliTorr(SQLiteCommand cmd, string tableName, string columnName)
		{
			bool isMillitorr = false;
			try
			{
                cmd.CommandText = "SELECT Avg(Pressure) AS AvgPressure FROM (SELECT " + columnName + " AS Pressure FROM " +
                                  tableName + " WHERE IFNULL(" + columnName + ", 0) > 0 ORDER BY FrameNum LIMIT 25) SubQ";

				object objResult = cmd.ExecuteScalar();
				if (objResult != null && objResult != DBNull.Value)
				{
					if (Convert.ToSingle(objResult) > 100)
					{
						isMillitorr = true;
					}
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception examining pressure column " + columnName + " in table " + tableName + ": " + ex.Message);
			}

			return isMillitorr;
		}

		/// <summary>
		/// Examines the pressure columns to determine whether they are in torr or mTorr
		/// </summary>
		internal void DeterminePressureUnits()
		{
			try
			{
				PressureIsMilliTorr = false;

				SQLiteCommand cmd = new SQLiteCommand(m_uimfDatabaseConnection);

				bool isMilliTorr = ColumnIsMilliTorr(cmd, "Frame_Parameters", "HighPressureFunnelPressure");
				if (isMilliTorr)
				{
				    PressureIsMilliTorr = true;
				    return;
				}

                isMilliTorr = ColumnIsMilliTorr(cmd, "Frame_Parameters", "PressureBack");
                if (isMilliTorr)
                {
                    PressureIsMilliTorr = true;
                    return;
                }

              
				isMilliTorr = ColumnIsMilliTorr(cmd, "Frame_Parameters", "IonFunnelTrapPressure");
				if (isMilliTorr)
				{
				    PressureIsMilliTorr = true;
				    return;
				}

				isMilliTorr = ColumnIsMilliTorr(cmd, "Frame_Parameters", "RearIonFunnelPressure");
				if (isMilliTorr) PressureIsMilliTorr = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception determining whether pressure columns are in milliTorr: " + ex.Message);
			}
		}

		/// <summary>
		/// Determines if the MS1 Frames of this file are labeled as 0 or 1. 
		/// Note that MS1 frames should recorded as '1'. But we need to
		/// support legacy UIMF files which have values of '0' for MS1. 
		/// The determined value is stored in a class-wide variable for later use.
		/// Exception is thrown if both 0 and 1 are found.
		/// </summary>
		private void DetermineFrameTypes()
		{
			List<int> frameTypeList = new List<int>();

			using (SQLiteCommand dbCmd = m_uimfDatabaseConnection.CreateCommand())
			{
				dbCmd.CommandText = "SELECT DISTINCT(FrameType) FROM Frame_Parameters";
				dbCmd.Prepare();
				using (SQLiteDataReader reader = dbCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						frameTypeList.Add(Convert.ToInt32(reader["FrameType"]));
					}
				}
			}

			if(frameTypeList.Contains(0))
			{
				if(frameTypeList.Contains(1))
				{
					throw new Exception("FrameTypes of 0 and 1 found. Not a valid UIMF file.");
				}

				m_frameTypeMs = 0;
			}
			else
			{
				m_frameTypeMs = 1;
			}
		}

		/// <summary>
		/// Returns the bin value that corresponds to an m/z value.  
		/// NOTE: this may not be accurate if the UIMF file uses polynomial calibration values  (eg.  FrameParameter A2)
		/// </summary>
		/// <param name="slope"></param>
		/// <param name="intercept"></param>
		/// <param name="binWidth"></param>
		/// <param name="correctionTimeForTOF"></param>
		/// <param name="targetMZ"></param>
		/// <returns></returns>
		private static double GetBinClosestToMZ(double slope, double intercept, double binWidth, double correctionTimeForTOF, double targetMZ)
		{
			//NOTE: this may not be accurate if the UIMF file uses polynomial calibration values  (eg.  FrameParameter A2)
			double binCorrection = (correctionTimeForTOF / 1000) / binWidth;
			double bin = (Math.Sqrt(targetMZ) / slope + intercept) / binWidth * 1000;
			//TODO:  have a test case with a TOFCorrectionTime > 0 and verify the binCorrection adjustment
			return bin + binCorrection;
		}

		private int[][][] GetIntensityBlock(int startFrameNumber, int endFrameNumber, FrameType frameType, int startScan, int endScan, int startBin, int endBin)
		{
			if (startBin < 0)
			{
				startBin = 0;
			}

			if (endBin > m_globalParameters.Bins)
			{
				endBin = m_globalParameters.Bins;
			}

			int lengthOfFrameArray = (endFrameNumber - startFrameNumber + 1);

			int[][][] intensities = new int[lengthOfFrameArray][][];
			for (int i = 0; i < lengthOfFrameArray; i++)
			{
				intensities[i] = new int[endScan - startScan + 1][];
				for (int j = 0; j < endScan - startScan + 1; j++)
				{
					intensities[i][j] = new int[endBin - startBin + 1];
				}
			}

			//now setup queries to retrieve data (April 2011 Note: there is probably a better query method for this)
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameNum1", startFrameNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameNum2", endFrameNumber));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("ScanNum1", startScan));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("ScanNum2", endScan));
			m_getSpectrumCommand.Parameters.Add(new SQLiteParameter("FrameType", (frameType.Equals(FrameType.MS1) ? m_frameTypeMs : (int)frameType)));
			
			using (SQLiteDataReader reader = m_getSpectrumCommand.ExecuteReader())
			{
				byte[] decompSpectraRecord = new byte[m_globalParameters.Bins * DATASIZE];

				while (reader.Read())
				{
					int frameNum = Convert.ToInt32(reader["FrameNum"]);
					int binIndex = 0;

					byte[] spectra = (byte[])(reader["Intensities"]);
					int scanNum = Convert.ToInt32(reader["ScanNum"]);

					//get frame number so that we can get the frame calibration parameters
					if (spectra.Length > 0)
					{
						int outputLength = LZFCompressionUtil.Decompress(ref spectra, spectra.Length, ref decompSpectraRecord, m_globalParameters.Bins * DATASIZE);
						int numBins = outputLength / DATASIZE;
						for (int i = 0; i < numBins; i++)
						{
							int decodedIntensityValue = BitConverter.ToInt32(decompSpectraRecord, i * DATASIZE);
							if (decodedIntensityValue < 0)
							{
								binIndex += -decodedIntensityValue;
							}
							else
							{
								if (startBin <= binIndex && binIndex <= endBin)
								{
									intensities[frameNum - startFrameNumber][scanNum - startScan][binIndex - startBin] = decodedIntensityValue;
								}
								binIndex++;
							}
						}
					}
				}
			}

			m_getSpectrumCommand.Parameters.Clear();

			return intensities;
		}

		/// <summary>
		/// Get TIC or BPI for scans of given frame type in given frame range
		/// Optionally filter on scan range
		/// </summary>
		/// <param name="frameType"></param>
		/// <param name="startFrameNumber"></param>
		/// <param name="endFrameNumber"></param>
		/// <param name="startScan"></param>
		/// <param name="endScan"></param>
		/// <param name="fieldName"></param>
		private double[] GetTicOrBpi(FrameType frameType, int startFrameNumber, int endFrameNumber, int startScan, int endScan, string fieldName)
		{
			// Make sure endFrame is valid
			if (endFrameNumber < startFrameNumber)
			{
				endFrameNumber = startFrameNumber;
			}

			// Compute the number of frames to be returned
			int nframes = endFrameNumber - startFrameNumber + 1;

			double[] data = new double[nframes];

			// Construct the SQL
			string sql = " SELECT Frame_Scans.FrameNum, Sum(Frame_Scans." + fieldName + ") AS Value " +
						 " FROM Frame_Scans INNER JOIN Frame_Parameters ON Frame_Scans.FrameNum = Frame_Parameters.FrameNum " +
						 " WHERE Frame_Parameters.FrameType = " + (frameType.Equals(FrameType.MS1) ? m_frameTypeMs : (int)frameType) + " AND " +
							   " Frame_Parameters.FrameNum >= " + startFrameNumber + " AND " +
							   " Frame_Parameters.FrameNum <= " + endFrameNumber;

			if (!(startScan == 0 && endScan == 0))
			{
				// Filter by scan number
				sql += " AND Frame_Scans.ScanNum >= " + startScan + " AND Frame_Scans.ScanNum <= " + endScan;
			}

			sql += " GROUP BY Frame_Scans.FrameNum ORDER BY Frame_Scans.FrameNum";

			using (SQLiteCommand dbcmdUIMF = m_uimfDatabaseConnection.CreateCommand())
			{
				dbcmdUIMF.CommandText = sql;
				using (SQLiteDataReader reader = dbcmdUIMF.ExecuteReader())
				{
					int ncount = 0;
					while (reader.Read())
					{
						data[ncount] = Convert.ToDouble(reader["Value"]);
						ncount++;
					}
				}
			}

			return data;
		}

		private int[] GetUpperLowerBinsFromMz(int frameNumber, double targetMZ, double toleranceInMZ)
		{
			int[] bins = new int[2];
			double lowerMZ = targetMZ - toleranceInMZ;
			double upperMZ = targetMZ + toleranceInMZ;
			FrameParameters fp = GetFrameParameters(frameNumber);
			GlobalParameters gp = GetGlobalParameters();
			bool polynomialCalibrantsAreUsed = (Math.Abs(fp.a2 - 0) > float.Epsilon || 
												Math.Abs(fp.b2 - 0) > float.Epsilon || 
												Math.Abs(fp.c2 - 0) > float.Epsilon || 
												Math.Abs(fp.d2 - 0) > float.Epsilon || 
												Math.Abs(fp.e2 - 0) > float.Epsilon || 
												Math.Abs(fp.f2 - 0) > float.Epsilon);
			if (polynomialCalibrantsAreUsed)
			{
				//note: the reason for this is that we are trying to get the closest bin for a given m/z.  But when a polynomial formula is used to adjust the m/z, it gets
				// much more complicated.  So someone else can figure that out  :)
				throw new NotImplementedException("DriftTime profile extraction hasn't been implemented for UIMF files containing polynomial calibration constants.");
			}

			double lowerBin = GetBinClosestToMZ(fp.CalibrationSlope, fp.CalibrationIntercept, gp.BinWidth, gp.TOFCorrectionTime, lowerMZ);
			double upperBin = GetBinClosestToMZ(fp.CalibrationSlope, fp.CalibrationIntercept, gp.BinWidth, gp.TOFCorrectionTime, upperMZ);
			bins[0] = (int)Math.Round(lowerBin, 0);
			bins[1] = (int)Math.Round(upperBin, 0);
			return bins;
		}

		private void LoadPrepStmts()
		{
			m_getFileBytesCommand = m_uimfDatabaseConnection.CreateCommand();

			m_getFrameNumbers = m_uimfDatabaseConnection.CreateCommand();
			m_getFrameNumbers.CommandText = "SELECT FrameNum FROM Frame_Parameters WHERE FrameType = :FrameType";
			m_getFrameNumbers.Prepare();

			m_getFrameParametersCommand = m_uimfDatabaseConnection.CreateCommand();
			m_getFrameParametersCommand.CommandText = "SELECT * FROM Frame_Parameters WHERE FrameNum = :FrameNum"; // FrameType not necessary
			m_getFrameParametersCommand.Prepare();

			// Table: Frame_Scans
			m_sumVariableScansPerFrameCommand = m_uimfDatabaseConnection.CreateCommand();

			m_getFramesAndScanByDescendingIntensityCommand = m_uimfDatabaseConnection.CreateCommand();
			m_getFramesAndScanByDescendingIntensityCommand.CommandText = "SELECT FrameNum, ScanNum, BPI FROM Frame_Scans ORDER BY BPI";
			m_getFramesAndScanByDescendingIntensityCommand.Prepare();

			m_getSpectrumCommand = m_uimfDatabaseConnection.CreateCommand();
			m_getSpectrumCommand.CommandText = "SELECT FS.ScanNum, FS.FrameNum, FS.Intensities FROM Frame_Scans FS JOIN Frame_Parameters FP ON (FS.FrameNum = FP.FrameNum) WHERE FS.FrameNum >= :FrameNum1 AND FS.FrameNum <= :FrameNum2 AND FS.ScanNum >= :ScanNum1 AND FS.ScanNum <= :ScanNum2 AND FP.FrameType = :FrameType";
			m_getSpectrumCommand.Prepare();

			m_getCountPerSpectrumCommand = m_uimfDatabaseConnection.CreateCommand();
			m_getCountPerSpectrumCommand.CommandText = "SELECT NonZeroCount FROM Frame_Scans WHERE FrameNum = :FrameNum AND ScanNum = :ScanNum";
			m_getCountPerSpectrumCommand.Prepare();

			m_getCountPerFrameCommand = m_uimfDatabaseConnection.CreateCommand();
			m_getCountPerFrameCommand.CommandText = "SELECT sum(NonZeroCount) FROM Frame_Scans WHERE FrameNum = :FrameNum AND NOT NonZeroCount IS NULL";
			m_getCountPerFrameCommand.Prepare();
		}

		private void PopulateFrameParameters(FrameParameters fp, SQLiteDataReader reader)
		{
			try
			{
				bool columnMissing;

				fp.FrameNum = Convert.ToInt32(reader["FrameNum"]);
				fp.StartTime = Convert.ToDouble(reader["StartTime"]);

				if (fp.StartTime > 1E+17)
				{
					// StartTime is stored as Ticks in this file
					// Auto-compute the correct start time
					DateTime dtRunStarted;
					if (DateTime.TryParse(m_globalParameters.DateStarted, out dtRunStarted))
					{
						Int64 lngTickDifference = (Int64)fp.StartTime - dtRunStarted.Ticks;
						if (lngTickDifference >= 0)
						{
							fp.StartTime = dtRunStarted.AddTicks(lngTickDifference).Subtract(dtRunStarted).TotalMinutes;
						}
					}
				}

				fp.Duration = Convert.ToDouble(reader["Duration"]);
				fp.Accumulations = Convert.ToInt32(reader["Accumulations"]);
				
				int frameTypeInt = Convert.ToInt16(reader["FrameType"]);

				// If the frametype is 0, then this is an older UIMF file where the MS1 frames were labeled as 0.
				if(frameTypeInt == 0)
				{
					fp.FrameType = FrameType.MS1;
				}
				else
				{
					fp.FrameType = (FrameType) frameTypeInt;
				}

				fp.Scans = Convert.ToInt32(reader["Scans"]);
				fp.IMFProfile = Convert.ToString(reader["IMFProfile"]);
				fp.TOFLosses = Convert.ToDouble(reader["TOFLosses"]);
				fp.AverageTOFLength = Convert.ToDouble(reader["AverageTOFLength"]);
				fp.CalibrationSlope = Convert.ToDouble(reader["CalibrationSlope"]);
				fp.CalibrationIntercept = Convert.ToDouble(reader["CalibrationIntercept"]);
				fp.Temperature = Convert.ToDouble(reader["Temperature"]);
				fp.voltHVRack1 = Convert.ToDouble(reader["voltHVRack1"]);
				fp.voltHVRack2 = Convert.ToDouble(reader["voltHVRack2"]);
				fp.voltHVRack3 = Convert.ToDouble(reader["voltHVRack3"]);
				fp.voltHVRack4 = Convert.ToDouble(reader["voltHVRack4"]);
				fp.voltCapInlet = Convert.ToDouble(reader["voltCapInlet"]);                // 14, Capillary Inlet Voltage

				fp.voltEntranceHPFIn = TryGetFrameParam(reader, "voltEntranceHPFIn", 0, out columnMissing); // 15, HPF In Voltage
				if (columnMissing)
				{
					// Legacy column names are present
					fp.voltEntranceHPFIn = TryGetFrameParam(reader, "voltEntranceIFTIn", 0);
					fp.voltEntranceHPFOut = TryGetFrameParam(reader, "voltEntranceIFTOut", 0);
				}
				else
				{
					fp.voltEntranceHPFOut = TryGetFrameParam(reader, "voltEntranceHPFOut", 0); // 16, HPF Out Voltage
				}

				fp.voltEntranceCondLmt = Convert.ToDouble(reader["voltEntranceCondLmt"]); // 17, Cond Limit Voltage
				fp.voltTrapOut = Convert.ToDouble(reader["voltTrapOut"]);                 // 18, Trap Out Voltage
				fp.voltTrapIn = Convert.ToDouble(reader["voltTrapIn"]);                   // 19, Trap In Voltage
				fp.voltJetDist = Convert.ToDouble(reader["voltJetDist"]);                 // 20, Jet Disruptor Voltage
				fp.voltQuad1 = Convert.ToDouble(reader["voltQuad1"]);                     // 21, Fragmentation Quadrupole Voltage
				fp.voltCond1 = Convert.ToDouble(reader["voltCond1"]);                     // 22, Fragmentation Conductance Voltage
				fp.voltQuad2 = Convert.ToDouble(reader["voltQuad2"]);                     // 23, Fragmentation Quadrupole Voltage
				fp.voltCond2 = Convert.ToDouble(reader["voltCond2"]);                     // 24, Fragmentation Conductance Voltage
				fp.voltIMSOut = Convert.ToDouble(reader["voltIMSOut"]);                   // 25, IMS Out Voltage

				fp.voltExitHPFIn = TryGetFrameParam(reader, "voltExitHPFIn", 0, out columnMissing); // 26, HPF In Voltage
				if (columnMissing)
				{
					// Legacy column names are present
					fp.voltExitHPFIn = TryGetFrameParam(reader, "voltExitIFTIn", 0);
					fp.voltExitHPFOut = TryGetFrameParam(reader, "voltExitIFTOut", 0);
				}
				else
				{
					fp.voltExitHPFOut = TryGetFrameParam(reader, "voltExitHPFOut", 0);      // 27, HPF Out Voltage
				}

				fp.voltExitCondLmt = Convert.ToDouble(reader["voltExitCondLmt"]);           // 28, Cond Limit Voltage
				fp.PressureFront = Convert.ToDouble(reader["PressureFront"]);
				fp.PressureBack = Convert.ToDouble(reader["PressureBack"]);
				fp.MPBitOrder = Convert.ToInt16(reader["MPBitOrder"]);
				fp.FragmentationProfile = ArrayFragmentationSequence((byte[])(reader["FragmentationProfile"]));

				fp.HighPressureFunnelPressure = TryGetFrameParam(reader, "HighPressureFunnelPressure", 0, out columnMissing);
				if (columnMissing)
				{
					if (m_errMessageCounter < 5)
					{
						Console.WriteLine("Warning: this UIMF file is created with an old version of IMF2UIMF (HighPressureFunnelPressure is missing from the Frame_Parameters table); please get the newest version from \\\\floyd\\software");
						m_errMessageCounter++;
					}
				}
				else
				{
					fp.IonFunnelTrapPressure = TryGetFrameParam(reader, "IonFunnelTrapPressure", 0);
					fp.RearIonFunnelPressure = TryGetFrameParam(reader, "RearIonFunnelPressure", 0);
					fp.QuadrupolePressure = TryGetFrameParam(reader, "QuadrupolePressure", 0);
					fp.ESIVoltage = TryGetFrameParam(reader, "ESIVoltage", 0);
					fp.FloatVoltage = TryGetFrameParam(reader, "FloatVoltage", 0);
					fp.CalibrationDone = TryGetFrameParamInt32(reader, "CalibrationDone", 0);
					fp.Decoded = TryGetFrameParamInt32(reader, "Decoded", 0);

					if (PressureIsMilliTorr)
					{
						// Divide each of the pressures by 1000 to convert from milliTorr to Torr
						fp.HighPressureFunnelPressure /= 1000.0;
						fp.IonFunnelTrapPressure /= 1000.0;
						fp.RearIonFunnelPressure /= 1000.0;
						fp.QuadrupolePressure /= 1000.0;
					}
				}


				fp.a2 = TryGetFrameParam(reader, "a2", 0, out columnMissing);
				if (columnMissing)
				{
					fp.b2 = 0;
					fp.c2 = 0;
					fp.d2 = 0;
					fp.e2 = 0;
					fp.f2 = 0;
					if (m_errMessageCounter < 5)
					{
						Console.WriteLine("Warning: this UIMF file is created with an old version of IMF2UIMF (b2 calibration column is missing from the Frame_Parameters table); please get the newest version from \\\\floyd\\software");
						m_errMessageCounter++;
					}
				}
				else
				{
					fp.b2 = TryGetFrameParam(reader, "b2", 0);
					fp.c2 = TryGetFrameParam(reader, "c2", 0);
					fp.d2 = TryGetFrameParam(reader, "d2", 0);
					fp.e2 = TryGetFrameParam(reader, "e2", 0);
					fp.f2 = TryGetFrameParam(reader, "f2", 0);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to access frame parameters table " + ex);
			}
		}

		private static void StripZerosFromArrays<T>(int nonZeroCount, ref T[] xDataArray, ref int[] yDataArray)
		{
			List<T> xArrayList = new List<T>(nonZeroCount);
			List<int> yArrayList = new List<int>(nonZeroCount);

			for (int i = 0; i < xDataArray.Length; i++)
			{
				int yDataPoint = yDataArray[i];

				if (yDataPoint > 0)
				{
					xArrayList.Add(xDataArray[i]);
					yArrayList.Add(yDataPoint);
				}
			}

			xDataArray = xArrayList.ToArray();
			yDataArray = yArrayList.ToArray();
		}

		private static double TryGetFrameParam(SQLiteDataReader reader, string columnName, double defaultValue)
		{
			bool columnMissing;
			return TryGetFrameParam(reader, columnName, defaultValue, out columnMissing);
		}

		private static double TryGetFrameParam(SQLiteDataReader reader, string columnName, double defaultValue, out bool columnMissing)
		{
			double result = defaultValue;
			columnMissing = false;

			try
			{
				result = !DBNull.Value.Equals(reader[columnName]) ? Convert.ToDouble(reader[columnName]) : defaultValue;
			}
			catch (IndexOutOfRangeException)
			{
				columnMissing = true;
			}

			return result;
		}

		private static int TryGetFrameParamInt32(SQLiteDataReader reader, string columnName, int defaultValue)
		{
			bool columnMissing;
			return TryGetFrameParamInt32(reader, columnName, defaultValue, out columnMissing);
		}

		private static int TryGetFrameParamInt32(SQLiteDataReader reader, string columnName, int defaultValue, out bool columnMissing)
		{
			int result = defaultValue;
			columnMissing = false;

			try
			{
				result = !DBNull.Value.Equals(reader[columnName]) ? Convert.ToInt32(reader[columnName]) : defaultValue;
			}
			catch (IndexOutOfRangeException)
			{
				columnMissing = true;
			}

			return result;
		}

		private void UnloadPrepStmts()
		{
			if (m_getCountPerSpectrumCommand != null) m_getCountPerSpectrumCommand.Dispose();

			if (m_getCountPerFrameCommand != null) m_getCountPerFrameCommand.Dispose();

			if (m_getFileBytesCommand != null) m_getFileBytesCommand.Dispose();

			if (m_getFrameNumbers != null) m_getFrameNumbers.Dispose();

			if (m_getFrameParametersCommand != null) m_getFrameParametersCommand.Dispose();

			if (m_getFramesAndScanByDescendingIntensityCommand != null) m_getFramesAndScanByDescendingIntensityCommand.Dispose();

			if (m_getSpectrumCommand != null) m_getSpectrumCommand.Dispose();

			if (m_sumVariableScansPerFrameCommand != null) m_sumVariableScansPerFrameCommand.Dispose();
		}

		#region IDisposable Members

		public void Dispose()
		{
			try
			{
				UnloadPrepStmts();

				if (m_uimfDatabaseConnection != null)
				{
					m_uimfDatabaseConnection.Close();
					m_uimfDatabaseConnection.Dispose();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to close UIMF file " + ex);
			}
		}

		#endregion
	}

}
