using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    public class ICR2LSRun : DeconToolsRun
    {

        #region Constructors
        public ICR2LSRun()
        {
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.IsDataThresholded = false;   //TODO: check this

        }

        public ICR2LSRun(string filename)
            : this()
        {
            this.Filename = filename;


            try
            {

                this.RawData = new DeconToolsV2.Readers.clsRawData(filename, DeconToolsV2.Readers.FileType.ICR2LSRAWDATA);
            }
            catch (Exception ex)
            {

                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }
            this.MinScan = 1;        //  remember that DeconEngine is 1-based
            this.MaxScan = GetMaxPossibleScanIndex();


        }

        public ICR2LSRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            throw new NotImplementedException("Can't define scanRange yet - needs further development");

        }
        #endregion

        #region Public Methods
        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            Check.Require(scanset != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanset.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            int totScans = this.GetNumMSScans();


            double[] xvals = new double[0];
            double[] yvals = new double[0];

            if (scanset.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {


                this.RawData.GetSpectrum(scanset.IndexValues[0], ref xvals, ref yvals);
            }
            else
            {
                throw new NotImplementedException("Summing is not yet supported for ICR2LS_MS Data files.");
                //this.rawData.GetSummedSpectra(scanset.getLowestScanNumber(), scanset.getHighestScanNumber(), minMZ, maxMZ, ref xvals, ref yvals);
            }

            this.XYData.SetXYValues(ref xvals, ref yvals);
            this.FilterXYPointsByMZRange(minMZ, maxMZ);
        }
        #endregion


    }
}
