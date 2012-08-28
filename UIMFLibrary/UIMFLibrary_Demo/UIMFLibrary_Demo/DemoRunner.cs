using System;
using System.IO;
using UIMFLibrary;

namespace UIMFLibrary_Demo
{
    public class DemoRunner
    {
        private string _testUIMFFile;

        #region Constructors

        public DemoRunner(string uimfFile)
        {

            _testUIMFFile = uimfFile;
        }

        #endregion

  
        #region Public Methods

        public void Execute()
        {
            //open and close UIMF file
            
            if (!File.Exists(_testUIMFFile))
            {
                reportProgress("UIMFFile not found. Input file path was: "+_testUIMFFile);
                return;
            }

            UIMFLibrary.DataReader datareader = new UIMFLibrary.DataReader();
            datareader.OpenUIMF(_testUIMFFile);





            //--------------------------------------------------------------------------Get Global parameters

            GlobalParameters gp = datareader.GetGlobalParameters();

            reportProgress();
            reportProgress();
            reportProgress("Displaying some global parameters...");
            reportProgress("NumBins= " + gp.Bins);
            reportProgress("NumFrames= " + gp.NumFrames);

            //--------------------------------------------------------------------------Get Frame parameters

            int testFrame = 500;
            FrameParameters fp = datareader.GetFrameParameters(testFrame);

            reportProgress();
            reportProgress();
            reportProgress("Displaying frame parameters for frame " + testFrame);
            reportProgress(TestUtilities.FrameParametersToString(fp));

            //--------------------------------------------------------------------------Get mass spectrum
            int frameLower = 500;
            int frameUpper = 502;
            int imsScanLower = 125;
            int imsScanUpper = 131;

            int frameType = 0;   //ms1 = 0 OR 1; ms2= 2

            double[] xvals = new double[gp.Bins];
            int[] yvals = new int[gp.Bins];
            datareader.SumScansNonCached(xvals, yvals, frameType, frameLower, frameUpper, imsScanLower, imsScanUpper);

            reportProgress();
            reportProgress();
            reportProgress("The following are a few m/z and intensitys for the summed mass spectrum from frames: " + frameLower + "-" + frameUpper + "; Scans: " + imsScanLower + "-" + imsScanUpper);

            UIMFDataUtilities.ParseOutZeroValues(ref xvals, ref yvals, 639, 640);    //note - this utility is for parsing out the zeros or filtering on m/z

            reportProgress(TestUtilities.displayRawMassSpectrum(xvals, yvals));


            //-------------------------------------------------------------------------Get LC profile
            int frameTarget = 500;
            int imsScanTarget = 126;
            double targetMZ = 639.32;
            double toleranceInPPM = 25;

            double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;
            int[] frameVals = null;
            int[] intensityVals = null;

            datareader.GetLCProfile(frameTarget - 25, frameTarget + 25, frameType, imsScanTarget - 2, imsScanTarget + 2, targetMZ, toleranceInMZ, ref frameVals, ref intensityVals);
            reportProgress();
            reportProgress();

            reportProgress("2D Extracted ion chromatogram in the LC dimension. Target m/z= " + targetMZ);
            reportProgress(TestUtilities.display2DChromatogram(frameVals,intensityVals));


            //-----------------------------------------------------------------------Get Drift time profile
            frameTarget = 500;
            imsScanTarget = 126;
            targetMZ = 639.32;
            toleranceInPPM = 25;

            int[] scanVals = null;
            datareader.GetDriftTimeProfile(frameTarget - 1, frameTarget + 1, frameType, imsScanTarget - 25, imsScanTarget + 25, targetMZ, toleranceInMZ, ref scanVals, ref intensityVals);
            
            reportProgress();
            reportProgress();

            reportProgress("2D Extracted ion chromatogram in the drift time dimension. Target m/z= " + targetMZ);
            reportProgress(TestUtilities.display2DChromatogram(scanVals, intensityVals));


            //------------------------------------------------------------------------------------Get 3D elution profile
            datareader.Get3DElutionProfile(frameTarget - 5, frameTarget + 5, 0, imsScanTarget - 5, imsScanTarget + 5, targetMZ, toleranceInMZ, ref frameVals, ref scanVals, ref intensityVals);
            reportProgress();

            reportProgress("3D Extracted ion chromatogram. Target m/z= " + targetMZ);
            reportProgress(TestUtilities.Display3DChromatogram(frameVals, scanVals, intensityVals));
         

        }

       

        #endregion

        #region Private Methods
        private void reportProgress()
        {
            Console.WriteLine();
        }

        private void reportProgress(string progressString)
        {
            Console.WriteLine(progressString);
        }

        #endregion

    }
}
