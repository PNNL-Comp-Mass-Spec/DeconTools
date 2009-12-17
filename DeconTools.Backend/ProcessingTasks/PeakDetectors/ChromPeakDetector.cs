using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class ChromPeakDetector:IPeakDetector
    {
        DeconToolsV2.Peaks.clsPeakProcessorParameters oldPeakParameters;

        DeconToolsV2.Peaks.clsPeakProcessor peakProcessor;

        DeconToolsV2.Peaks.clsPeak[] deconEnginePeaklist;

        #region Constructors
        public ChromPeakDetector():this(2,2)
        {

        }

        public ChromPeakDetector(double peakBackgroundRatio, double sigNoise)
        {
            this.PeakBackgroundRatio = peakBackgroundRatio;
            this.SigNoise = sigNoise;

            oldPeakParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters(this.sigNoise, this.peakBackgroundRatio, false, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            peakProcessor = new DeconToolsV2.Peaks.clsPeakProcessor();
        }

        #endregion

        #region Properties
        double peakBackgroundRatio;
        public double PeakBackgroundRatio
        {
            get { return peakBackgroundRatio; }
            set { peakBackgroundRatio = value; }
        }
        double sigNoise;
        public double SigNoise
        {
            get { return sigNoise; }
            set { sigNoise = value; }
        }
        #endregion

        #region Public Methods
        public override void FindPeaks(DeconTools.Backend.Core.ResultCollection resultList)
        {
            resultList.Run.MSPeakList = new List<MSPeak>();

            oldPeakParameters.PeakBackgroundRatio = this.peakBackgroundRatio;
            oldPeakParameters.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            oldPeakParameters.SignalToNoiseThreshold = this.sigNoise;
            oldPeakParameters.ThresholdedData = false;
            
            peakProcessor.SetOptions(this.oldPeakParameters);

            //Find peaks using DeconEngine
            float[] xvals = new float[1];
            float[] yvals = new float[1];

            resultList.Run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);
            double largestXValue = resultList.Run.XYData.Xvalues[resultList.Run.XYData.Xvalues.Length - 1];

            deconEnginePeaklist = new DeconToolsV2.Peaks.clsPeak[0];
            try
            {
                peakProcessor.DiscoverPeaks(ref xvals, ref yvals, ref deconEnginePeaklist, (float)(0), (float)(largestXValue));
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("ChromPeakDetector failed." , Logger.Instance.OutputFilename);
                throw ex;
            }

            foreach (DeconToolsV2.Peaks.clsPeak peak in deconEnginePeaklist)
            {
                MSPeak chromPeak = new MSPeak();      //TODO:   refactor this.  Need create abstract Peak and inherit from this.
                chromPeak.MZ = peak.mdbl_mz;          // here,  mz is actually the scan / or NET 
                chromPeak.Intensity =(float)peak.mdbl_intensity;     
                chromPeak.SN = (float)peak.mdbl_SN;     
                chromPeak.FWHM = (float)peak.mdbl_FWHM;    

                resultList.Run.MSPeakList.Add(chromPeak);
                
            }

            
        }

      

        protected override void addDataToScanResult(DeconTools.Backend.Core.ResultCollection resultList, DeconTools.Backend.Core.ScanResult scanresult)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        #endregion
     
    }
}
