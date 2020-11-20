using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;
using Peak = DeconTools.Backend.Core.Peak;

namespace DeconTools.Backend.Workflows
{
    public class DeconMSnWorkflow : ScanBasedWorkflow
    {
        private DeconToolsPeakDetectorV2 _moreSensitiveMS1PeakDetector;
        private DeconToolsPeakDetectorV2 _superSensitiveMS1PeakDetector;

        private DeconToolsPeakDetectorV2 _ms2PeakDetectorForCentroidData;
        private DeconToolsPeakDetectorV2 _ms2PeakDetectorForProfileData;

        private XYData _currentMS1XYValues;
        private List<Peak> _currentMS1Peaks;
        private string _outputFileName;
        private int _scanCounter;
        private string _outputSummaryFilename;
        private string _outputParentProfileDataFilename;

        private double _currentMS1TICIntensity;
        private const int NumScansBetweenProgress = 500;

         private readonly string _delimiter = "\t";

        #region Constructors

        public DeconMSnWorkflow(DeconToolsParameters parameters, Run run, string outputDirectoryPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputDirectoryPath, backgroundWorker)
        {
            ToleranceInPPM = 30;
            NumMaxAttemptsAtLowIntensitySpecies = 4;
            parameters.ScanBasedWorkflowParameters.ProcessMS2 = true;
            DeconMSnResults = new List<DeconMSnResult>();
            IsParentProfileDataExported = true;
        }

