using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;


namespace DeconTools.Backend.ProcessingTasks
{
    public class DeconToolsPeakDetector : IPeakDetector
    {
        DeconToolsV2.Peaks.clsPeakProcessor peakProcessor;

        double m_backgroundIntensity;
        DeconToolsV2.Peaks.clsPeak[] m_deconEnginePeaklist = new DeconToolsV2.Peaks.clsPeak[0];



        #region Properties
        private double peakBackgroundRatio;

        public double PeakBackgroundRatio
        {
            get { return peakBackgroundRatio; }
            set { peakBackgroundRatio = value; }
        }


        public double BackGroundIntensity
        {
            get { return m_backgroundIntensity; }
            set { m_backgroundIntensity = value; }
        }



        private double sigNoiseThreshold;

        public double SigNoiseThreshold
        {
            get { return sigNoiseThreshold; }
            set { sigNoiseThreshold = value; }
        }

        private DeconTools.Backend.Globals.PeakFitType peakFitType;
        public DeconTools.Backend.Globals.PeakFitType PeakFitType
        {
            get { return peakFitType; }
            set { peakFitType = value; }
        }



        private bool isDataThresholded;

        public bool IsDataThresholded
        {
            get { return isDataThresholded; }
            set { isDataThresholded = value; }
        }

        private bool storePeakData;
        public bool StorePeakData
        {
            get { return storePeakData; }
            set { storePeakData = value; }
        }

        DeconToolsV2.Peaks.clsPeakProcessorParameters deconEngineParameters;

        public DeconToolsV2.Peaks.clsPeakProcessorParameters DeconEngineParameters
        {
            get { return deconEngineParameters; }
            internal set { deconEngineParameters = value; }
        }


        #endregion

        #region Constructors
        public DeconToolsPeakDetector()
        {
            this.DeconEngineParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            this.setDefaults();
        }

        private void setDefaults()
        {
            this.peakFitType = Globals.PeakFitType.QUADRATIC;
            this.peakBackgroundRatio = 1.3;
            this.sigNoiseThreshold = 2;
            this.isDataThresholded = false;
            this.StorePeakData = false;

        }

        public DeconToolsPeakDetector(DeconToolsV2.Peaks.clsPeakProcessorParameters parameters)
            : this()
        {
            convertDeconEngineParameters(parameters);
        }


        public DeconToolsPeakDetector(double peakToBackgroundRatio, double signalToNoiseRatio, Globals.PeakFitType peakFitType, bool isDataThresholded)
            : this()
        {
            this.peakBackgroundRatio = peakToBackgroundRatio;
            this.sigNoiseThreshold = signalToNoiseRatio;
            this.peakFitType = peakFitType;
            this.IsDataThresholded = IsDataThresholded;


        }


        #endregion

        #region Private Methods
        private void updateDeconEngineParameters()
        {
            this.DeconEngineParameters.PeakBackgroundRatio = this.peakBackgroundRatio;
            this.DeconEngineParameters.SignalToNoiseThreshold = this.sigNoiseThreshold;
            this.DeconEngineParameters.PeakFitType = getDeconPeakFitType(this.peakFitType);
            this.DeconEngineParameters.ThresholdedData = this.isDataThresholded;
            this.DeconEngineParameters.WritePeaksToTextFile = this.StorePeakData;

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
            this.peakBackgroundRatio = parameters.PeakBackgroundRatio;
            this.peakFitType = getPeakFitType(parameters.PeakFitType);
            this.sigNoiseThreshold = parameters.SignalToNoiseThreshold;
            this.StorePeakData = parameters.WritePeaksToTextFile;

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

        private List<DeconTools.Backend.Core.IPeak> ConvertDeconEnginePeakList(DeconToolsV2.Peaks.clsPeak[] peaklist)
        {
            List<IPeak> returnedList = new List<IPeak>();

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
        public override List<IPeak> FindPeaks(XYData xydata, double minX, double maxX)
        {
            if (xydata == null || xydata.Xvalues == null || xydata.Xvalues.Length == 0) return null;


            List<IPeak> peakList = new List<IPeak>();

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

            try
            {
                peakProcessor.DiscoverPeaks(ref xvals, ref yvals, ref m_deconEnginePeaklist, (float)(minX), (float)(maxX));

            }
            catch (Exception ex)
            {
                throw ex; 
            }

            this.m_backgroundIntensity = peakProcessor.GetBackgroundIntensity(ref yvals);

            peakList = ConvertDeconEnginePeakList(m_deconEnginePeaklist);   
            return peakList;
        }


        #endregion




        public override void addPeakRelatedData(Run run)
        {
            run.CurrentScanSet.BackgroundIntensity = m_backgroundIntensity;
            run.CurrentScanSet.NumPeaks = run.PeakList.Count;    //used in ScanResult
            run.CurrentScanSet.BasePeak = getBasePeak(run.PeakList);     //Used in ScanResult

            run.DeconToolsPeakList = m_deconEnginePeaklist;    //this must be stored since the THRASH algorithms works on DeconEngine peaks. 


            //if (run.CurrentScanSet.PrimaryScanNumber == 142)
            //{
            //    foreach (var peak in run.PeakList)
            //    {
            //        Console.WriteLine(peak.XValue + "\t" + peak.Height);

            //    }
            //}

            if (this.StorePeakData)    //store all peak data;   (Exporters are triggered to access this and export info and clear the MSPeakResults)
            {
                run.ResultCollection.FillMSPeakResults();    //data from the MSPeakList is transferred to 'MSPeakResults'
            }


        }


        public override void Cleanup()
        {
            base.Cleanup();
            m_deconEnginePeaklist = null;
            
        }



        public override void applyRunRelatedSettings(Run run)
        {
            this.isDataThresholded = run.IsDataThresholded;
        }
    }
}
