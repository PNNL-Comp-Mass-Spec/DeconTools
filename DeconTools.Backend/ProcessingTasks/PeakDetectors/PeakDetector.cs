using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public abstract class PeakDetector:Task
    {

     
        public abstract List<Peak> FindPeaks(XYData xydata, double minX, double maxX);

        
        public double MinX { get; set; }

        public double MaxX { get; set; }

        public bool PeaksAreStored { get; set; }

        public double BackgroundIntensity { get; set; }


        public override void Execute(ResultCollection resultList)
        {
            if (resultList.Run.XYData == null ||
                resultList.Run.XYData.Xvalues == null ||
                resultList.Run.XYData.Yvalues == null ||
                resultList.Run.XYData.Xvalues.Length == 0 ||
                resultList.Run.XYData.Yvalues.Length == 0)
            {
                //resultList.AddLog("Peak Detector aborted; XY data is empty; Scan = " + resultList.Run.CurrentScanSet.PrimaryScanNumber.ToString());


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
                    sb.Append(ex.StackTrace);

                    if (Logger.Instance.OutputFilename==null)
                    {
                        Console.WriteLine(sb.ToString());
                    }
                    else
                    {
                        Logger.Instance.AddEntry(sb.ToString(), Logger.Instance.OutputFilename);
                    }
                    

                }

                counter++;

            }
            
        }
    }
}
