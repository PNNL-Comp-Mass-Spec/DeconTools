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
            this.ResultList = new List<IsosResult>();
            this.massTagResultList = new Dictionary<MassTag, MassTagResultBase>();
            this.scanResultList = new List<ScanResult>();
            this.MSPeakResultList = new List<MSPeakResult>();
            this.m_IsosResultBin = new List<IsosResult>();
            this.logMessageList = new List<string>();
        }

        private Dictionary<MassTag, MassTagResultBase> massTagResultList;
        public Dictionary<MassTag, MassTagResultBase> MassTagResultList
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

        private IList<IsosResult> m_IsosResultBin;
        public IList<IsosResult> IsosResultBin
        {
            get { return m_IsosResultBin; }
            set { m_IsosResultBin = value; }
        }


        private List<ScanResult> scanResultList;

        public List<ScanResult> ScanResultList
        {
            get { return scanResultList; }
            set { scanResultList = value; }
        }

        public List<IsosResult> ResultList {get; set;}
        

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
            addedResult.MSFeatureID = MSFeatureCounter;
            MSFeatureCounter++;    // by placing it here, we make the MSFeatureID a zero-based ID, as Kevin requested in an email (Jan 20/2010)
            this.IsosResultBin.Add(addedResult);
        }

        public MassTagResultBase GetMassTagResult(MassTag massTag)
        {
            if (massTagResultList.ContainsKey(massTag))
            {
                return massTagResultList[massTag];
            }
            else
            {
                MassTagResultBase result = CreateMassTagResult(massTag);   // this creates the appropriate type and adds it to the MassTagResultList and increments the MSFeatureID number
                return result;  
            }
        }

        public Globals.MassTagResultType MassTagResultType { get; set; }

        public void ClearAllResults()
        {
            this.IsosResultBin.Clear();
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
                    MSPeakResult peakResult = new MSPeakResult(PeakCounter, ((UIMFRun)this.Run).CurrentFrameSet.PrimaryFrame, this.Run.CurrentScanSet.PrimaryScanNumber, peak);
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

        public MassTagResultBase CreateMassTagResult(MassTag massTag)
        {
            MassTagResultBase result;

            switch (MassTagResultType)
            {
                case Globals.MassTagResultType.BASIC_MASSTAG_RESULT:
                    result = new MassTagResult(massTag);
                    break;
                case Globals.MassTagResultType.N14N15_MASSTAG_RESULT:
                    result = new N14N15_TResult(massTag);
                    break;
                default:
                    result = new MassTagResult();
                    break;
            }

            this.MassTagResultList.Add(massTag, result);
            result.MSFeatureID = MSFeatureCounter;
            result.Score = 1;
            this.MSFeatureCounter++;
            return result;
        }

        public MassTagResultBase AddMassTagResult(Globals.MassTagResultType massTagResultType)
        {
            MassTagResultBase result;

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

        public List<MassTagResultBase> GetSuccessfulMassTagResults()
        {
            List<MassTagResultBase> filteredResults = new List<MassTagResultBase>();

            HashSet<int> massTagIDs = new HashSet<int>();

            //first collect all massTagIDs   (there are more than one massTag having the same ID - because there are multiple charge states for each ID

            List<MassTagResultBase> resultList = this.MassTagResultList.Values.ToList();
            for (int i = 0; i < resultList.Count; i++)
            {
                massTagIDs.Add(resultList[i].MassTag.ID);
            }

            foreach (var mtID in massTagIDs)
            {
                List<MassTagResultBase> tempResults = resultList.Where(p => p.Score < 0.15 && p.MassTag.ID == mtID).ToList();
                if (tempResults.Count > 0)
                {
                    filteredResults.Add(tempResults.OrderByDescending(p => p.Score).First());
                }
            }

            return filteredResults;




        }
    }
}
