using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class ResultCollection
    {

        public ResultCollection(Run run)
        {
            this.run = run;
            this.resultList = new List<IsosResult>();
            this.massTagResultList = new Dictionary<MassTag, IMassTagResult>();
            this.scanResultList = new List<ScanResult>();
            this.MSPeakResultList = new List<MSPeakResult>();
            this.currentScanIsosResultBin = new List<IsosResult>();
            this.logMessageList = new List<string>();
        }

        private Dictionary<MassTag, IMassTagResult> massTagResultList;
        public Dictionary<MassTag, IMassTagResult> MassTagResultList
        {
            get { return massTagResultList; }
            set { massTagResultList = value; }
        }


  


        private List<MSPeakResult> mSPeakResultList;
        public List<MSPeakResult> MSPeakResultList
        {
            get { return mSPeakResultList; }
            set { mSPeakResultList = value; }
        }

        private List<IsosResult> currentScanIsosResultBin;
        public List<IsosResult> CurrentScanIsosResultBin
        {
            get { return currentScanIsosResultBin; }
            set { currentScanIsosResultBin = value; }
        }


        private List<ScanResult> scanResultList;

        public List<ScanResult> ScanResultList
        {
            get { return scanResultList; }
            set { scanResultList = value; }
        }

        private ScanResult currentScanResult;

        public ScanResult CurrentScanResult
        {
            get { return currentScanResult; }
            set { currentScanResult = value; }
        }


        private List<IsosResult> resultList;

        public List<IsosResult> ResultList
        {
            get { return resultList; }
            set { resultList = value; }
        }

        private ScanSet currentScanSet;

        public ScanSet CurrentScanSet
        {
            get { return currentScanSet; }
            set { currentScanSet = value; }
        }

        private Run run;

        public Run Run
        {
            get { return run; }
            set { run = value; }
        }

        public ScanResult GetCurrentScanResult()
        {
            if (scanResultList == null || scanResultList.Count == 0) return null;
            return this.scanResultList[scanResultList.Count - 1];
        }



        internal IsosResult CreateIsosResult()
        {
            IsosResult result;
            if (this.Run is UIMFRun)
            {
                UIMFRun uimfRun = (UIMFRun)run;
                result = new UIMFIsosResult(this.Run, uimfRun.CurrentFrameSet, uimfRun.CurrentScanSet);
            }
            else
            {
                result = new StandardIsosResult(this.Run, this.Run.CurrentScanSet);
            }
            return result;
        }

        private List<string> logMessageList;

        public List<string> LogMessageList
        {
            get { return logMessageList; }
            set { logMessageList = value; }
        }


        internal void AddLog(string logMessage)
        {
            logMessageList.Add(logMessage);
        }


        internal static List<IsosResult> getIsosResultsForCurrentScanSet(ResultCollection results)
        {
            Check.Require(results != null, "Can't retrieve IsosResults. Input list is null");



            var queryList = from n in results.ResultList
                            where n.ScanSet == results.Run.CurrentScanSet
                            select n;


            return queryList.ToList();
        }

        public int getTotalIsotopicProfiles()
        {
            if (this.ScanResultList == null) return 0;

            int totIsotopicProfiles = 0;
            foreach (ScanResult scanResult in this.ScanResultList)
            {
                totIsotopicProfiles += scanResult.NumIsotopicProfiles;

            }
            return totIsotopicProfiles;
        }

        public int MSFeatureCounter { get; set; }
        

        //This is the primary way to add an IsosResult
        public void AddIsosResult(IsosResult addedResult)
        {
            MSFeatureCounter++;
            addedResult.MSFeatureID = MSFeatureCounter;
            this.CurrentScanIsosResultBin.Add(addedResult);
        }

        public IMassTagResult GetMassTagResult(MassTag massTag)
        {
            if (massTagResultList.ContainsKey(massTag))
            {
                return massTagResultList[massTag];

            }
            else
            {
                return null;
            }


        }

     

        public Globals.MassTagResultType MassTagResultType { get; set; }


   





        public void ClearAllResults()
        {
            this.CurrentScanIsosResultBin.Clear();
            this.MSPeakResultList.Clear();
            this.ResultList.Clear();
            this.ScanResultList.Clear();
        }


        public int PeakCounter { get; set; }

        public void FillMSPeakResults()
        {
            if (this.Run is UIMFRun)
            {
                foreach (MSPeak peak in this.Run.PeakList)
                {
                    PeakCounter++;
                    MSPeakResult peakResult = new MSPeakResult(PeakCounter,((UIMFRun)this.Run).CurrentFrameSet.PrimaryFrame, this.Run.CurrentScanSet.PrimaryScanNumber, peak);
                    this.MSPeakResultList.Add(peakResult);
                }
            }
            else
            {
                foreach (MSPeak peak in this.Run.PeakList)
                {
                    PeakCounter++;
                    MSPeakResult peakResult = new MSPeakResult(PeakCounter, this.Run.CurrentScanSet.PrimaryScanNumber, peak);
                    this.MSPeakResultList.Add(peakResult);
                }
            }

        }

        public IMassTagResult CreateMassTagResult(MassTag massTag)
        {
            IMassTagResult result;

            switch (MassTagResultType)
            {
                case Globals.MassTagResultType.BASIC_MASSTAG_RESULT:
                    result = new MassTagResult(massTag);
                    break;
                case Globals.MassTagResultType.N14N15_MASSTAG_RESULT:
                    result = new N14N15_TResult();
                    break;
                default:
                    result = new MassTagResult();
                    break;
            }

            this.MassTagResultList.Add(massTag, result);
            return result;
        }
        
        public IMassTagResult AddMassTagResult(Globals.MassTagResultType massTagResultType)
        {
            IMassTagResult result;

            switch (massTagResultType)
            {
                case Globals.MassTagResultType.BASIC_MASSTAG_RESULT:
                    return new DeconTools.Backend.Core.MassTagResult();
                case Globals.MassTagResultType.N14N15_MASSTAG_RESULT:
                    return new N14N15_TResult();
                default:
                    return new DeconTools.Backend.Core.MassTagResult();
            }


        }
    }
}
