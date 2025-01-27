﻿using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public abstract class PeakDetector : Task
    {
        private readonly PeakUtilities _peakUtilities = new PeakUtilities();

        public abstract List<Peak> FindPeaks(XYData xyData, double minX = 0, double maxX = 0);

        public double MinX { get; set; }

        public double MaxX { get; set; }

        public bool PeaksAreStored { get; set; }

        public double BackgroundIntensity { get; set; }

        public bool UseNewPeakDetector { get; set; }

        public override void Execute(ResultCollection resultList)
        {
            if (resultList.Run.XYData?.Xvalues == null ||
                resultList.Run.XYData.Yvalues == null ||
                resultList.Run.XYData.Xvalues.Length == 0 ||
                resultList.Run.XYData.Yvalues.Length == 0)
            {
                resultList.Run.PeakList = new List<Peak>();
                return;
            }

            var isSuccess = false;

            var counter = 1;

            //[gord] I'm adding a loop here, because we are experiencing an infrequent and seemingly random failure with the peak detector on data from UIMF files
            //looping it may force it to process the current ims scan.
            while (!isSuccess && counter <= 4)
            {
                try
                {
                    resultList.Run.PeakList = FindPeaks(resultList.Run.XYData, MinX, MaxX);
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    var frameAndScanInfo = resultList.Run.GetCurrentScanOrFrameInfo();
                    var sb = new StringBuilder();
                    sb.Append("Attempt ");
                    sb.Append(counter);
                    sb.Append("; PeakDetector is throwing an error; ");
                    sb.Append(frameAndScanInfo);
                    sb.Append("; RawXYData counts: xVals = ");
                    sb.Append(resultList.Run.XYData.Xvalues.Length);
                    sb.Append("; yVals = ");
                    sb.Append(resultList.Run.XYData.Yvalues.Length);
                    sb.Append("; additional details: ");
                    sb.Append(ex.Message);
                    sb.Append("; ");
                    sb.Append(PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));

                    if (Logger.Instance.OutputFilename == null)
                    {
                        Console.WriteLine(sb.ToString());
                    }
                    else
                    {
                        Logger.Instance.AddEntry(sb.ToString(), true);
                    }
                }

                ExecutePostProcessingHook(resultList.Run);
                counter++;
            }
        }

        protected virtual void ExecutePostProcessingHook(Run run)
        {
            run.CurrentBackgroundIntensity = BackgroundIntensity;
        }

        protected void GatherPeakStatistics(Run run)
        {
            ScanSet currentScanSet;

            if (run is UIMFRun uimfRun)
            {
                currentScanSet = uimfRun.CurrentIMSScanSet;
            }
            else
            {
                currentScanSet = run.CurrentScanSet;
            }

            Check.Require(currentScanSet != null, "the CurrentScanSet for the Run is null. This needs to be set.");

            if (currentScanSet == null)
            {
                return;
            }

            currentScanSet.BackgroundIntensity = BackgroundIntensity;
            currentScanSet.NumPeaks = run.PeakList.Count; //used in ScanResult
            currentScanSet.BasePeak = _peakUtilities.GetBasePeak(run.PeakList); //Used in ScanResult
        }

        protected abstract double GetBackgroundIntensity(double[] yValues, double[] xValues = null);
    }
}
