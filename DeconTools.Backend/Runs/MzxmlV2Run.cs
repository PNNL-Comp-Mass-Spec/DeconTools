using System;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Runs
{
    public sealed class MzxmlV2Run : Run
    {

        #region Constructors

        public MzxmlV2Run()
        {

        }


        public MzxmlV2Run(string fileName)
            : this()
        {
            MSFileType = Globals.MSFileType.MZXML_Rawdata;


            Filename = fileName;


            MinScan = GetMinPossibleScanNum();
            MaxScan = GetMaxPossibleScanNum();

        }


        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override XYData XYData
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override int GetNumMSScans()
        {
            throw new NotImplementedException();
        }

        public override double GetTime(int scanNum)
        {
            throw new NotImplementedException();
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            throw new NotImplementedException();
        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException();
        }

        public override int GetMinPossibleScanNum()
        {
            return 0;
        }

        public override int GetMaxPossibleScanNum()
        {
            return GetNumMSScans();
        }
    }
}
