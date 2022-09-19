using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class ResultCollection
    {
        #region Constructors
        public ResultCollection(Run run)
        {
            Run = run;
            ResultList = new List<IsosResult>();
            MassTagResultList = new Dictionary<TargetBase, TargetedResultBase>();
            ScanResultList = new List<ScanResult>();
            msPeakResultsGroupedAndMzOrdered = new Dictionary<int, List<MSPeakResult>>();
            MSPeakResultList = new List<MSPeakResult>();
            IsosResultBin = new List<IsosResult>(10);
            LogMessageList = new List<string>();
            ElutingPeakCollection = new List<ElutingPeak>();
        }

        #endregion

        #region Properties

        public Dictionary<TargetBase, TargetedResultBase> MassTagResultList { get; set; }

        private List<MSPeakResult> msPeakResultList;
        public List<MSPeakResult> MSPeakResultList
        {
            get => msPeakResultList;
            set
            {
                msPeakResultList = value;
                msPeakResultsGroupedAndMzOrdered.Clear();
            }
        }

        public IList<IsosResult> IsosResultBin { get; set; }
        public List<ScanResult> ScanResultList { get; set; }

        public List<IsosResult> ResultList { get; set; }
        public Run Run { get; set; }
        public List<string> LogMessageList { get; set; }
        public List<ElutingPeak> ElutingPeakCollection { get; set; }

        /// <summary>
        /// Keeps track of the number of deisotoped features that have been found
        /// </summary>
        public int MSFeatureCounter { get; set; }

        /// <summary>
        /// Keeps track of the number of spectra that have been processed
        /// </summary>
        public int MSScanCounter { get; set; }

        //public Globals.IsosResultType ResultType { get; set; }

        public Globals.ResultType ResultType { get; set; }

        public int PeakCounter { get; set; }

        private Dictionary<int, List<MSPeakResult>> msPeakResultsGroupedAndMzOrdered;
        #endregion

        #region Public Methods
        public ScanResult GetCurrentScanResult()
        {
            if (ScanResultList == null || ScanResultList.Count == 0) return null;
            return ScanResultList[ScanResultList.Count - 1];
        }

        public Dictionary<int, List<MSPeakResult>> GetMsPeakResultsGroupedAndMzOrdered()
        {
            if (msPeakResultsGroupedAndMzOrdered?.Any() != true)
            {
                msPeakResultsGroupedAndMzOrdered = new Dictionary<int, List<MSPeakResult>>();

                if (msPeakResultList != null)
                {
                    // Group by scan number
                    foreach (var grouping in msPeakResultList.GroupBy(x => x.Scan_num))
                    {
                        // Order by m/z
                        var sortedPeaks = grouping.OrderBy(x => x.MSPeak.XValue).ToList();
                        msPeakResultsGroupedAndMzOrdered.Add(grouping.Key, sortedPeaks);
                    }
                }
            }

            return msPeakResultsGroupedAndMzOrdered;
        }

        public int getTotalIsotopicProfiles()
        {
            if (ScanResultList == null) return 0;

            var totIsotopicProfiles = 0;
            foreach (var scanResult in ScanResultList)
            {
                totIsotopicProfiles += scanResult.NumIsotopicProfiles;
            }
            return totIsotopicProfiles;
        }

        public TargetedResultBase CurrentTargetedResult { get; set; }

        public TargetedResultBase GetTargetedResult(TargetBase target)
        {
            if (target == null)
            {
                throw new NullReferenceException("Tried to get the TargetResult, but Target is null");
            }

            if (MassTagResultList.ContainsKey(target))
            {
                CurrentTargetedResult = MassTagResultList[target];
                return CurrentTargetedResult;
            }

            var result = CreateMassTagResult(target);   // this creates the appropriate type and adds it to the MassTagResultList and increments the MSFeatureID number
            CurrentTargetedResult = result;
            return result;
        }

        /// <summary>
        /// Primary method for adding an IsosResult
        /// </summary>
        /// <param name="addedResult"></param>
        public void AddIsosResult(IsosResult addedResult)
        {
            addedResult.MSFeatureID = MSFeatureCounter;
            MSFeatureCounter++;    // by placing it here, we make the MSFeatureID a zero-based ID
            IsosResultBin.Add(addedResult);
        }

        public void ClearAllResults()
        {
            IsosResultBin.Clear();
            MSPeakResultList.Clear();
            ResultList.Clear();
            ScanResultList.Clear();
            MassTagResultList.Clear();
            msPeakResultsGroupedAndMzOrdered.Clear();
        }

        public TargetedResultBase CreateMassTagResult(TargetBase massTag)
        {
            TargetedResultBase result;

            switch (ResultType)
            {
                case Globals.ResultType.BASIC_TARGETED_RESULT:
                    result = new MassTagResult(massTag);
                    break;
                case Globals.ResultType.N14N15_TARGETED_RESULT:
                    result = new N14N15_TResult(massTag);
                    break;
                case Globals.ResultType.O16O18_TARGETED_RESULT:
                    result = new O16O18TargetedResultObject(massTag);
                    break;
                case Globals.ResultType.SIPPER_TARGETED_RESULT:
                    result = new SipperLcmsTargetedResult(massTag);
                    break;
                case Globals.ResultType.TOPDOWN_TARGETED_RESULT:
                    result = new TopDownTargetedResult(massTag);
                    break;
                case Globals.ResultType.DEUTERATED_TARGETED_RESULT:
                    result = new DeuteratedTargetedResultObject(massTag);
                    break;
                default:
                    result = new MassTagResult(massTag);
                    break;
            }

            MassTagResultList.Add(massTag, result);
            result.MSFeatureID = MSFeatureCounter;
            result.Score = 1;
            result.Run = Run;

            MSFeatureCounter++;
            return result;
        }

        //TODO: delete this if sure not used
        //public MassTagResultBase AddMassTagResult(Globals.ResultType ResultType)
        //{
        //    switch (ResultType)
        //    {
        //        case Globals.ResultType.BASIC_TARGETED_RESULT:
        //            return new DeconTools.Backend.Core.MassTagResult();
        //        case Globals.ResultType.N14N15_TARGETED_RESULT:
        //            return new N14N15_TResult();
        //        default:
        //            return new DeconTools.Backend.Core.MassTagResult();
        //    }

        //}

        public List<TargetedResultBase> GetMassTagResults()
        {
            return MassTagResultList.Values.ToList();
        }

        public List<TargetedResultBase> GetSuccessfulMassTagResults()
        {
            //first collect all massTagIDs   (there are more than one massTag having the same ID - because there are multiple charge states for each ID

            var resultList = MassTagResultList.Values.ToList();
            var massTagIDs = new HashSet<int>();
            foreach (var result in resultList)
            {
                massTagIDs.Add(result.Target.ID);
            }
            var filteredResults = new List<TargetedResultBase>(massTagIDs.Count);

            foreach (var mtID in massTagIDs)
            {
                var tempResults = resultList.Where(p => p.Score < 0.15 && p.Target.ID == mtID).ToList();
                if (tempResults.Count > 0)
                {
                    filteredResults.Add(tempResults.OrderByDescending(p => p.Score).First());
                }
            }

            return filteredResults;
        }

        public void FillMSPeakResults()
        {
            if (Run is UIMFRun uimfRun)
            {
                foreach (var item in Run.PeakList)
                {
                    var peak = (MSPeak)item;
                    PeakCounter++;
                    var peakResult = new MSPeakResult(PeakCounter, uimfRun.CurrentScanSet.PrimaryScanNumber,
                                                      uimfRun.CurrentIMSScanSet.PrimaryScanNumber, peak);
                    MSPeakResultList.Add(peakResult);
                }
            }
            else
            {
                foreach (var item in Run.PeakList)
                {
                    var peak = (MSPeak)item;
                    PeakCounter++;
                    var peakResult = new MSPeakResult(PeakCounter, Run.CurrentScanSet.PrimaryScanNumber, peak);
                    MSPeakResultList.Add(peakResult);
                }
            }
        }

        #endregion

        #region Internal Methods
        internal IsosResult CreateIsosResult()
        {
            IsosResult result;

            switch (ResultType)
            {
                case Globals.ResultType.BASIC_TRADITIONAL_RESULT:
                    result = new StandardIsosResult(Run, Run.CurrentScanSet);
                    break;
                case Globals.ResultType.UIMF_TRADITIONAL_RESULT:
                    Check.Require(Run is UIMFRun, "Tried to create an IMS_TRADITIONAL_RESULT but the Dataset is not a UIMF file.");
                    var uimfRun = (UIMFRun)Run;
                    result = new UIMFIsosResult(Run, uimfRun.CurrentScanSet, uimfRun.CurrentIMSScanSet);
                    break;
                case Globals.ResultType.O16O18_TRADITIONAL_RESULT:
                    result = new O16O18IsosResult(Run, Run.CurrentScanSet);
                    break;
                case Globals.ResultType.IMS_TRADITIONAL_RESULT:
                    result = new StandardIsosResult(Run, Run.CurrentScanSet);
                    break;
                case Globals.ResultType.BASIC_TARGETED_RESULT:
                    throw new ApplicationException("ResultType is a Targeted type but currently we are trying to create a Traditional result");
                case Globals.ResultType.O16O18_TARGETED_RESULT:
                    throw new ApplicationException("ResultType is a Targeted type but currently we are trying to create a Traditional result");
                case Globals.ResultType.N14N15_TARGETED_RESULT:
                    throw new ApplicationException("ResultType is a Targeted type but currently we are trying to create a Traditional result");
                case Globals.ResultType.DEUTERATED_TARGETED_RESULT:
                    throw new ApplicationException("ResultType is a Targeted type but currently we are trying to create a Traditional result");
                default:
                    throw new ApplicationException("ResultType is not of a know type: " + ResultType);
            }

            return result;
        }

        internal void AddLog(string logMessage)
        {
            LogMessageList.Add(logMessage);
        }

        internal static List<IsosResult> GetIsosResultsForCurrentScanSet(ResultCollection results)
        {
            Check.Require(results != null, "Can't retrieve IsosResults. Input list is null");

            if (results == null)
                return null;

            var queryList = from n in results.ResultList
                            where n.ScanSet == results.Run.CurrentScanSet
                            select n;

            return queryList.ToList();
        }

        #endregion

    }
}
