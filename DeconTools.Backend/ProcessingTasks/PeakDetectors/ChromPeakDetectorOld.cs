using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class ChromPeakDetectorOld:PeakDetector
    {
#if !Disable_DeconToolsV2
        DeconToolsV2.Peaks.clsPeakProcessorParameters _oldPeakParameters;

        DeconToolsV2.Peaks.clsPeakProcessor _oldPeakProcessor;

        DeconToolsV2.Peaks.clsPeak[] _oldDeconEnginePeaklist;
#endif

        private DeconToolsPeakDetectorV2 _peakDetectorV2;


        #region Constructors
        public ChromPeakDetectorOld():base() { }

        public ChromPeakDetectorOld(double peakBackgroundRatio, double sigNoise)
        {
            this.PeakBackgroundRatio = peakBackgroundRatio;
            this.SigNoise = sigNoise;

            _peakDetectorV2 = new DeconToolsPeakDetectorV2();
            _peakDetectorV2.PeakToBackgroundRatio = peakBackgroundRatio;
            _peakDetectorV2.SignalToNoiseThreshold = sigNoise;
            _peakDetectorV2.PeakFitType = Globals.PeakFitType.QUADRATIC;
            _peakDetectorV2.IsDataThresholded = false;

        }

#if !Disable_DeconToolsV2
        public ChromPeakDetectorOld(DeconToolsV2.Peaks.clsPeakProcessorParameters parameters)
        {
            this.PeakBackgroundRatio = parameters.PeakBackgroundRatio;
            SigNoise = parameters.SignalToNoiseThreshold;
            _oldPeakParameters = parameters;

        }
#endif

        #endregion

        #region Properties

        public double PeakBackgroundRatio { get; set; }

        public double SigNoise { get; set; }

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

            if (UseNewPeakDetector)
            {
                _peakDetectorV2.FindPeaks(xydata, xMin, xMax);

            }
            else
            {
#if Disable_DeconToolsV2
                throw new NotImplementedException("Cannot use the old Peak Detector since support for C++ based DeconToolsV2 is disabled");
#else
                if (_oldPeakParameters==null)
                {
                    _oldPeakParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters(this.SigNoise, this.PeakBackgroundRatio, false, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);     

                }

                if (_oldPeakProcessor==null)
                {
                    _oldPeakProcessor = new DeconToolsV2.Peaks.clsPeakProcessor(); 
                }


                _oldPeakParameters.PeakBackgroundRatio = this.PeakBackgroundRatio;
                _oldPeakParameters.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
                _oldPeakParameters.SignalToNoiseThreshold = this.SigNoise;
                _oldPeakParameters.ThresholdedData = false;

                _oldPeakProcessor.SetOptions(this._oldPeakParameters);


                //Find peaks using DeconEngine
                float[] xvals = new float[1];
                float[] yvals = new float[1];

                xydata.GetXYValuesAsSingles(ref xvals, ref yvals);
                double largestXValue = xydata.Xvalues[xydata.Xvalues.Length - 1];

                _oldDeconEnginePeaklist = new DeconToolsV2.Peaks.clsPeak[0];
                try
                {
                    _oldPeakProcessor.DiscoverPeaks(ref xvals, ref yvals, ref _oldDeconEnginePeaklist, (float)(0), (float)(largestXValue));
                }
                catch (Exception ex)
                {
                    Logger.Instance.AddEntry("ChromPeakDetector failed.", Logger.Instance.OutputFilename);
                    throw ex;
                }

                foreach (DeconToolsV2.Peaks.clsPeak peak in _oldDeconEnginePeaklist)
                {
                    ChromPeak chromPeak = new ChromPeak();
                    chromPeak.XValue = peak.mdbl_mz;          // here,  mz is actually the scan / or NET 
                    chromPeak.Height = (float)peak.mdbl_intensity;
                    chromPeak.SignalToNoise = (float)peak.mdbl_SN;
                    chromPeak.Width = (float)peak.mdbl_FWHM;

                    peakList.Add(chromPeak);

                }
#endif

            }


            //resultList.Run.PeakList = new List<IPeak>();

            


            return peakList;

            
        }


        public override void Execute(ResultCollection resultList)
        {
            base.Execute(resultList);

            foreach (ChromPeak peak in resultList.Run.PeakList)
            {
                peak.NETValue = resultList.Run.NetAlignmentInfo.GetNETValueForScan((int)peak.XValue);
            }
        }

        protected override double GetBackgroundIntensity(double[] yvalues, double[] xvalues = null)
        {
            return 0;
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
