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
        private List<IsosResult> _currentMSFeatures = new List<IsosResult>();
        private DeconToolsPeakDetectorV2 _ms2PeakDetectorForCentroidData;
        private DeconToolsPeakDetectorV2 _ms2PeakDetectorForProfileData;

        private XYData _currentMS1XYValues;
        private List<Peak> _currentMS1Peaks;
        private string _outputFileName;
        private int _scanCounter;
        private int _currentMS1Scan;
        private string _outputSummaryFilename;
        private const int NumScansBetweenProgress = 500;



        #region Constructors

        public DeconMSnWorkflow(DeconToolsParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputFolderPath, backgroundWorker)
        {
            ToleranceInPPM = 30;
            parameters.ScanBasedWorkflowParameters.ProcessMS2 = true;

            DeconMSnResults = new List<DeconMSnResult>();
        }

        protected override void InitializeProcessingTasks()
        {

            MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
            PeakDetector = PeakDetectorFactory.CreatePeakDetector(NewDeconToolsParameters);
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

        /// <summary>
        /// Information about the type of MS2 data: centroided or profile. This
        /// has important implications on the MS2 peak detection
        /// </summary>
        public Globals.RawDataType MS2DataType { get; set; }

        #endregion

        #region Public Methods

        public List<DeconMSnResult> DeconMSnResults { get; set; }



        #endregion

        #region Private Methods

        #endregion


        protected override void CreateOutputFileNames()
        {
            string basefileName = GetBaseFileName(Run);

            Logger.Instance.OutputFilename = basefileName + "_log.txt";
            _outputFileName = basefileName + ".mgf";
            _outputSummaryFilename = basefileName + "_DeconMSn_log.txt";

            if (File.Exists(_outputFileName)) File.Delete(_outputFileName);
            if (File.Exists(_outputSummaryFilename)) File.Delete(_outputSummaryFilename);
            
        }



        protected override void IterateOverScans()
        {
            DeconMSnResults.Clear();
            _scanCounter = 0;

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
                int currentMSLevel = Run.GetMSLevel(scanSet.PrimaryScanNumber);

                if (currentMSLevel == 1)
                {
                    _currentMS1Scan = scanSet.PrimaryScanNumber;
                    _currentMSFeatures.Clear();
                    Run.ResultCollection.IsosResultBin.Clear();

                    Run.CurrentScanSet = scanSet;

                    MSGenerator.Execute(Run.ResultCollection);

                    _currentMS1XYValues = new XYData();
                    _currentMS1XYValues.Xvalues = Run.XYData.Xvalues;
                    _currentMS1XYValues.Yvalues = Run.XYData.Yvalues;

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

                    MSGenerator.Execute(Run.ResultCollection);
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

                    double inaccurateParentMZ = precursorInfo.PrecursorMZ;
                    double lowerMZ = inaccurateParentMZ - 1.1;
                    double upperMZ = inaccurateParentMZ + 1.1;


                    var filteredMS1Peaks = _currentMS1Peaks.Where(p => p.XValue > lowerMZ && p.XValue < upperMZ).ToList();
                    var ms1Features = ((ThrashDeconvolutorV2)Deconvolutor).PerformThrash(_currentMS1XYValues, filteredMS1Peaks, 0, 0, 0);


                    var deconMSnResult = new DeconMSnResult();
                    deconMSnResult.ParentScan = Run.GetParentScan(scanSet.PrimaryScanNumber);
                    deconMSnResult.ScanNum = scanSet.PrimaryScanNumber;
                    deconMSnResult.OriginalMZTarget = inaccurateParentMZ;

                    List<IsotopicProfile> candidateMS1Features = new List<IsotopicProfile>();
                    foreach (var msfeature in ms1Features)
                    {
                        foreach (var peak in msfeature.Peaklist)
                        {
                            double currentDiff = Math.Abs(peak.XValue - inaccurateParentMZ);

                            double toleranceInMZ = ToleranceInPPM * peak.XValue / 1e6;

                            if (currentDiff < toleranceInMZ)
                            {
                                candidateMS1Features.Add(msfeature);
                            }

                        }
                    }

                    IsotopicProfile selectedMS1Feature = null;
                    if (candidateMS1Features.Count == 0)
                    {
                        //TODO: perform another round
                    }
                    else if (candidateMS1Features.Count == 1)
                    {
                        selectedMS1Feature = candidateMS1Features.First();


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
                        List<Peak> candidatePeaks = new List<Peak>();

                        foreach (var peak in filteredMS1Peaks)
                        {
                            double currentDiff = Math.Abs(peak.XValue - inaccurateParentMZ);

                            double toleranceInMZ = ToleranceInPPM * peak.XValue / 1e6;

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
                    string outputString = GetMGFOutputString(Run, scanSet.PrimaryScanNumber, deconMSnResult, ms2Peaks);

                    if (ExportData)
                    {
                        WriteOutData(outputString);
                    }

                    if (deconMSnResult.ParentIntensity > 0)
                    {
                        DeconMSnResults.Add(deconMSnResult);
                    }




                }
                else
                {
                    throw new System.ApplicationException(
                        "DeconMSn only works on MS1 and MS2 data; You are attempting MS3");
                }


                ReportProgress();

            }

            if (ExportData)
            {
                string deconResultsStringOutput = DeconMSnResultsToString1(DeconMSnResults);
                WriteOutDeconMSnSummary(deconResultsStringOutput);


                //Console.WriteLine(deconResultsStringOutput);

            }


        }

        private void WriteOutDeconMSnSummary(string deconResultsStringOutput)
        {
            using (var sw = new StreamWriter(new System.IO.FileStream(_outputSummaryFilename, System.IO.FileMode.Append,
                      System.IO.FileAccess.Write, System.IO.FileShare.Read)))
            {
                sw.AutoFlush = true;
                sw.Write(deconResultsStringOutput);
            }
        }

        private string DeconMSnResultsToString1(List<DeconMSnResult> deconMSnResults)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("MSn_Scan\tMSn_Level\tParent_Scan\tParent_Scan_Level\tParent_Mz\tMono_Mz\tCharge_State\tMonoisotopic_Mass\tIsotopic_Fit\tParent_Intensity\tMono_Intensity\tOriginalMZ\tExtraInfo");
            sb.Append(Environment.NewLine);

            string delimiter = "\t";
            foreach (var result in deconMSnResults)
            {
                sb.Append(result.ScanNum);
                sb.Append(delimiter);
                sb.Append(2);
                sb.Append(delimiter);
                sb.Append(result.ParentScan);
                sb.Append(delimiter);
                sb.Append(1);
                sb.Append(delimiter);
                sb.Append(result.ParentMZ.ToString("0.00000"));
                sb.Append(delimiter);
                sb.Append(result.ParentMZ.ToString("0.00000"));
                sb.Append(delimiter);
                sb.Append(result.ParentChargeState);
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile != null ? result.IsotopicProfile.MonoIsotopicMass.ToString("0.00000") : "-1");
                sb.Append(delimiter);
                sb.Append(result.IsotopicProfile != null ? result.IsotopicProfile.Score.ToString("0.0000") : "-1");
                sb.Append(delimiter);
                sb.Append(result.ParentIntensity.ToString("0"));
                sb.Append(delimiter);
                sb.Append(result.IntensityAggregate.ToString("0"));
                sb.Append(delimiter);
                sb.Append(result.OriginalMZTarget);
                sb.Append(delimiter);

                sb.Append(result.ExtraInfo);
                sb.Append(Environment.NewLine);

            }

            return sb.ToString();
        }

        protected override Globals.ResultType GetResultType()
        {
            return Globals.ResultType.DECON_MSN_RESULT;
        }

        protected override void WriteProcessingInfoToLog()
        {
            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(ScanBasedWorkflow)));
            Logger.Instance.AddEntry("ParameterFile = " + (NewDeconToolsParameters.ParameterFilename == null ? "[NONE]" :
                Path.GetFileName(NewDeconToolsParameters.ParameterFilename)), Logger.Instance.OutputFilename);
        }

        private void WriteOutData(string outputString)
        {
            using (var sw = new StreamWriter(new System.IO.FileStream(_outputFileName, System.IO.FileMode.Append,
                      System.IO.FileAccess.Write, System.IO.FileShare.Read)))
            {
                sw.AutoFlush = true;
                sw.Write(outputString);
                sw.Flush();

                sw.Close();
            }
        }

        protected override void WriteOutSummaryToLogfile()
        {
            Logger.Instance.AddEntry("Finished file processing", Logger.Instance.OutputFilename);

            string formattedOverallprocessingTime = string.Format("{0:00}:{1:00}:{2:00}",
                WorkflowStats.ElapsedTime.Hours, WorkflowStats.ElapsedTime.Minutes, WorkflowStats.ElapsedTime.Seconds);

            Logger.Instance.AddEntry("total processing time = " + formattedOverallprocessingTime);
            Logger.Instance.WriteToFile(Logger.Instance.OutputFilename);
            Logger.Instance.Close();
        }


        private string GetMGFOutputString(Run run, int scanNum, DeconMSnResult deconMSnResult, IEnumerable<Peak> ms2Peaks)
        {
            StringBuilder sb = new StringBuilder();

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


        public override void ReportProgress()
        {
            if (Run.ScanSetCollection == null || Run.ScanSetCollection.ScanSetList.Count == 0) return;

            ScanBasedProgressInfo userstate = new ScanBasedProgressInfo(Run, Run.CurrentScanSet, null);

            float percentDone = (float)(_scanCounter) / (float)(Run.ScanSetCollection.ScanSetList.Count) * 100;
            userstate.PercentDone = percentDone;

            string logText = "Scan= " + Run.GetCurrentScanOrFrame() + "; PercentComplete= " +
                             percentDone.ToString("0.0");

            if (BackgroundWorker != null)
            {
                BackgroundWorker.ReportProgress((int)percentDone, userstate);
            }

            if (_scanCounter % NumScansBetweenProgress == 0)
            {
                Logger.Instance.AddEntry(logText, Logger.Instance.OutputFilename);

                if (BackgroundWorker == null)
                {
                    Console.WriteLine(DateTime.Now + "\t" + logText);
                }

            }
        }
    }
}
