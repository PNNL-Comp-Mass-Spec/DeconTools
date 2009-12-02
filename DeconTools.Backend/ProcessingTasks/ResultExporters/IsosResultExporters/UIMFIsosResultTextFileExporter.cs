﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.IsosResultExporters
{
    public class UIMFIsosResultTextFileExporter:IsosResultTextFileExporter
    {
        private int triggerVal;
        private char delimiter;

        #region Constructors
        public UIMFIsosResultTextFileExporter(string fileName)
            : this(fileName, 1000000)
        {

        }

        public UIMFIsosResultTextFileExporter(string fileName, int triggerValueToExport)
        {
            try
            {
                sw = new StreamWriter(fileName);
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("IsosResultExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw new Exception("Result exporter failed.  Check to see if it is already open or not.");
            }

            this.TriggerToExport = triggerValueToExport;
            this.delimiter = ',';

            sw.Write(buildHeaderLine());

        }
        #endregion

        #region Properties
        public override char Delimiter
        {
            get
            {
                return delimiter;
            }
            set
            {
                delimiter = value;
            }
        }
        public override int TriggerToExport
        {
            get
            {
                return triggerVal;
            }
            set
            {
                triggerVal = value;
            }
        }

        #endregion

        protected override string buildIsosResultOutput(DeconTools.Backend.Core.IsosResult result)
        {
            Check.Require(result is UIMFIsosResult, "UIMF Isos Exporter is only used with UIMF results");
            UIMFIsosResult uimfResult = (UIMFIsosResult)result;

            StringBuilder sb = new StringBuilder();
            sb.Append(uimfResult.FrameSet.PrimaryFrame);
            sb.Append(delimiter);
            sb.Append(uimfResult.ScanSet.PrimaryScanNumber + 1);    //adds 1 to PrimaryScanNumber (which is 0-based)
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.ChargeState);
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetAbundance());
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMZ().ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetScore().ToString("0.####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.AverageMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.MonoIsotopicMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.MostAbundantIsotopeMass.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetFWHM().ToString("0.####"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetSignalToNoise().ToString("0.##"));
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMonoAbundance());
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.GetMonoPlusTwoAbundance());
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.OriginalIntensity);
            sb.Append(delimiter);
            sb.Append(uimfResult.IsotopicProfile.Original_Total_isotopic_abundance);
            sb.Append(delimiter);
            sb.Append(uimfResult.ScanSet.DriftTime.ToString("0.###"));
            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("frame_num");
            sb.Append(Delimiter);
            sb.Append("ims_scan_num");
            sb.Append(Delimiter);
            sb.Append("charge");
            sb.Append(Delimiter);
            sb.Append("abundance");
            sb.Append(Delimiter);
            sb.Append("mz");
            sb.Append(Delimiter);
            sb.Append("fit");
            sb.Append(Delimiter);
            sb.Append("average_mw");
            sb.Append(Delimiter);
            sb.Append("monoisotopic_mw");
            sb.Append(Delimiter);
            sb.Append("mostabundant_mw");
            sb.Append(Delimiter);
            sb.Append("fwhm");
            sb.Append(Delimiter);
            sb.Append("signal_noise");
            sb.Append(Delimiter);
            sb.Append("mono_abundance");
            sb.Append(Delimiter);
            sb.Append("mono_plus2_abundance");
            sb.Append(Delimiter);
            sb.Append("orig_intensity");
            sb.Append(Delimiter);
            sb.Append("TIA_orig_intensity");
            sb.Append(Delimiter);
            sb.Append("drift_time");
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}