#if !Disable_DeconToolsV2
using System;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconToolsV2.Readers;

namespace DeconTools.Backend.Runs
{
    [Serializable]
    public sealed class IMFRun : DeconToolsRun
    {
        public IMFRun()
        {
            XYData = new XYData();
            MSFileType = Globals.MSFileType.PNNL_IMS;
        }

        public IMFRun(string filename)
            : this()
        {
            Check.Require(File.Exists(filename),"File does not exist");

            Filename = filename;

            var baseFilename = Path.GetFileName(Filename);
            if (baseFilename == null)
                throw new Exception("Could not determine the path to the input file");

            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DataSetPath = Path.GetDirectoryName(filename);

#pragma warning disable 618
            RawData = new clsRawData(filename, FileType.PNNL_IMS);
#pragma warning restore 618
            MinLCScan = GetMinPossibleLCScanNum();
            MaxLCScan = GetMaxPossibleLCScanNum();
        }

        public IMFRun(string filename, int minScan, int maxScan)
            : this(filename)
        {
            MinLCScan = minScan;
            MaxLCScan = maxScan;
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 0;
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans() - 1;
        }

        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            Check.Require(scanSet != null, "Can't get mass spectrum; inputted set of scans is null");
            if (scanSet == null)
                return new XYData();

            Check.Require(scanSet.IndexValues.Count > 0, "Can't get mass spectrum; no scan numbers inputted");

            // Unused: var totScans = GetNumMSScans();

            var xVals = new double[0];
            var yVals = new double[0];

            //if (scanSet.IndexValues.Count == 1)            //this is the case of only wanting one MS spectrum
            //{
            //    this.rawData.GetSpectrum(scanSet.IndexValues[0], ref xVals, ref yVals);
            //}
            //else
            //{
            //    int upperScan = Math.Min(scanSet.GetHighestScanNumber(), this.GetNumMSScans());
            //    int lowerScan = Math.Max(scanSet.GetLowestScanNumber(), 1);
            //    this.rawData.GetSummedSpectra(lowerScan, upperScan, minMZ, maxMZ, ref xVals, ref yVals);
            //}

            var upperScan = Math.Min(scanSet.GetHighestScanNumber(), GetNumMSScans());
            var lowerScan = Math.Max(scanSet.GetLowestScanNumber(), 1);

            //TODO:  Old DeconTools reference!! remove this
            RawData.GetSummedSpectra(lowerScan, upperScan, minMZ, maxMZ, ref xVals, ref yVals);

            var xydata = new XYData
            {
                Xvalues = xVals,
                Yvalues = yVals
            };

            return xydata;
        }
    }
}
#endif
