using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class ResultCollection
    {

        public ResultCollection(Run run)
        {
            this.run = run;
            this.resultList = new List<IsosResult>();
            this.scanResultList = new List<ScanResult>();
            this.currentScanIsosResultBin = new List<IsosResult>();
            this.logMessageList = new List<string>();
        }

        private List<IsosResult> currentScanIsosResultBin;
        public List<IsosResult> CurrentScanIsosResultBin
        {
            get { return currentScanIsosResultBin; }
            set { currentScanIsosResultBin=value;}
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
                UIMFRun uimfRun=(UIMFRun)run;
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

        public void AddIsosResult(IsosResult addedResult)
        {
            this.CurrentScanIsosResultBin.Add(addedResult);     
        }

        public void ClearAllResults()
        {
            this.CurrentScanIsosResultBin.Clear();
            this.ResultList.Clear();
            this.ScanResultList.Clear();
        }
    }
}
