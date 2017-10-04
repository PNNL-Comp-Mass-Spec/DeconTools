#if !Disable_DeconToolsV2
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.DTO;

namespace DeconTools.UnitTesting2.DeconEngineClasses
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
            if (!File.Exists(filename)) throw new System.IO.IOException("PeakImporter failed. File doesn't exist: " + Backend.Utilities.DiagnosticUtilities.GetFullPathSafe(filename));

            var fi = new FileInfo(filename);
            this.numRecords = (int)(fi.Length / 1000 * 26);   // a way of approximating how many peaks there are... only for use with the backgroundWorker

            this.m_filename = filename;
            this.backgroundWorker = bw;
        }

        #endregion

        #region Public Methods
        public override void ImportPeaks(List<DeconTools.Backend.DTO.MSPeakResult> peakList)
        {
             this.m_peakIndex = 0;

             var oldPeakResults = new DeconToolsV2.Results.clsTransformResults();
            Engine.Results.LcmsPeak[] importedPeaks = null;

             try
             {
                 oldPeakResults.ReadResults(this.m_filename);
                 oldPeakResults.GetRawData(out importedPeaks);
             }
             catch (Exception ex)
             {
                 throw ex;
             }

             foreach (var p in importedPeaks)
             {
                 var peak = ConvertDecon2LSPeakToPeakResult(p);
                 m_peakIndex++;

                 peakList.Add(peak);
             }
        }

        #endregion

        #region Private Methods

        protected override void reportProgress(int progressCounter)
        {
            base.reportProgress(progressCounter);
        }

        private MSPeakResult ConvertDecon2LSPeakToPeakResult(Engine.Results.LcmsPeak p)
        {
            float intensity;
            if (p.Intensity > float.MaxValue)
                intensity = float.MaxValue;
            else
                intensity = (float)p.Intensity;

            var peakResult = new MSPeakResult
            {
                MSPeak = new MSPeak(p.Mz, intensity),
                Scan_num = p.ScanNum,
                PeakID = this.m_peakIndex
            };

            return peakResult;
        }

        #endregion
    }
}
#endif
