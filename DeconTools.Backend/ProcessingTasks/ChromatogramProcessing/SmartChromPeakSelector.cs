using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class SmartChromPeakSelector : SmartChromPeakSelectorBase
    {
      

       

        

        #region Constructors
        public SmartChromPeakSelector(SmartChromPeakSelectorParameters parameters)
        {
            this.Parameters = parameters;

            MSPeakDetector = new DeconToolsPeakDetector(parameters.MSPeakDetectorPeakBR, parameters.MSPeakDetectorSigNoiseThresh, Globals.PeakFitType.QUADRATIC, true);

            IterativeTFFParameters iterativeTFFParams = new IterativeTFFParameters();
            iterativeTFFParams.ToleranceInPPM = parameters.MSToleranceInPPM;

            if (parameters.MSFeatureFinderType == Globals.TargetedFeatureFinderType.BASIC)
            {
                TargetedMSFeatureFinder = new TargetedFeatureFinders.BasicTFF(parameters.MSToleranceInPPM);
            }
            else
            {
                TargetedMSFeatureFinder = new IterativeTFF(iterativeTFFParams);
            }
            
            

            resultValidator = new ResultValidators.ResultValidatorTask();
            fitScoreCalc = new MassTagFitScoreCalculator();

           

        }

        #endregion



        
    }
}
