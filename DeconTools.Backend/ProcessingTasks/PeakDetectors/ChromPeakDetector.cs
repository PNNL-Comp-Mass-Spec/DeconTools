using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class ChromPeakDetector:PeakDetector
    {
        DeconToolsV2.Peaks.clsPeakProcessorParameters oldPeakParameters;

        DeconToolsV2.Peaks.clsPeakProcessor peakProcessor;

        DeconToolsV2.Peaks.clsPeak[] deconEnginePeaklist;

        #region Constructors
        public ChromPeakDetector():base() { }

        public ChromPeakDetector(double peakBackgroundRatio, double sigNoise)
        {
            this.PeakBackgroundRatio = peakBackgroundRatio;
            this.SigNoise = sigNoise;

            oldPeakParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters(this.sigNoise, this.peakBackgroundRatio, false, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            peakProcessor = new DeconToolsV2.Peaks.clsPeakProcessor();
        }


        public ChromPeakDetector(DeconToolsV2.Peaks.clsPeakProcessorParameters parameters)
        {
            this.PeakBackgroundRatio = parameters.PeakBackgroundRatio;
            SigNoise = parameters.SignalToNoiseThreshold;
            oldPeakParameters = parameters;

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

        //TODO: remove code duplication (see DeconToolsPeakDetector)
        public override List<Peak> FindPeaks(XYData xydata, double xMin, double xMax)
        {
            List<Peak> peakList = new List<Peak>();

            if (xydata == null)
            {
                return peakList;
            }

            //resultList.Run.PeakList = new List<IPeak>();

            oldPeakParameters.PeakBackgroundRatio = this.peakBackgroundRatio;
            oldPeakParameters.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            oldPeakParameters.SignalToNoiseThreshold = this.sigNoise;
            oldPeakParameters.ThresholdedData = false;
            
            peakProcessor.SetOptions(this.oldPeakParameters);

            //Find peaks using DeconEngine
            float[] xvals = new float[1];
            float[] yvals = new float[1];

            xydata.GetXYValuesAsSingles(ref xvals, ref yvals);
            double largestXValue = xydata.Xvalues[xydata.Xvalues.Length - 1];

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
                ChromPeak chromPeak = new ChromPeak();      
                chromPeak.XValue = peak.mdbl_mz;          // here,  mz is actually the scan / or NET 
                chromPeak.Height =(float)peak.mdbl_intensity;     
                chromPeak.SignalToNoise = (float)peak.mdbl_SN;     
                chromPeak.Width = (float)peak.mdbl_FWHM;

                peakList.Add(chromPeak);
                
            }

            return peakList;

            
        }


        public override void Execute(ResultCollection resultList)
        {
            base.Execute(resultList);

            foreach (ChromPeak peak in resultList.Run.PeakList)
            {
                peak.NETValue = resultList.Run.GetNETValueForScan((int)peak.XValue);
            }
        }


        //public override void AddPeakRelatedData(Run run)
        //{

        //    if (run.PeakList == null || run.PeakList.Count == 0)
        //    {
        //        bool workflowIsTargeted = (run.CurrentMassTag != null);
        //        if (workflowIsTargeted)
        //        {
        //            TargetedResultBase result = run.ResultCollection.CurrentTargetedResult;
        //            result.FailedResult = true;
        //            result.FailureType = Globals.TargetedResultFailureType.CHROM_PEAKS_NOT_DETECTED;
        //        }
        //    }
            

        //    foreach (ChromPeak peak in run.PeakList)
        //    {
        //        peak.NETValue = run.GetNETValueForScan((int)peak.XValue);
        //    }



        //}
        #endregion

      
    }
}
