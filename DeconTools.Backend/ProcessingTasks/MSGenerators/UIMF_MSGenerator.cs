using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMF_MSGenerator : I_MSGenerator
    {

        private FrameSet frameSet;
        private ScanSet scanSet;

        public UIMF_MSGenerator()
            : this(0, 5000)
        {

        }

        public UIMF_MSGenerator(double minMZ, double maxMZ)
        {
            Check.Require(minMZ <= maxMZ, "MS Generator failed. MinMZ must be less than or equal to maxMZ");
            //Check.Require(minMZ >=10,"MS Generator failed. MinMZ should be equal or greater than 10. This is due to a problem in the UIMFLibrary that sometimes returns m/z values of 0.000. This will be fixed later.");
            this.MinMZ = minMZ;
            this.MaxMZ= maxMZ;
        }


        public override double MinMZ
        {
            get
            {
                return base.MinMZ;
            }
            set
            {
                if (value < 10)
                {
                    //throw new Exception("MS Generator failed. MinMZ should be greater than 10.\nIf using the xml parameter file, see the 'UseMZRange' and 'MinMZ' parameters.\nThis deficiency will be fixed later.");
                }
                base.MinMZ = value;
            }
        }


        public override void GenerateMS(Run run)
        {
            Check.Require(run is UIMFRun, "UIMF_MSGenerator can only be used with UIMF files");
            Check.Require(run.CurrentScanSet != null, "Cannot generate MS. Target ScanSet ('CurrentScanSet') has not been assigned to the Run");


            UIMFRun uimfRun = (UIMFRun)(run);
            Check.Require(uimfRun.CurrentFrameSet != null, "Cannot generate MS. Target FrameSet ('CurrentFrameSet') has not been assigned to the Run");
            Check.Require(uimfRun.FrameSetCollection != null, "UIMF_MSGenerator failed... FrameSetCollection is null");



            bool containsScans = (uimfRun.CurrentScanSet.Count() > 0);
            Check.Require(containsScans, "Error: there are no scans defined for the UIMF Run");

            bool requestOneOrMoreScansButNoFrames = (containsScans && uimfRun.CurrentFrameSet.Count() == 0);
            bool requestScansAndFrames = (containsScans && uimfRun.CurrentFrameSet.Count() > 0);

            if (requestOneOrMoreScansButNoFrames)
            {
                uimfRun.GetMassSpectrum(uimfRun.CurrentScanSet, this.MinMZ, this.MaxMZ);
            }
            if (requestScansAndFrames)
            {
                uimfRun.GetMassSpectrum(uimfRun.CurrentScanSet, uimfRun.CurrentFrameSet, this.MinMZ, this.MaxMZ);
            }
            
            if (uimfRun.XYData.Xvalues == null || uimfRun.XYData.Xvalues.Length == 0)
            {
                uimfRun.XYData.Xvalues = new double[1];
                uimfRun.XYData.Yvalues = new double[1];
            }

        }

        protected override void createNewScanResult(DeconTools.Backend.Core.ResultCollection resultList, DeconTools.Backend.Core.ScanSet scanSet)
        {
            Check.Require(resultList.Run is UIMFRun, "UIMF_MSGenerator can only be used with UIMF files");
            Check.Require(resultList.Run.ScanSetCollection != null && resultList.Run.ScanSetCollection.ScanSetList.Count > 0, "MS Generator failed...ScanSetCollection is empty");
            UIMFRun uimfRun = (UIMFRun)(resultList.Run);

            if (uimfRun.CurrentScanSet == uimfRun.ScanSetCollection.ScanSetList[0])    //only one scanResult is created per frame
            {
                resultList.ScanResultList.Add(new UIMFScanResult(uimfRun.CurrentFrameSet));
                UIMFScanResult scanresult = (UIMFScanResult)(resultList.GetCurrentScanResult());

                scanresult.ScanTime = resultList.Run.GetTime(uimfRun.CurrentFrameSet.PrimaryFrame);
                resultList.GetCurrentScanResult().SpectrumType = resultList.Run.GetMSLevel(scanSet.PrimaryScanNumber);  //TODO: need to find out if this is 'ims_scan_number'(0-599) or the absolute scan num (0-1439999);

                scanresult.FramePressureFront = uimfRun.GetFramePressureFront(uimfRun.CurrentFrameSet.PrimaryFrame);
                scanresult.FramePressureBack = uimfRun.GetFramePressureBack(uimfRun.CurrentFrameSet.PrimaryFrame);
            }
        }
    }
}
