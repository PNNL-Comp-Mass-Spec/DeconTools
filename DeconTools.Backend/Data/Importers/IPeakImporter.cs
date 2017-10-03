using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using System.ComponentModel;

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

        protected virtual void reportProgress(int progressCounter)
        {
            if (numRecords == 0) return;
            if (progressCounter % 10000 == 0)
            {

                var percentProgress = (int)((double)progressCounter / (double)numRecords * 100);

                if (backgroundWorker != null)
                {
                    peakProgressInfo.ProgressInfoString = "Loading Peaks ";
                    backgroundWorker.ReportProgress(percentProgress);
                }
                else
                {
                    if (progressCounter % 50000 == 0) Console.WriteLine("Peak importer progress (%) = " + percentProgress);

                }

                return;
            }
        }


        #region Private Methods
        #endregion
    }
}
