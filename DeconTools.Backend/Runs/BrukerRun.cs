using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using System.IO;

namespace DeconTools.Backend.Runs
{
    public class BrukerRun : Run
    {

        public BrukerRun()
        {
            this.xyData = new XYData();
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.IsDataThresholded = true;
            this.MSFileType = Globals.MSFileType.Bruker;
            this.ContainsMSMSData = false;
        }

        public BrukerRun(string folderName)
            : this()
        {
            this.Filename = folderName;
            this.DatasetName = getDataSetName(this.Filename);
            this.DataSetPath = getDataSetFolderName(this.Filename);

            validateFileNameAndFolderStructure();


            try
            {
                this.rawData = new DeconToolsV2.Readers.clsRawData();
                this.rawData.LoadFile(this.Filename, DeconToolsV2.Readers.FileType.BRUKER);
            }
            catch (Exception ex)
            {

                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }
            this.MinScan = 1;        //  remember that DeconEngine is 1-based
            this.MaxScan = GetMaxPossibleScanIndex();
 

        }

        private void validateFileNameAndFolderStructure()
        {
            //check if the datasetPath is the same as the FileName, if so, change the Filename, or else DeconEngine will fail
            if (this.DataSetPath == this.Filename)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(this.Filename);

                DirectoryInfo[] childFolders = dirinfo.GetDirectories();

                DirectoryInfo serFolder=null;

                foreach (var folder in childFolders)
                {
                    if (folder.Name.EndsWith("0.ser", StringComparison.OrdinalIgnoreCase))
                    {
                        serFolder=folder;
                        break;
                    }
                }

                if (serFolder!=null)
                {
                    this.Filename = serFolder.FullName;
                    
                }
            }


        }

        private string getDataSetFolderName(string p)
        {
            string trimmedPath = p.TrimEnd('\\');

            if (trimmedPath.EndsWith("acqus", StringComparison.OrdinalIgnoreCase))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(p);
                return dirinfo.Parent.Parent.FullName;
            }
            else if (trimmedPath.EndsWith("0.ser", StringComparison.OrdinalIgnoreCase))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(p);
                return dirinfo.Parent.FullName;
            }
            else
            {
                return trimmedPath;
            }
        }

        private string getDataSetName(string filename)
        {


            string trimmedfilename = filename.TrimEnd(new char[] { '\\' });

            if (trimmedfilename.EndsWith("0.ser", StringComparison.OrdinalIgnoreCase))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(trimmedfilename);
                trimmedfilename = dirinfo.Parent.FullName.TrimEnd('\\');
            }


            if (trimmedfilename.EndsWith("acqus", StringComparison.OrdinalIgnoreCase))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(trimmedfilename);
                trimmedfilename = dirinfo.Parent.Parent.FullName.TrimEnd('\\');
            }

           

            int pathIndex = trimmedfilename.LastIndexOf('\\');
            if (pathIndex == -1) return String.Empty;



            return trimmedfilename.Substring((pathIndex+1), (trimmedfilename.Length - pathIndex-1));


        }

        public BrukerRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            this.MinScan = minScan;
            this.MaxScan = maxScan;
        }


        #region Properties

        [field: NonSerialized]
        private XYData xyData;
        public override XYData XYData
        {
            get
            {
                return xyData;
            }
            set
            {
                xyData = value;
            }
        }

        [field: NonSerialized]
        private DeconToolsV2.Readers.clsRawData rawData;
        public DeconToolsV2.Readers.clsRawData RawData
        {
            get { return rawData; }
            set { rawData = value; }
        }



        #endregion


        #region Methods
        public override int GetNumMSScans()
        {
            if (rawData == null) return 0;
            return this.rawData.GetNumScans();
        }

        public override void GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            int totScans = this.GetNumMSScans();

            double[] xvals = new double[0];
            double[] yvals = new double[0];

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                this.rawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);
            }
            else    // need to sum spectra
            {
                //assume:  each scan has exactly same x values
                
                //get first spectrum
                this.rawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals);

                //
                double[] summedYvals = new double[xvals.Length];
                yvals.CopyTo(summedYvals, 0);

                for (int i = 1; i < scanSet.IndexValues.Count; i++)
                {
                    this.rawData.GetSpectrum(scanSet.IndexValues[i], ref xvals, ref yvals);

                    for (int n = 0; n < xvals.Length; n++)
                    {
                        summedYvals[n] += yvals[n];
                    }
                }

                yvals = summedYvals;
            }
            this.xyData.SetXYValues(ref xvals, ref yvals);

            if (xyData.Xvalues == null || xyData.Xvalues.Length == 0) return;
            bool needsFiltering = (minMZ > this.xyData.Xvalues[0] || maxMZ < this.xyData.Xvalues[this.xyData.Xvalues.Length - 1]);
            if (needsFiltering)
            {
                this.FilterXYPointsByMZRange(minMZ, maxMZ);
            }

        }

        public override double GetTime(int scanNum)
        {
            return this.rawData.GetScanTime(scanNum);
        }


        internal override int GetMaxPossibleScanIndex()
        {
            return this.GetNumMSScans();
        }
        #endregion






        public override int GetMSLevelFromRawData(int scanNum)
        {

            if (!ContainsMSMSData) return 1;    // if we know the run doesn't contain MS/MS data, don't waste time checking
            int mslevel =(byte)this.rawData.GetMSLevel(scanNum);

            addToMSLevelData(scanNum, mslevel);
 
            return mslevel;
        }

  
    }
}
