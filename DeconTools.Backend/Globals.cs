﻿
using System;

namespace DeconTools.Backend
{
    public class Globals
    {
        public const double PROTON_MASS = 1.00727649;
        public const double MASS_DIFF_BETWEEN_ISOTOPICPEAKS = 1.00235;
        public const double N14_MASS = 14.003074007418;    // IUPAC, 2002
        public const double N15_MASS = 15.000108973000;

        public const double Hydrogen_MASS = 1.00782503196;    // IUPAC, 2007
        public const double Deuterium_MASS = 2.01410177796;   // IUPAC, 2007

        public enum MSFileType
        {
            Undefined,
            Agilent_WIFF,
            Agilent_D,
            Ascii,
            Bruker,
            Bruker_Ascii,
            Thermo_Raw,
            ICR2LS_Rawdata,
            Micromass_Rawdata,
            MZXML_Rawdata,
            MZ5,
            MZML,
            PNNL_IMS,
            PNNL_UIMF,
            SUNEXTREL,
            [Obsolete("Yafms is an unsupported file format")]
            YAFMS,
            Bruker_V2
        }

        public enum PeakFitType
        {
            Undefined,
            APEX,
            LORENTZIAN,
            QUADRATIC
        }

        public enum IsotopicProfileFitType
        {
            Undefined,
            AREA,
            CHISQ,
            PEAK
        }
        public enum DeconState
        {
            IDLE = 0,
            RUNNING_DECON,
            RUNNING_TIC,
            DONE,
            ERROR
        }

        public enum PeakDetectorType
        {
            DeconTools,
            DeconToolsChromPeakDetector
        }

        public enum DeconvolutionType
        {
            /// <summary>
            /// No deconvolution
            /// </summary>
            None,

            /// <summary>
            /// 2016 port of ThrashV1 in DeconEngineV2 to C#, .NET 4
            /// This is the preferred deconvoluter
            /// </summary>
            ThrashV1,

            /// <summary>
            /// 2012 port of DeconEngine to C#
            /// </summary>
            /// <remarks>As of 2016, not used because results do not agree with ThrashV1, C++</remarks>
            ThrashV2,

            /// <summary>
            /// Experimental deconvoluter; not recommended for use
            /// </summary>
            Rapid
        }

        public enum ExporterType
        {
            Text,
            Sqlite
        }

        public enum PeakSelectorMode            //for selecting the best peak from a list of peaks
        {
            Smart,
            ClosestToTarget,
            MostIntense,
            RelativeToOtherChromPeak,
            N15IntelligentMode,
            SmartUIMF
        }

        public enum MassTagDBImporterMode
        {
            Std_four_parameter_mode,
            List_of_MT_IDs_Mode
        }

        public enum ResultType
        {
            BASIC_TRADITIONAL_RESULT,
            UIMF_TRADITIONAL_RESULT,    // this is used for .UIMF files (LC-IMS-MS data)
            IMS_TRADITIONAL_RESULT,         // this is for .imf type files (uncommon now)
            O16O18_TRADITIONAL_RESULT,
            BASIC_TARGETED_RESULT,
            O16O18_TARGETED_RESULT,
            N14N15_TARGETED_RESULT,
            SIPPER_TARGETED_RESULT,
            DECON_MSN_RESULT,
            TOPDOWN_TARGETED_RESULT,
            DEUTERATED_TARGETED_RESULT
        }

        public enum ScanSelectionMode
        {
            ASCENDING,
            DESCENDING,
            CLOSEST
        }

        public enum ScanBasedWorkflowType
        {
            standard,
            uimf_standard,
            uimf_saturation_workflow,
        }

        public enum IsotopicProfileType
        {
            UNLABELED,
            LABELED
        }

        public enum ProjectControllerType
        {
            UNDEFINED,
            STANDARD,
            BONES_CONTROLLER,
            RUN_MERGER_CONTROLLER,
            KOREA_IMS_PEAKSONLY_CONTROLLER,
            UIMF_MS_Only_TestController
        }

        public enum LabelingType
        {
            NONE,
            O18,
            N15,
            Deuterium
        }

        public enum TargetedResultFailureType
        {
            None,
            ChromDataNotFound,
            ChromPeaksNotDetected,
            ChrompeakNotFoundWithinTolerances,
            MspeaksNotDetected,
            MsfeatureNotFound,
            TooManyHighQualityChrompeaks,
            QuantifierFailure,
            DeisotopingProblemDetected,
            MinimalCriteriaNotMet
        }

        public enum TargetedFeatureFinderType
        {
            BASIC,
            ITERATIVE
        }

        public enum SmoothingType
        {
            SavitzkyGolay
        }

        public enum ElutionTimeUnit
        {
            NormalizedElutionTime,
            ElutionTimeInSeconds,
            ScanNum
        }

        /// <summary>
        /// raw ms data usually comes in these formats
        /// </summary>
        public enum RawDataType
        {
            Profile,
            Centroided,
            MixedProfileAndCentroided
        }

        public enum ChromatogramGeneratorMode
        {
            MZ_BASED,
            MONOISOTOPIC_PEAK,
            MOST_ABUNDANT_PEAK,
            TOP_N_PEAKS,
            O16O18_THREE_MONOPEAKS
        }

        public enum ToleranceUnit
        {
            PPM,
            MZ
        }
    }
}
