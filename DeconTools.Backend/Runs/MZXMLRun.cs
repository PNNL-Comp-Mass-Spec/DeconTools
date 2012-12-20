using System;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Runs
{
    [Obsolete("Not used anymore. Use 'MzRun'",true)]
    public sealed class MZXMLRun : DeconToolsRun
    {

        #region Constructors

        public MZXMLRun()
        {
            IsDataThresholded = true;      //TODO:   We should not hard-code this. Needs to be put in parameter file. Peak detector uses this info.
            MSFileType = Globals.MSFileType.MZXML_Rawdata;
        }

        public MZXMLRun(string filename)
            : this()
        {
            Filename = filename;

            try
            {

                RawData = new DeconToolsV2.Readers.clsRawData(filename, DeconToolsV2.Readers.FileType.MZXMLRAWDATA);
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR:  Couldn't open the file.  Details: " + ex.Message);
            }
            
            MinLCScan = GetMinPossibleLCScanNum();        
            MaxLCScan = GetMaxPossibleLCScanNum();

        }

        public MZXMLRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            MinLCScan = minScan;
            MaxLCScan = maxScan;
        }


        #endregion

        #region Public Methods

        public override int GetMinPossibleLCScanNum()
        {
            return 1;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans() - 1;      // this is tricky...  some mzXML files might be 1-based;  others might be 0-based. So I will play it safe and go zero-based
        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            Check.Require(scanset != null, "Can't get mass spectrum; inputted set of scans is null");
            Check.Require(scanset.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            int totScans = GetNumMSScans();

            double[] xvals = new double[0];
            double[] yvals = new double[0];

            if (scanset.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {
                RawData.GetSpectrum(scanset.IndexValues[0], ref xvals, ref yvals);
            }
            else
            {
                throw new NotImplementedException("Summing is not yet supported for MZXML files.");
                //this.rawData.GetSummedSpectra(scanset.getLowestScanNumber(), scanset.getHighestScanNumber(), minMZ, maxMZ, ref xvals, ref yvals);
            }


            XYData.SetXYValues(ref xvals, ref yvals);
        }

        #endregion

    }
}
