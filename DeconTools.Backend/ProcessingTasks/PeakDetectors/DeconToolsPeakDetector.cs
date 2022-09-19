using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class DeconToolsPeakDetector : PeakDetector
    {
        PeakProcessor peakProcessor;

        #region Properties

        public List<ThrashV1Peak> DeconEnginePeakList { get; set; }

        public bool IsDataThresholded { get; set; }

        public Globals.PeakFitType PeakFitType { get; set; }

        public double SignalToNoiseThreshold { get; set; }

        public double PeakToBackgroundRatio { get; set; }

        #endregion

        #region Constructors

        public DeconToolsPeakDetector()
        {
            SignalToNoiseThreshold = 3;
            PeakToBackgroundRatio = 5;
            PeakFitType = Globals.PeakFitType.QUADRATIC;
        }

        public DeconToolsPeakDetector(PeakDetectorParameters parameters)
        {
            IsDataThresholded = parameters.IsDataThresholded;
            PeakFitType = parameters.PeakFitType;
            SignalToNoiseThreshold = parameters.SignalToNoiseThreshold;
            PeakToBackgroundRatio = parameters.PeakToBackgroundRatio;
            PeaksAreStored = parameters.PeaksAreStored;
            MinX = parameters.MinX;
            MaxX = parameters.MaxX;
        }

        public DeconToolsPeakDetector(double peakToBackgroundRatio, double signalToNoiseRatio, Globals.PeakFitType peakFitType, bool isDataThresholded, double minX = 0, double maxX = double.MaxValue)
        {
            PeakToBackgroundRatio = peakToBackgroundRatio;
            SignalToNoiseThreshold = signalToNoiseRatio;
            PeakFitType = peakFitType;
            IsDataThresholded = isDataThresholded;
            MaxX = maxX;
            MinX = minX;
        }

        #endregion

        #region Private Methods

        private void UpdateDeconEngineParameters()
        {
            peakProcessor.SetOptions(SignalToNoiseThreshold, 0, IsDataThresholded, GetDeconPeakFitType(PeakFitType));
        }

        private PeakFitType GetDeconPeakFitType(Globals.PeakFitType peakFitType)
        {
            switch (peakFitType)
            {
                case Globals.PeakFitType.Undefined:
                    throw new Exception("Failed to convert PeakType to DeconEnginePeakType");
                case Globals.PeakFitType.APEX:
                    return Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing.PeakFitType.Apex;
                case Globals.PeakFitType.LORENTZIAN:
                    return Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing.PeakFitType.Lorentzian;
                case Globals.PeakFitType.QUADRATIC:
                    return Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing.PeakFitType.Quadratic;
                default:
                    return Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing.PeakFitType.Apex;
            }
        }

        private List<Peak> ConvertDeconEnginePeakList(List<ThrashV1Peak> peakList)
        {
            var returnedList = new List<Peak>();

            foreach (var p in peakList)
            {
                var peak = new MSPeak(p.Mz, (int)p.Intensity, (float)p.FWHM, p.SignalToNoise)
                {
                    DataIndex = p.DataIndex         // this points to the index value of the raw xy values - I think
                };

                returnedList.Add(peak);
            }
            return returnedList;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Finds peaks in XY Data within the specified range of X values. Optionally, to use all XY data points, enter 0 for both min and max values
        /// </summary>
        /// <param name="xyData"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <returns></returns>
        public override List<Peak> FindPeaks(XYData xyData, double minX = 0, double maxX = 0)
        {
            if (xyData?.Xvalues == null || xyData.Xvalues.Length == 0)
            {
                return null;
            }

            var xVals = xyData.Xvalues.ToList();
            var yVals = xyData.Yvalues.ToList();

            //initialize DeconEngine's peakFinding class
            peakProcessor = new PeakProcessor();

            //initialize options

            //this.isDataThresholded = resultList.Run.IsDataThresholded;   [commented out:  2010_04_05 by gord]
            UpdateDeconEngineParameters();

            BackgroundIntensity = peakProcessor.GetBackgroundIntensity(yVals);
            peakProcessor.SetOptions(SignalToNoiseThreshold, BackgroundIntensity * PeakToBackgroundRatio, IsDataThresholded, GetDeconPeakFitType(PeakFitType));

            //Find peaks using DeconEngine
            if (Math.Abs(minX) < float.Epsilon && Math.Abs(maxX) < float.Epsilon)
            {
                minX = xyData.Xvalues[0];
                maxX = xyData.Xvalues.Last();
            }

            try
            {
                peakProcessor.DiscoverPeaks(xVals, yVals, minX, maxX);
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("DeconToolsPeakDetector.FindPeaks exception: " + ex.Message, true);
                throw;
            }

            DeconEnginePeakList = peakProcessor.PeakData.PeakTops;

            return ConvertDeconEnginePeakList(DeconEnginePeakList);
        }

        #endregion

        public override void Execute(ResultCollection resultList)
        {
            base.Execute(resultList);

            resultList.Run.DeconToolsPeakList = DeconEnginePeakList.ToArray();
        }

        protected override void ExecutePostProcessingHook(Run run)
        {
            base.ExecutePostProcessingHook(run);

            if (PeaksAreStored)
            {
                run.ResultCollection.FillMSPeakResults();
            }
        }

        protected override double GetBackgroundIntensity(double[] yValues, double[] xValues = null)
        {
            return 0;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            DeconEnginePeakList = null;
        }
    }
}
