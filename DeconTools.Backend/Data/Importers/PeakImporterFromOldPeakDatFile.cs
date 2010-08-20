using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Data.Importers
{
    public class PeakImporterFromOldPeakDatFile : IPeakImporter
    {
        private string m_filename;

        private int m_peakIndex;   //each imported peak is given an index


        #region Constructors
        public PeakImporterFromOldPeakDatFile(string filename)
            : this(filename, null)
        {

        }

        public PeakImporterFromOldPeakDatFile(string filename, BackgroundWorker bw)
        {
            if (!File.Exists(filename)) throw new System.IO.IOException("PeakImporter failed. File doesn't exist.");

            FileInfo fi = new FileInfo(filename);
            numRecords = (int)(fi.Length / 1000 * 26);   // a way of approximating how many peaks there are... only for use with the backgroundWorker

            this.m_filename = filename;
            this.backgroundWorker = bw;

        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public override void ImportPeaks(List<DeconTools.Backend.DTO.MSPeakResult> peakList)
        {
            m_peakIndex = 0;

             DeconToolsV2.Peaks.clsPeakProcessor oldProcessor = new DeconToolsV2.Peaks.clsPeakProcessor();
             DeconToolsV2.Results.clsTransformResults oldPeakResults = new DeconToolsV2.Results.clsTransformResults();
             DeconToolsV2.Results.clsLCMSPeak[] importedPeaks = null;

             try
             {
                 oldPeakResults.ReadResults(this.m_filename);
                 oldPeakResults.GetRawData(ref importedPeaks);

             }
             catch (Exception ex)
             {
                 throw ex;
             }

             foreach (var p in importedPeaks)
             {
                 MSPeakResult peak = convertOldDecon2LSPeakToPeakResult(p);
                 m_peakIndex++;

                 peakList.Add(peak);
             }

             oldPeakResults.__dtor();
             oldProcessor.__dtor();


        }

  
        #endregion

        #region Private Methods

        protected override void reportProgress(int progressCounter)
        {
            base.reportProgress(progressCounter);
        }

        private MSPeakResult convertOldDecon2LSPeakToPeakResult(DeconToolsV2.Results.clsLCMSPeak p)
        {
            MSPeakResult peakResult = new MSPeakResult();
            peakResult.MSPeak = new MSPeak();
            peakResult.MSPeak.XValue = p.mflt_mz;
            peakResult.MSPeak.Height = p.mflt_intensity;

            peakResult.Scan_num = p.mint_scan;
            peakResult.PeakID = m_peakIndex;

            return peakResult;

        }
        #endregion




    }
}
