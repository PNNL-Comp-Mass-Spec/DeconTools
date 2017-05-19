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
#if !Disable_DeconToolsV2
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
            if (!File.Exists(filename)) throw new System.IO.IOException("PeakImporter failed. File doesn't exist: " + Utilities.DiagnosticUtilities.GetFullPathSafe(filename));

            var fi = new FileInfo(filename);
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

             var oldProcessor = new DeconToolsV2.Peaks.clsPeakProcessor();
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

             //oldPeakResults.__dtor();
             //oldProcessor.__dtor();


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
                MSPeak = new MSPeak
                {
                    XValue = p.Mz,
                    Height = intensity
                },
                Scan_num = p.ScanNum,
                PeakID = m_peakIndex
            };


            return peakResult;

        }

        #endregion

    }
#endif


}
