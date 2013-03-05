using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agilent.MassSpectrometry.DataAnalysis;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using PNNLOmics.Data;

namespace DeconTools.Backend.Runs
{
    public sealed class AgilentDRun : Run
    {
        #region Constructors

        IMsdrDataReader m_reader;
        IBDASpecData m_spec;

        /// <summary>
        /// Agilent XCT .D datafolder
        /// </summary>
        /// <param name="folderName">The name of the Agilent data folder. Folder has a '.d' suffix</param>

        public AgilentDRun()
        {
            this.MSFileType = Globals.MSFileType.Agilent_D;
            this.ContainsMSMSData = true;    //not sure if it does, but setting it to 'true' ensures that each scan will be checked. 
        }

        public AgilentDRun(string dataFileName)
            : this()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dataFileName);
            FileInfo fileInfo = new FileInfo(dataFileName);

            Check.Require(!fileInfo.Exists, "Dataset's inputted name refers to a file, but should refer to a Folder");
            Check.Require(dirInfo.Exists, "Dataset not found.");

            Check.Require(dirInfo.FullName.EndsWith("d", StringComparison.OrdinalIgnoreCase), "Agilent_D dataset folders must end with with the suffix '.d'. Check your folder name.");


            Filename = dirInfo.FullName;
            DatasetName = dirInfo.Name.Substring(0, dirInfo.Name.LastIndexOf(".d", StringComparison.OrdinalIgnoreCase));
            //get dataset name without .d extension
            DataSetPath = dirInfo.FullName;

            OpenDataset();

