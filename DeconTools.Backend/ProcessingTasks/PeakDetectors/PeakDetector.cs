using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public abstract class PeakDetector:Task
    {
        private PeakUtilities _peakUtilities = new PeakUtilities();
     
        public abstract List<Peak> FindPeaks(XYData xydata, double minX=0, double maxX=0);

        
        public double MinX { get; set; }

        public double MaxX { get; set; }

        public bool PeaksAreStored { get; set; }

        public double BackgroundIntensity { get; set; }

        public bool UseNewPeakDetector { get; set; }

        public override void Execute(ResultCollection resultList)
        {
            if (resultList.Run.XYData == null ||
                resultList.Run.XYData.Xvalues == null ||
                resultList.Run.XYData.Yvalues == null ||
                resultList.Run.XYData.Xvalues.Length == 0 ||
                resultList.Run.XYData.Yvalues.Length == 0)
            {
                resultList.Run.PeakList = new List<Peak>();
                return;
            }
           
            bool isSuccess = false;

            int counter = 1;


            //[gord] I'm adding a loop here, because we are experiencing an infrequent and seemingly random failure with the peak detector on data from UIMF files
            //looping it may force it to process the current ims scan. 
            while (!isSuccess && counter<=4)
            {
                try
                {
                    resultList.Run.PeakList = FindPeaks(resultList.Run.XYData, MinX, MaxX);
                    isSuccess = true;


                    
                }
                catch (Exception ex)
                {
                    string frameAndScanInfo = resultList.Run.GetCurrentScanOrFrameInfo();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Attempt ");
                    sb.Append(counter);
                    sb.Append("; PeakDetector is throwing an error; ");
                    sb.Append(frameAndScanInfo);
                    sb.Append("; RawXYData counts: xvals = ");
                    sb.Append(resultList.Run.XYData.Xvalues.Length);
                    sb.Append("; yvals = ");
                    sb.Append(resultList.Run.XYData.Yvalues.Length);
                    sb.Append("; additional details: ");
                    sb.Append(ex.Message);
                    sb.Append("; ");
                    sb.Append(PRISM.clsStackTraceFormatter.GetExceptionStackTraceMultiLine(ex));

                    if (Logger.Instance.OutputFilename==null)
                    {
                        Console.WriteLine(sb.ToString());
                    }
                    else
                    {
                        Logger.Instance.AddEntry(sb.ToString(), Logger.Instance.OutputFilename);
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

            ScanSet currentScanset;

            if (run is UIMFRun)
            {
                currentScanset = ((UIMFRun)run).CurrentIMSScanSet;
            }
            else
            {
                currentScanset = run.CurrentScanSet;
            }

            Check.Require(currentScanset != null, "the CurrentScanSet for the Run is null. This needs to be set.");

            currentScanset.BackgroundIntensity = BackgroundIntensity;
            currentScanset.NumPeaks = run.PeakList.Count;    //used in ScanResult
            currentScanset.BasePeak = _peakUtilities.GetBasePeak(run.PeakList);     //Used in ScanResult

        }


        
        protected abstract double GetBackgroundIntensity(double[] yvalues, double[] xvalues = null);
    }
}