        protected override void InitializeProcessingTasks()
        {
            MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
            PeakDetector = PeakDetectorFactory.CreatePeakDetector(NewDeconToolsParameters);

            var moreSensitivePeakToBackgroundRatio = NewDeconToolsParameters.PeakDetectorParameters.PeakToBackgroundRatio / 2;
            var moreSensitiveSigNoiseThresh = NewDeconToolsParameters.PeakDetectorParameters.SignalToNoiseThreshold;
            _moreSensitiveMS1PeakDetector = new DeconToolsPeakDetectorV2(moreSensitivePeakToBackgroundRatio,
                                             moreSensitiveSigNoiseThresh, NewDeconToolsParameters.PeakDetectorParameters.PeakFitType,
                                             NewDeconToolsParameters.PeakDetectorParameters.IsDataThresholded);

            _superSensitiveMS1PeakDetector = new DeconToolsPeakDetectorV2(0, 0, NewDeconToolsParameters.PeakDetectorParameters.PeakFitType,
                                             NewDeconToolsParameters.PeakDetectorParameters.IsDataThresholded);

            Deconvolutor = DeconvolutorFactory.CreateDeconvolutor(NewDeconToolsParameters);

            //Will initialize these but whether or not they are used are determined elsewhere
            ZeroFiller = new DeconToolsZeroFiller(NewDeconToolsParameters.MiscMSProcessingParameters.ZeroFillingNumZerosToFill);
            Smoother = new SavitzkyGolaySmoother(NewDeconToolsParameters.MiscMSProcessingParameters.SavitzkyGolayNumPointsInSmooth,
                NewDeconToolsParameters.MiscMSProcessingParameters.SavitzkyGolayOrder);

            FitScoreCalculator = new DeconToolsFitScoreCalculator();

            ResultValidator = new ResultValidatorTask();

            PeakToMSFeatureAssociator = new PeakToMSFeatureAssociator();

            _ms2PeakDetectorForCentroidData = new DeconToolsPeakDetectorV2(0, 0, Globals.PeakFitType.QUADRATIC, true) { RawDataType = Globals.RawDataType.Centroided };

            _ms2PeakDetectorForProfileData = new DeconToolsPeakDetectorV2(NewDeconToolsParameters.PeakDetectorParameters.PeakToBackgroundRatio,
                                             NewDeconToolsParameters.PeakDetectorParameters.SignalToNoiseThreshold,
                                             NewDeconToolsParameters.PeakDetectorParameters.PeakFitType,
                                             NewDeconToolsParameters.PeakDetectorParameters.IsDataThresholded);

            Check.Ensure(Deconvolutor is ThrashDeconvolutorV2, "Error. Currently the DeconMSn workflow only works with the ThrashV2 deconvolutor. Selected deconvolutor= " + Deconvolutor);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The tolerance used in comparing the inaccurate precursor m/z to the accurate m/z values from the MS1
        /// </summary>
        public double ToleranceInPPM { get; set; }

        public List<DeconMSnResult> DeconMSnResults { get; set; }

        /// <summary>
        /// Number of attempts at trying to find MS1 features. This
        /// In each attempt, two more spectra are summed.  In the last attempt,
        /// the peak detector thresholds are dropped to lowest levels so that all
        /// peaks are reported and used in the Thrash algorithm
        /// </summary>
        public int NumMaxAttemptsAtLowIntensitySpecies { get; set; }

        /// <summary>
        /// Create a _profile.txt file. This supports DTARefinery which looks for this file.
        /// Otherwise, the same data can be found in the _DeconMSn_log.txt file
        /// </summary>
        public bool IsParentProfileDataExported { get; set; }

        #endregion

        #region Public Methods

        protected override void IterateOverScans()
        {
            DeconMSnResults.Clear();
            _scanCounter = 0;

            var resultCounter = 0;

            foreach (var scanSet in Run.ScanSetCollection.ScanSetList)
            {
                _scanCounter++;

                if (BackgroundWorker != null)
                {
                    if (BackgroundWorker.CancellationPending)
                    {
                        return;
                    }
                }

                //check ms level
                var currentMSLevel = Run.GetMSLevel(scanSet.PrimaryScanNumber);

                if (currentMSLevel == 1)
                {
                    Run.ResultCollection.IsosResultBin.Clear();

                    Run.CurrentScanSet = scanSet;

                    MSGenerator.Execute(Run.ResultCollection);

                    _currentMS1XYValues = new XYData
                    {
                        Xvalues = Run.XYData.Xvalues,
                        Yvalues = Run.XYData.Yvalues
                    };

                    _currentMS1TICIntensity = Run.GetTICFromInstrumentInfo(scanSet.PrimaryScanNumber);

                    PeakDetector.Execute(Run.ResultCollection);
                    _currentMS1Peaks = new List<Peak>(Run.PeakList);
                }
                else if (currentMSLevel == 2)
                {
                    if (_currentMS1Peaks == null || _currentMS1Peaks.Count == 0)
                    {
                        continue;
                    }

                    var precursorInfo = Run.GetPrecursorInfo(scanSet.PrimaryScanNumber);
                    Run.CurrentScanSet = scanSet;
                    var inaccurateParentMZ = precursorInfo.PrecursorMZ;

                    resultCounter++;
                    var deconMSnResult = new DeconMSnResult {ParentScan = Run.GetParentScan(scanSet.PrimaryScanNumber)};
                    deconMSnResult.IonInjectionTime = Run.GetIonInjectionTimeInMilliseconds(deconMSnResult.ParentScan);
                    deconMSnResult.ScanNum = scanSet.PrimaryScanNumber;
                    deconMSnResult.OriginalMZTarget = inaccurateParentMZ;
                    deconMSnResult.ParentScanTICIntensity = _currentMS1TICIntensity;

                    MSGenerator.Execute(Run.ResultCollection);

                    var lowerMZ = inaccurateParentMZ - 1.1;
                    var upperMZ = inaccurateParentMZ + 1.1;

                    var dataIsCentroided = Run.IsDataCentroided(scanSet.PrimaryScanNumber);
                    if (dataIsCentroided)
                    {
                        _ms2PeakDetectorForCentroidData.Execute(Run.ResultCollection);
                    }
                    else
                    {
                        _ms2PeakDetectorForProfileData.Execute(Run.ResultCollection);
                    }
                    var ms2Peaks = new List<Peak>(Run.PeakList);

                    var filteredMS1Peaks = _currentMS1Peaks.Where(p => p.XValue > lowerMZ && p.XValue < upperMZ).ToList();

                    IsotopicProfile selectedMS1Feature = null;
                    for (var attemptNum = 0; attemptNum < NumMaxAttemptsAtLowIntensitySpecies; attemptNum++)
                    {
                        var candidateMS1Features = GetCandidateMS1Features(inaccurateParentMZ, filteredMS1Peaks);

                        //if none were found, will regenerate MS1 spectrum and find peaks again
                        if (candidateMS1Features.Count == 0)
                        {
                            var numSummed = attemptNum * 2 + 3;
                            var ms1Scan = precursorInfo.PrecursorScan;

                            var ms1ScanSet = new ScanSetFactory().CreateScanSet(Run, ms1Scan, numSummed);

                            //get MS1 mass spectrum again. This time sum spectra
                            Run.CurrentScanSet = ms1ScanSet;
                            MSGenerator.Execute(Run.ResultCollection);

                            //run MS1 peak detector, with greater sensitivity
                            var isLastAttempt = attemptNum >= NumMaxAttemptsAtLowIntensitySpecies - 2;    //need to do -2 because of the way the loop advances the counter.

                            if (isLastAttempt)
                            {
                                _superSensitiveMS1PeakDetector.MinX = lowerMZ;
                                _superSensitiveMS1PeakDetector.MaxX = upperMZ;
                                _superSensitiveMS1PeakDetector.Execute(Run.ResultCollection);
                                filteredMS1Peaks = new List<Peak>(Run.PeakList);
                            }
                            else
                            {
                                _moreSensitiveMS1PeakDetector.Execute(Run.ResultCollection);
                                var moreSensitiveMS1Peaks = new List<Peak>(Run.PeakList);
                                filteredMS1Peaks = moreSensitiveMS1Peaks.Where(p => p.XValue > lowerMZ && p.XValue < upperMZ).ToList();
                            }
                        }
                        else if (candidateMS1Features.Count == 1)
                        {
                            selectedMS1Feature = candidateMS1Features.First();
                            break;   //we found something, so no need for any further attempts
                        }
                        else
                        {
                            var highQualityCandidates = candidateMS1Features.Where(p => p.Score < 0.15).ToList();
                            if (highQualityCandidates.Count == 0)
                            {
                                selectedMS1Feature = candidateMS1Features.OrderByDescending(p => p.IntensityMostAbundantTheor).First();
                            }
                            else if (highQualityCandidates.Count == 1)
                            {
                                selectedMS1Feature = highQualityCandidates.First();
                            }
                            else
                            {
                                selectedMS1Feature = highQualityCandidates.OrderByDescending(p => p.IntensityMostAbundantTheor).First();
                            }

                            deconMSnResult.ExtraInfo = "Warning - multiple MSFeatures found for target parent MZ";
                            break;   //we found something, so no need for any further attempts
                        }
                    }

                    if (selectedMS1Feature != null)
                    {
                        deconMSnResult.ParentMZ = selectedMS1Feature.MonoPeakMZ;
                        deconMSnResult.ParentChargeState = selectedMS1Feature.ChargeState;
                        deconMSnResult.ParentIntensity = selectedMS1Feature.IntensityMostAbundantTheor;
                        deconMSnResult.IntensityAggregate = selectedMS1Feature.IntensityMostAbundantTheor;
                        deconMSnResult.IsotopicProfile = selectedMS1Feature;
                    }
                    else
                    {
                        var candidatePeaks = new List<Peak>();

                        foreach (var peak in filteredMS1Peaks)
                        {
                            var currentDiff = Math.Abs(peak.XValue - inaccurateParentMZ);

                            var toleranceInMZ = ToleranceInPPM * peak.XValue / 1e6;

                            if (currentDiff < toleranceInMZ)
                            {
                                candidatePeaks.Add(peak);
                            }
                        }

                        Peak selectedPeak = null;

                        if (candidatePeaks.Count == 0)
                        {
                            //cannot even find a suitable MS1 peak. So can't do anything

                        }
                        else if (candidatePeaks.Count == 1)
                        {
                            selectedPeak = candidatePeaks.First();
                        }
                        else
                        {
                            selectedPeak = candidatePeaks.OrderByDescending(p => p.Height).First();
                        }

                        if (selectedPeak != null)
                        {
                            deconMSnResult.ParentMZ = selectedPeak.XValue;
                            deconMSnResult.ParentChargeState = 1;   //not sure what charge I should assign... Ask SangTae
                            deconMSnResult.ParentIntensity = selectedPeak.Height;
                            deconMSnResult.IsotopicProfile = null;
                            deconMSnResult.ExtraInfo = "Failure code 1: No MSFeature, but peak found";
                        }
                        else
                        {
                            deconMSnResult.ParentMZ = precursorInfo.PrecursorMZ;    //could not find the accurate parentMZ, so just report what the instrument found.
                            deconMSnResult.ParentChargeState = 1;   //not sure what charge I should assign... Ask SangTae
                            deconMSnResult.ParentIntensity = 0;
                            deconMSnResult.IsotopicProfile = null;
                            deconMSnResult.ExtraInfo = "Failure code 2: No MSFeature or peak found";
                        }
                    }

                    //Build export data.
                    var outputString = GetMGFOutputString(Run, scanSet.PrimaryScanNumber, deconMSnResult, ms2Peaks);

                    var includeHeader = resultCounter == 1;
                    var summaryString = GetSummaryString(deconMSnResult, includeHeader);

                    var parentProfileString = GetParentProfileString(deconMSnResult, includeHeader);

                    if (ExportData)
                    {
                        WriteOutMainData(outputString);

                        WriteOutDeconMSnSummary(summaryString);

                        if (IsParentProfileDataExported)
                        {
                            WriteOutParentProfileInfoString(parentProfileString);
                        }
                    }

                    if (deconMSnResult.ParentIntensity > 0)
                    {
                        DeconMSnResults.Add(deconMSnResult);
                    }
                }
                else
                {
                    throw new ApplicationException(
                        "DeconMSn only works on MS1 and MS2 data; You are attempting MS3");
                }

                ReportProgress();
            }
        }

        protected override void CreateOutputFileNames()
        {
            var baseFileName = GetBaseFileName(Run);

            Logger.Instance.OutputFilename = baseFileName + "_log.txt";
            _outputFileName = baseFileName + ".mgf";
            _outputSummaryFilename = baseFileName + "_DeconMSn_log.txt";
            _outputParentProfileDataFilename = baseFileName + "_profile.txt";

            if (File.Exists(_outputFileName)) File.Delete(_outputFileName);
            if (File.Exists(_outputSummaryFilename)) File.Delete(_outputSummaryFilename);
            if (File.Exists(_outputParentProfileDataFilename)) File.Delete(_outputParentProfileDataFilename);
        }

        protected override Globals.ResultType GetResultType()
        {
            return Globals.ResultType.DECON_MSN_RESULT;
        }

        public virtual void ReportProgress()
        {
            if (Run.ScanSetCollection == null || Run.ScanSetCollection.ScanSetList.Count == 0) return;

            var userState = new ScanBasedProgressInfo(Run, Run.CurrentScanSet);

            var percentDone = _scanCounter / (float)(Run.ScanSetCollection.ScanSetList.Count) * 100;
            userState.PercentDone = percentDone;

            var logText = "Scan= " + Run.GetCurrentScanOrFrame() + "; PercentComplete= " +
                             percentDone.ToString("0.0");

            BackgroundWorker?.ReportProgress((int)percentDone, userState);

            if (_scanCounter % NumScansBetweenProgress == 0)
            {
                Logger.Instance.AddEntry(logText, true);

                if (BackgroundWorker == null)
                {
                    Console.WriteLine(DateTime.Now + "\t" + logText);
                }
            }
        }
        #endregion

        #region Private Methods

        private string GetParentProfileString(DeconMSnResult result, bool includeHeader)
        {
            var headers = new List<string> {
                "MSn_Scan",
                "Parent_Scan",
                "AGC_accumulation_time",
                "TIC" };

            var data = new List<string>
            {
                result.ScanNum.ToString(),
                result.ParentScan.ToString(),
                result.IonInjectionTime.ToString("0.0000"),
                result.ParentScanTICIntensity.ToString("0")
            };

            if (includeHeader)
            {
                return string.Join(_delimiter, headers) + Environment.NewLine + string.Join(_delimiter, data);
            }

            return string.Join(_delimiter, data);
        }

        private string GetSummaryString(DeconMSnResult result, bool includeHeader = false)
        {
            var headers = new List<string>
            {
                "MSn_Scan",
                "MSn_Level",
                "Parent_Scan",
                "Parent_Scan_Level",
                "Parent_Mz",
                "Mono_Mz",
                "Charge_State",
                "Monoisotopic_Mass",
                "Isotopic_Fit",
                "Parent_Intensity",
                "Mono_Intensity",
                "Parent_TIC",
                "Ion_Injection_Time",
                "OriginalMZ",
                "ExtraInfo"
            };

            var data = new List<string>
            {
                result.ScanNum.ToString(),
                "2",
                result.ParentScan.ToString(),
                "1",
                result.ParentMZ.ToString("0.00000"),
                result.ParentMZ.ToString("0.00000"),
                result.ParentChargeState.ToString(),
                result.IsotopicProfile?.MonoIsotopicMass.ToString("0.00000") ?? "-1",
                result.IsotopicProfile?.Score.ToString("0.0000") ?? "-1",
                result.ParentIntensity.ToString("0"),
                result.IntensityAggregate.ToString("0"),
                result.ParentScanTICIntensity.ToString("0"),
                result.IonInjectionTime.ToString("0.0000"),
                result.OriginalMZTarget.ToString("0.0000"),
                result.ExtraInfo
            };

            if (includeHeader)
            {
                return string.Join(_delimiter, headers) + Environment.NewLine + string.Join(_delimiter, data);
            }

            return string.Join(_delimiter, data);
        }

        private string GetMGFOutputString(Run run, int scanNum, DeconMSnResult deconMSnResult, IEnumerable<Peak> ms2Peaks)
        {
            var sb = new StringBuilder();

            sb.Append("BEGIN IONS");
            sb.Append(Environment.NewLine);
            sb.Append("TITLE=" + run.DatasetName + "." + scanNum + "." + scanNum + ".1.dta");
            sb.Append(Environment.NewLine);
            sb.Append("PEPMASS=" + deconMSnResult.ParentMZ);
            sb.Append(Environment.NewLine);
            sb.Append("CHARGE=" + deconMSnResult.ParentChargeState + "+");
            sb.Append(Environment.NewLine);
            foreach (var peak in ms2Peaks)
            {
                sb.Append(Math.Round(peak.XValue, 5));
                sb.Append(" ");
                sb.Append(peak.Height);
                sb.Append(Environment.NewLine);
            }
            sb.Append("END IONS");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        protected override void WriteProcessingInfoToLog()
        {
            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(ScanBasedWorkflow)));
            Logger.Instance.AddEntry("ParameterFile = " + (NewDeconToolsParameters.ParameterFilename == null ? "[NONE]" :
                Path.GetFileName(NewDeconToolsParameters.ParameterFilename)), true);
        }

