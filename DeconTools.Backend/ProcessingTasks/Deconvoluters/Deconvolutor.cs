using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks
{
    public abstract class Deconvolutor : Task
    {
        public enum DeconResultComboMode
        {
            simplyAddIt,
            addItIfUnique,
            addAndReplaceIfOneDaltonErrorDetected
        }

        public abstract void Deconvolute(ResultCollection resultList);

        public override void Execute(ResultCollection resultList)
        {
            clearCurrentScanIsosResultBin(resultList);   //TODO:   this does not clear

            ShowTraceMessageIfEnabled("Deconvolute: " + resultList);

            Deconvolute(resultList);

            ShowTraceMessageIfEnabled("GatherMSFeatureStatistics: " + resultList.Run);
            GatherMSFeatureStatistics(resultList.Run);

            ShowTraceMessageIfEnabled("associatePeaksToMSFeatureID: " + resultList);
            associatePeaksToMSFeatureID(resultList);

            ShowTraceMessageIfEnabled("addCurrentScanIsosResultsToOverallList: " + resultList);
            addCurrentScanIsosResultsToOverallList(resultList);
        }

        protected virtual void GatherMSFeatureStatistics(Run run)
        {
            if (run.ResultCollection.IsosResultBin == null || run.ResultCollection.IsosResultBin.Count == 0) return;

            ScanSet currentScanset;

            if (run is UIMFRun uimfRun)
            {
                currentScanset = uimfRun.CurrentIMSScanSet;
            }
            else
            {
                currentScanset = run.CurrentScanSet;
            }

            Check.Require(currentScanset != null, "the CurrentScanSet for the Run is null. This needs to be set.");
            if (currentScanset == null)
                return;

            currentScanset.NumIsotopicProfiles = run.ResultCollection.IsosResultBin.Count;    //used in ScanResult
        }

        private void associatePeaksToMSFeatureID(ResultCollection resultList)
        {
            if (resultList.IsosResultBin == null || resultList.IsosResultBin.Count == 0) return;

            foreach (var msFeature in resultList.IsosResultBin)
            {
                foreach (var peak in msFeature.IsotopicProfile.Peaklist)
                {
                    peak.MSFeatureID = msFeature.MSFeatureID;
                }
            }
        }

        private void clearCurrentScanIsosResultBin(ResultCollection resultList)
        {
            //remove the result if it was a result of a different scan. Otherwise keep it
            //this allows running of two back-to-back deconvolutors without clearing the results
            //between deconvolutions.   Going backwards through the list prevents exceptions.
            if (resultList.IsosResultBin == null || resultList.IsosResultBin.Count == 0) return;

            if (resultList.Run is UIMFRun)
            {
                resultList.IsosResultBin.Clear();
            }
            else
            {
                for (var i = resultList.IsosResultBin.Count - 1; i >= 0; i--)
                {
                    if (resultList.IsosResultBin[i].ScanSet.PrimaryScanNumber != resultList.Run.CurrentScanSet.PrimaryScanNumber)
                    {
                        resultList.IsosResultBin.RemoveAt(i);
                    }
                }
            }
        }

        private void addCurrentScanIsosResultsToOverallList(ResultCollection resultList)
        {
            resultList.ResultList.AddRange(resultList.IsosResultBin);
        }

        protected void AddDeconResult(ResultCollection baseResultList, IsosResult addedResult, DeconResultComboMode comboMode = DeconResultComboMode.simplyAddIt)
        {
            Check.Require(baseResultList != null, "Deconvolutor problem. Can't combine results. Base resultList is null.");
            if (baseResultList == null)
                return;

            Check.Require(addedResult != null, "Deconvolutor problem. Can't combine results. Added IsosResult is null.");

            switch (comboMode)
            {
                case DeconResultComboMode.simplyAddIt:
                    baseResultList.AddIsosResult(addedResult);
                    break;
                case DeconResultComboMode.addItIfUnique:

                    //retrieve IsosResults for CurrentScanSet
                    //TODO: next line might be a time bottleneck! needs checking
                    //List<IsosResult> scanSetIsosResults = ResultCollection.GetIsosResultsForCurrentScanSet(baseResultList);

                    //search isosResults for a (monoPeak = addedResult's monoPeak) AND chargeState = addedResult's chargeState
                    if (doesResultExist(baseResultList.IsosResultBin, addedResult))
                    {
                        //do nothing...  isotopic profile already exists
                    }
                    else
                    {
                        baseResultList.AddIsosResult(addedResult);
                    }
                    break;
                case DeconResultComboMode.addAndReplaceIfOneDaltonErrorDetected:
                    throw new NotImplementedException("add and replace isotopic profile mode not yet supported");
            }
        }

        private bool doesResultExist(IEnumerable<IsosResult> scanSetIsosResults, IsosResult addedResult)
        {
            foreach (var result in scanSetIsosResults)
            {
                var addedMonoPeak = addedResult.IsotopicProfile.Peaklist[0];
                var baseMonoPeak = result.IsotopicProfile.Peaklist[0];

                if (addedResult.IsotopicProfile.ChargeState == result.IsotopicProfile.ChargeState
                    && Math.Abs(addedMonoPeak.XValue - baseMonoPeak.XValue) < float.Epsilon)
                {
                    return true;   //found a match
                }
            }
            //didn't find a matching monoisotopic peak
            return false;
        }
    }
}
