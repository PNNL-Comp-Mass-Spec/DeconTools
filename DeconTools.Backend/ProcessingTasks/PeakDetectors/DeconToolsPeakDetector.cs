using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconToolsV2.Peaks;


namespace DeconTools.Backend.ProcessingTasks
{
    public class DeconToolsPeakDetector : PeakDetector
    {
        DeconToolsV2.Peaks.clsPeakProcessor peakProcessor;

        

        #region Properties


        DeconToolsV2.Peaks.clsPeakProcessorParameters deconEngineParameters;

        public DeconToolsV2.Peaks.clsPeakProcessorParameters DeconEngineParameters
        {
            get { return deconEngineParameters; }
            internal set { deconEngineParameters = value; }
        }

        public clsPeak[] DeconEnginePeakList { get; set; }


        #endregion

        #region Constructors
        public DeconToolsPeakDetector()
            : base()
        {
            this.DeconEngineParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();

        }

        public DeconToolsPeakDetector(DeconToolsV2.Peaks.clsPeakProcessorParameters parameters)
            : this()
        {
            convertDeconEngineParameters(parameters);
        }


        public DeconToolsPeakDetector(PeakDetectorParameters parameters)
        {
            IsDataThresholded = parameters.IsDataThresholded;
            PeakFitType = parameters.PeakFitType;
            SignalToNoiseThreshold = parameters.SignalToNoiseThreshold;
            PeakToBackgroundRatio = parameters.PeakToBackgroundRatio;
            PeaksAreStored= parameters.PeaksAreStored;
            MinX = parameters.MinX;
            MaxX = parameters.MaxX;
        }

        


        public DeconToolsPeakDetector(double peakToBackgroundRatio, double signalToNoiseRatio, Globals.PeakFitType peakFitType, bool isDataThresholded, double minX = 0, double maxX = double.MaxValue)
            : this()
        {
            PeakToBackgroundRatio = peakToBackgroundRatio;
            SignalToNoiseThreshold = signalToNoiseRatio;
            PeakFitType = peakFitType;
            IsDataThresholded = isDataThresholded;
            MaxX = maxX;
            MinX = minX;

        }

        public bool IsDataThresholded { get; set; }

        public Globals.PeakFitType PeakFitType { get; set; }

        public double SignalToNoiseThreshold { get; set; }

        public double PeakToBackgroundRatio { get; set; }

        
        #endregion

        #region Private Methods
        private void updateDeconEngineParameters()
        {
            this.DeconEngineParameters.PeakBackgroundRatio = PeakToBackgroundRatio;
            this.DeconEngineParameters.SignalToNoiseThreshold = SignalToNoiseThreshold;
            this.DeconEngineParameters.PeakFitType = getDeconPeakFitType(PeakFitType);
            this.DeconEngineParameters.ThresholdedData = IsDataThresholded;
            this.DeconEngineParameters.WritePeaksToTextFile = PeaksAreStored;

        }

        private DeconToolsV2.Peaks.PEAK_FIT_TYPE getDeconPeakFitType(Globals.PeakFitType peakFitType)
        {
            switch (peakFitType)
            {
                case Globals.PeakFitType.Undefined:
                    throw new Exception("Failed to convert PeakType to DeconEnginePeakType");
                case Globals.PeakFitType.APEX:
                    return DeconToolsV2.Peaks.PEAK_FIT_TYPE.APEX;
                case Globals.PeakFitType.LORENTZIAN:
                    return DeconToolsV2.Peaks.PEAK_FIT_TYPE.LORENTZIAN;
                case Globals.PeakFitType.QUADRATIC:
                    return DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
                default:
                    return DeconToolsV2.Peaks.PEAK_FIT_TYPE.APEX;
            }
        }

        private void convertDeconEngineParameters(DeconToolsV2.Peaks.clsPeakProcessorParameters parameters)
        {

            //this.isDataThresholded = parameters.ThresholdedData;     //IsDataThresholded will now be extracted from the Run class

            PeakToBackgroundRatio = parameters.PeakBackgroundRatio;
            SignalToNoiseThreshold = parameters.SignalToNoiseThreshold; 
            PeakFitType = getPeakFitType(parameters.PeakFitType);
            PeaksAreStored = parameters.WritePeaksToTextFile;
            

        }

        private Globals.PeakFitType getPeakFitType(DeconToolsV2.Peaks.PEAK_FIT_TYPE pEAK_FIT_TYPE)
        {
            switch (pEAK_FIT_TYPE)
            {
                case DeconToolsV2.Peaks.PEAK_FIT_TYPE.APEX:
                    return Globals.PeakFitType.APEX;
                case DeconToolsV2.Peaks.PEAK_FIT_TYPE.LORENTZIAN:
                    return Globals.PeakFitType.LORENTZIAN;
                case DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC:
                    return Globals.PeakFitType.QUADRATIC;
                default:
                    return Globals.PeakFitType.Undefined;
            }
        }

        private List<DeconTools.Backend.Core.Peak> ConvertDeconEnginePeakList(DeconToolsV2.Peaks.clsPeak[] peaklist)
        {
            List<Peak> returnedList = new List<Peak>();

            for (int i = 0; i < peaklist.Length; i++)
            {
                MSPeak peak = new MSPeak();
                peak.XValue = peaklist[i].mdbl_mz;
                peak.Height = (int)peaklist[i].mdbl_intensity;
                peak.SN = (float)peaklist[i].mdbl_SN;
                peak.Width = (float)peaklist[i].mdbl_FWHM;

                peak.DataIndex = peaklist[i].mint_data_index;      // this points to the index value of the raw xy values - I think


                returnedList.Add(peak);

            }
            return returnedList;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Finds peaks in XY Data within the specified range of X values. Optionally, to use all XY data points, enter 0 for both min and max values
        /// </summary>
        /// <param name="xydata"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <returns></returns>
        public override List<Peak> FindPeaks(XYData xydata, double minX, double maxX)
        {
            if (xydata == null || xydata.Xvalues == null || xydata.Xvalues.Length == 0) return null;


            List<Peak> peakList = new List<Peak>();

            //initialize DeconEngine's peakFinding class
            peakProcessor = new DeconToolsV2.Peaks.clsPeakProcessor();

            //initialize options

            //this.isDataThresholded = resultList.Run.IsDataThresholded;   [commented out:  2010_04_05 by gord]
            updateDeconEngineParameters();
            peakProcessor.SetOptions(this.deconEngineParameters);

            //Find peaks using DeconEngine
            float[] xvals = new float[1];
            float[] yvals = new float[1];

            xydata.GetXYValuesAsSingles(ref xvals, ref yvals);

            if (minX == 0 && maxX == 0)
            {
                minX = xydata.Xvalues.First();
                maxX = xydata.Xvalues.Last();
            }

            clsPeak[] deconEnginePeakList = null;
            try
            {
                peakProcessor.DiscoverPeaks(ref xvals, ref yvals, ref deconEnginePeakList, (float)(minX), (float)(maxX));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            BackgroundIntensity = peakProcessor.GetBackgroundIntensity(ref yvals);
            DeconEnginePeakList = deconEnginePeakList;

            peakList = ConvertDeconEnginePeakList(DeconEnginePeakList);
            return peakList;
        }


        #endregion


        public override void Execute(ResultCollection resultList)
        {
            base.Execute(resultList);

            resultList.Run.DeconToolsPeakList = DeconEnginePeakList;

            resultList.Run.CurrentBackgroundIntensity = BackgroundIntensity;
        }

        
        public override void Cleanup()
        {
            base.Cleanup();
            DeconEnginePeakList = null;

        }

    }
}
