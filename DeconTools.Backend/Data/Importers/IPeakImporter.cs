using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Data
{
    public abstract class IPeakImporter
    {
        protected int numRecords;
        protected BackgroundWorker backgroundWorker;
        protected PeakProgressInfo peakProgressInfo;

        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public abstract void ImportPeaks(List<MSPeakResult> peakList);

        #endregion

        protected virtual void reportProgress(int peaksLoaded, ref DateTime lastReportProgress, ref DateTime lastReportProgressConsole)
        {
            if (numRecords == 0) return;
            if (peaksLoaded % 1000 != 0) return;

            var percentProgress = (int)(peaksLoaded / (double)numRecords * 100);

            if (backgroundWorker != null && DateTime.UtcNow.Subtract(lastReportProgress).TotalSeconds >= 0.5)
            {
                lastReportProgress = DateTime.UtcNow;
                peakProgressInfo.ProgressInfoString = "Loading Peaks ";
                backgroundWorker.ReportProgress(percentProgress);
                return;
            }

            if (DateTime.UtcNow.Subtract(lastReportProgressConsole).TotalSeconds >= 1)
            {
                lastReportProgressConsole = DateTime.UtcNow;
                Console.WriteLine("Peak importer progress (%) = " + percentProgress);
            }
        }


        #region Private Methods
        #endregion
    }
}
