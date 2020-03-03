#if !Disable_DeconToolsV2
using System;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconToolsV2.Readers;

namespace DeconTools.Backend.Runs
{
    public class ICR2LSRun : DeconToolsRun
    {
#region Constructors
        public ICR2LSRun()
        {
            IsDataThresholded = false;   //TODO: check this
        }

        public ICR2LSRun(string filename)
            : this()
        {
            DatasetFileOrDirectoryPath = filename;

            try
            {
#pragma warning disable 618
                RawData = new clsRawData(filename, FileType.ICR2LSRAWDATA);
#pragma warning restore 618
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

        public sealed override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans();
        }

        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            if (scanSet == null)
                return new XYData();

            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            // Unused: var totScans = GetNumMSScans();

            var xvals = new double[0];
            var yvals = new double[0];

            if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            {

                //TODO:  Old DeconTools reference!!
                RawData.GetSpectrum(scanSet.IndexValues[0], ref xvals, ref yvals, false);
            }
            else
            {
                throw new NotImplementedException("Summing is not yet supported for ICR2LS_MS Data files.");
                //this.rawData.GetSummedSpectra(scanSet.GetLowestScanNumber(), scanSet.GetHighestScanNumber(), minMZ, maxMZ, ref xVals, ref yVals);
            }

            var xydata = new XYData
            {
                Xvalues = xvals,
                Yvalues = yvals
            };

            xydata = xydata.TrimData(minMZ, maxMZ);
            return xydata;
        }
#endregion
    }
}
#endif