        private void WriteOutMainData(string outputString)
        {
            using (var sw = new StreamWriter(new FileStream(_outputFileName, FileMode.Append,
                      FileAccess.Write, FileShare.Read)))
            {
                sw.Write(outputString);
                sw.Close();
            }
        }

        private void WriteOutDeconMSnSummary(string deconResultsStringOutput)
        {
            using (var sw = new StreamWriter(new FileStream(_outputSummaryFilename, FileMode.Append,
                      FileAccess.Write, FileShare.Read)))
            {
                sw.WriteLine(deconResultsStringOutput);
            }
        }

        private void WriteOutParentProfileInfoString(string outputString)
        {
            using (var sw = new StreamWriter(new FileStream(_outputParentProfileDataFilename, FileMode.Append,
                    FileAccess.Write, FileShare.Read)))
            {
                sw.WriteLine(outputString);
            }
        }

        protected override void WriteOutSummaryToLogfile()
        {
            Logger.Instance.AddEntry("Finished file processing", true);

            var formattedOverallProcessingTime = string.Format("{0:00}:{1:00}:{2:00}",
                WorkflowStats.ElapsedTime.Hours, WorkflowStats.ElapsedTime.Minutes, WorkflowStats.ElapsedTime.Seconds);

            Logger.Instance.AddEntry("total processing time = " + formattedOverallProcessingTime, true);
            Logger.Instance.Close();
        }

        #endregion

        private List<IsotopicProfile> GetCandidateMS1Features(double inaccurateParentMZ, List<Peak> filteredMS1Peaks)
        {
            var candidateMS1Features = new List<IsotopicProfile>();
            var ms1Features = ((ThrashDeconvolutorV2)Deconvolutor).PerformThrash(_currentMS1XYValues, filteredMS1Peaks, 0, 0, 0);

            foreach (var msFeature in ms1Features)
            {
                foreach (var peak in msFeature.Peaklist)
                {
                    var currentDiff = Math.Abs(peak.XValue - inaccurateParentMZ);

                    var toleranceInMZ = ToleranceInPPM * peak.XValue / 1e6;

                    if (currentDiff < toleranceInMZ)
                    {
                        candidateMS1Features.Add(msFeature);
                    }
                }
            }
            return candidateMS1Features;
        }
    }
}