            MinLCScan = GetMinPossibleLCScanNum();
            MaxLCScan = GetMaxPossibleLCScanNum();
        }


        public AgilentDRun(string dataFileName, int minScan, int maxScan)
            : this(dataFileName)
        {
            MinLCScan = minScan;
            MaxLCScan = maxScan;

        }



        private void OpenDataset()
        {
            m_reader = new MassSpecDataReader();
            m_reader.OpenDataFile(this.Filename);
        }




        #endregion

        #region Properties
        public override XYData XYData { get; set; }

        #endregion

        #region Public Methods
        public override int GetMinPossibleLCScanNum()
        {
            // AgilentD files are 0-based.
            return 0;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans() - 1;
        }

        public override int GetNumMSScans()
        {
            //m_reader=new MassSpecDataReader();
            IBDAMSScanFileInformation msscan = m_reader.FileInformation.MSScanFileInformation;
            return (int)msscan.TotalScansPresent;
        }

        public override double GetTime(int scanNum)
        {
            double time = -1;

            if (m_spec == null || m_spec.ScanId == scanNum)    // get fresh spectrum
            {
                getAgilentSpectrum(scanNum);
            }

            if (m_spec == null) return -1;

            IRange[] timeRangeArr = m_spec.AcquiredTimeRange;
            if (timeRangeArr != null && timeRangeArr.Length == 1)
            {
                time = timeRangeArr[0].Start;
            }
            return time;
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum, null, null);

            MSLevel level = m_spec.MSLevelInfo;
            if (level == MSLevel.MS)
            {
                return 1;
            }
            else if (level == MSLevel.MSMS)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        public override PrecursorInfo GetPrecursorInfo(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum, null, null);

            PrecursorInfo precursor = new PrecursorInfo();

            MSLevel level = m_spec.MSLevelInfo;
            if (level == MSLevel.MS)
            {
                precursor.MSLevel = 1;
            }
            else if (level == MSLevel.MSMS)
            {
                precursor.MSLevel = 2;
            }
            else
            {
                precursor.MSLevel = 1;
            }

            int precursorMassCount;
            double precursorIntensity;
            int precursorCharge;
            bool getCharge;

            //this returns a list of precursor masses (not sure how there can be more than one)
            double[] precursorMZlist = m_spec.GetPrecursorIon(out precursorMassCount);

            //if a mass is returned
            if (precursorMassCount == 1)
            {
                //mass
                precursor.PrecursorMZ = precursorMZlist[0];

                //intensity
                m_spec.GetPrecursorIntensity(out precursorIntensity);
                precursor.PrecursorIntensity = (float)precursorIntensity;

                //charge
                getCharge = m_spec.GetPrecursorCharge(out precursorCharge);
                precursor.PrecursorCharge = precursorCharge;

                //adjust scan number if needed
                precursor.PrecursorScan = scanNum;


            }
            else if (precursorMassCount > 1)
            {

                throw new NotImplementedException("Strange case where more than one precursor is used to generate one spectra");

            }
            else
            {
                precursor.PrecursorMZ = 0;
                precursor.PrecursorIntensity = 0;
                precursor.PrecursorCharge = -1;
                precursor.PrecursorScan = scanNum;
            }
            return precursor;
        }



        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            if (scanSet == null) return null;

            XYData xydata = new XYData();
            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                getAgilentSpectrum(scanSet.PrimaryScanNumber);

                double[] xvals = m_spec.XArray;
                float[] yvals = m_spec.YArray;

                bool filterMassRange = true;
                FilterMassRange(minMZ, maxMZ, ref xvals, ref yvals, filterMassRange);

                
                xydata.Xvalues = xvals;
                xydata.Yvalues = yvals.Select(p=>(double)p).ToArray();
            }
            else
            {
                //throw new NotImplementedException("Summing isn't supported for Agilent.D files - yet");

                //this is an implementation of Anuj's summing algorithm 4-2-2012.  
                double[] xvals = null;
                float[] yvals = null;
                
                getSummedSpectrum(scanSet, ref xvals, ref yvals, minMZ, maxMZ);

                bool filterMassRange = true;
                FilterMassRange(minMZ, maxMZ, ref xvals, ref yvals, filterMassRange);

                xydata.Xvalues = xvals;
                xydata.Yvalues = yvals.Select(p => (double)p).ToArray();
            }

            return xydata;

        }

        private static void FilterMassRange(double minMZ, double maxMZ, ref double[] xvals, ref float[] yvals, bool filterMassRange)
        {
            if (filterMassRange)
            {
                List<double> xvalsShortened = new List<double>();
                List<float> yvalsShortened = new List<float>();
                double tempMass = 0;
                for (int i = 0; i < xvals.Length; i++)
                {
                    tempMass = xvals[i];
                    if (tempMass > minMZ && tempMass < maxMZ)
                    {
                        xvalsShortened.Add(xvals[i]);
                        yvalsShortened.Add(yvals[i]);
                    }
                }
                xvals = xvalsShortened.ToArray();
                yvals = yvalsShortened.ToArray();
            }
        }

        public override XYData GetMassSpectrum(ScanSet scanSet)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            if (scanSet == null) return null;

            XYData xydata=new XYData();

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                getAgilentSpectrum(scanSet.PrimaryScanNumber);

                xydata.Xvalues = m_spec.XArray;
                xydata.Yvalues = m_spec.YArray.Select(p=>(double)p).ToArray();
                
            }
            else
            {
                //throw new NotImplementedException("Summing isn't supported for Agilent.D files - yet");

                //this is an implementation of Anuj's summing algorithm 4-2-2012.  
                double[] xvals = null;
                float[] yvals = null;
                double minMZ = 0; double maxMZ = 0;
                getSummedSpectrum(scanSet, ref xvals, ref yvals, minMZ, maxMZ);
                xydata.Xvalues = xvals;
                xydata.Yvalues = yvals.Select(p => (double)p).ToArray();
                
            }

            return xydata;
        }

        #endregion

        #region scott

        public void getSummedSpectrum(ScanSet scanSet, ref double[] xvals, ref float[] yvals, double minMZ, double maxMZ)
        {
            // [gord] idea borrowed from Anuj! Jan 2010 [scott] brought back for agilent data that is evenly spaced

            //the idea is to convert the mz value to a integer. To avoid losing precision, we multiply it by 'precision'
            //the integer is added to a dictionary generic list (sorted)
            //

            SortedDictionary<long, float> mz_intensityPair = new SortedDictionary<long, float>();
            SortedDictionary<long, float> mz_intensityPairFiltered = new SortedDictionary<long, float>();
            double precision = 1e6;   // if the precision is set too high, can get artifacts in which the intensities for two m/z values should be added but are separately registered. 
            double[] tempXvals = new double[0];
            float[] tempYvals = new float[0];

            //long minXLong = (long)(minMZ * precision + 0.5);
            //long maxXLong = (long)(maxMZ * precision + 0.5);
            for (int scanCounter = 0; scanCounter < scanSet.IndexValues.Count; scanCounter++)
            {
                //this.RawData.GetSpectrum(scanSet.IndexValues[scanCounter], ref tempXvals, ref tempYvals);
                //m_reader.GetSpectrum(this.SpectraID, scanSet.IndexValues[scanCounter], ref tempXvals, ref tempYvals);
                getAgilentSpectrum(scanSet.IndexValues[0] + scanCounter);
                tempXvals = m_spec.XArray;
                tempYvals = m_spec.YArray;

                for (int i = 0; i < tempXvals.Length; i++)
                {
                    long tempmz = (long)Math.Floor(tempXvals[i] * precision + 0.5);
                    //if (tempmz < minXLong || tempmz > maxXLong) continue;

                    if (mz_intensityPair.ContainsKey(tempmz))
                    {
                        mz_intensityPair[tempmz] += tempYvals[i];
                    }
                    else
                    {
                        mz_intensityPair.Add(tempmz, tempYvals[i]);
                    }
                }
            }

            if (mz_intensityPair.Count == 0) return;

            List<long> summedXVals = mz_intensityPair.Keys.ToList();
            xvals = new double[summedXVals.Count];
            yvals = mz_intensityPair.Values.ToArray();
           
            for (int i = 0; i < summedXVals.Count; i++)
            {
                xvals[i] = summedXVals[i] / precision;
            }
        }

        #endregion


        #region Private Methods
        private void getAgilentSpectrum(int scanNum)
        {
            m_spec = m_reader.GetSpectrum(scanNum, null, null, DesiredMSStorageType.ProfileElsePeak);
        }




        #endregion

        public override void Dispose()
        {

            if (m_reader != null)
            {
                try
                {
                    m_reader.CloseDataFile();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred when trying to close AgilentD file: " + this.DatasetName + "\nDetails: " + ex.Message);

                }

            }
            base.Dispose();
        }
    }
}
