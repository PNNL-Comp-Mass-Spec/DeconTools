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
        private readonly string m_filename;
        private int m_peakIndex;   //each imported peak is given an index

#region Constructors
        public PeakImporterFromOldPeakDatFile(string filename)
            : this(filename, null)
        {

        }

        public PeakImporterFromOldPeakDatFile(string filename, BackgroundWorker bw)
        {
            if (!File.Exists(filename)) throw new IOException("PeakImporter failed. File doesn't exist: " + Backend.Utilities.DiagnosticUtilities.GetFullPathSafe(filename));

            var fi = new FileInfo(filename);
            numRecords = (int)(fi.Length / 1000 * 26);   // a way of approximating how many peaks there are... only for use with the backgroundWorker

            m_filename = filename;
            backgroundWorker = bw;
        }

#endregion

#region Public Methods
        public override void ImportPeaks(List<MSPeakResult> peakList)
        {
            m_peakIndex = 0;

            var oldPeakResults = new DeconToolsV2.Results.clsTransformResults();
            Engine.Results.LcmsPeak[] importedPeaks;

            try
            {
#pragma warning disable CS0618 // Type or member is obsolete
                oldPeakResults.ReadResults(m_filename);
#pragma warning restore CS0618 // Type or member is obsolete
                oldPeakResults.GetRawData(out importedPeaks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
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
                PeakID = m_peakIndex
            };

            return peakResult;
        }

#endregion
    }
}
#endif
