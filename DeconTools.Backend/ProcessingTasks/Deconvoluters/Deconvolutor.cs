using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
            
            Deconvolute(resultList);

            associatePeaksToMSFeatureID(resultList);

            addCurrentScanIsosResultsToOverallList(resultList);

            

        }

        private void associatePeaksToMSFeatureID(ResultCollection resultList)
        {
            if (resultList.IsosResultBin == null || resultList.IsosResultBin.Count == 0) return;

            foreach (var msfeature in resultList.IsosResultBin)
            {
                foreach (MSPeak peak in msfeature.IsotopicProfile.Peaklist)
                {
                    peak.MSFeatureID = msfeature.MSFeatureID;
                    
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
                for (int i = resultList.IsosResultBin.Count - 1; i >= 0; i--)
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

        protected void AddDeconResult(ResultCollection baseResultList, IsosResult addedResult, DeconResultComboMode comboMode= DeconResultComboMode.simplyAddIt)
        {
            Check.Require(baseResultList != null, "Deconvolutor problem. Can't combine results. Base resultList is null.");
            Check.Require(addedResult != null, "Deconvolutor problem. Can't combine results. Added IsosResult is null.");

            switch (comboMode)
            {
                case DeconResultComboMode.simplyAddIt:
                    baseResultList.AddIsosResult(addedResult);
                    break;
                case DeconResultComboMode.addItIfUnique:

                    //retrieve IsosResults for CurrentScanSet
                    //TODO: next line might be a time bottleneck! needs checking
                    //List<IsosResult> scanSetIsosResults = ResultCollection.getIsosResultsForCurrentScanSet(baseResultList);

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
                default:
                    break;
            }


        }

        private bool doesResultExist(IList<IsosResult> scanSetIsosResults, IsosResult addedResult)
        {
            MSPeak addedMonoPeak;
            MSPeak baseMonoPeak;

            foreach (IsosResult result in scanSetIsosResults)
            {
                addedMonoPeak = addedResult.IsotopicProfile.Peaklist[0];
                baseMonoPeak = result.IsotopicProfile.Peaklist[0];


                if (addedResult.IsotopicProfile.ChargeState == result.IsotopicProfile.ChargeState
                    && addedMonoPeak.XValue == baseMonoPeak.XValue)
                {
                    return true;   //found a match
                }
            }
            //didn't find a matching monoisotopic peak
            return false;
            
        }


    }
}
