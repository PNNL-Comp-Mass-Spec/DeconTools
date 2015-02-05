using System;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
#if !Disable_DeconToolsV2
using DeconToolsV2.Readers;
#endif

namespace DeconTools.Backend.Runs
{
#if !Disable_DeconToolsV2

    public class ICR2LSRun : DeconToolsRun
    {

        #region Constructors
        public ICR2LSRun()
        {
            this.IsDataThresholded = false;   //TODO: check this

        }

        public ICR2LSRun(string filename)
            : this()
        {
            Filename = filename;

            try
            {
                RawData = new clsRawData(filename, FileType.ICR2LSRAWDATA);
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }
            MinLCScan = 1; //  remember that DeconEngine is 1-based
            MaxLCScan = GetMaxPossibleLCScanNum();


        }

        public ICR2LSRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            throw new NotImplementedException("Can't define scanRange yet - needs further development");

        }
        #endregion

        #region Public Methods

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans();
        }


        public override XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            Check.Require(scanset != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanset.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            int totScans = this.GetNumMSScans();


            double[] xvals = new double[0];
            double[] yvals = new double[0];

            if (scanset.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {

                //TODO:  Old DeconTools reference!!
                this.RawData.GetSpectrum(scanset.IndexValues[0], ref xvals, ref yvals);
            }
            else
            {
                throw new NotImplementedException("Summing is not yet supported for ICR2LS_MS Data files.");
                //this.rawData.GetSummedSpectra(scanset.getLowestScanNumber(), scanset.getHighestScanNumber(), minMZ, maxMZ, ref xvals, ref yvals);
            }

            XYData xydata = new XYData();
            xydata.Xvalues = xvals;
            xydata.Yvalues = yvals;

            xydata = xydata.TrimData(minMZ, maxMZ);
            return xydata;
        }
        #endregion


    }
#endif
}
