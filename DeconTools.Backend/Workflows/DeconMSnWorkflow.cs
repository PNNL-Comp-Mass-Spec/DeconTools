using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;

namespace DeconTools.Backend.Workflows
{
    public class DeconMSnWorkflow : ScanBasedWorkflow
    {
        private List<IsosResult> _currentMSFeatures = new List<IsosResult>();
        private int _currentMS1Scan;

        private MSGenerator _msGenerator;
        private DeconToolsPeakDetectorV2 _peakDetector;
        private Deconvolutor _deconvolutor;
        TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();
        private BasicTFF _basicFeatureFinder = new BasicTFF();
        private DeconToolsFitScoreCalculator _fitScoreCalculator = new DeconToolsFitScoreCalculator();

        private DeconToolsZeroFiller _zeroFiller = new DeconToolsZeroFiller();


        #region Constructors

        public DeconMSnWorkflow(DeconToolsParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputFolderPath, backgroundWorker)
        {
            _currentMS1Scan = -1;


            _msGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
            _peakDetector = new DeconToolsPeakDetectorV2(
                parameters.PeakDetectorParameters.PeakToBackgroundRatio,
                parameters.PeakDetectorParameters.SignalToNoiseThreshold,
                parameters.PeakDetectorParameters.PeakFitType,
                parameters.PeakDetectorParameters.IsDataThresholded);

            _zeroFiller = new DeconToolsZeroFiller();

            _deconvolutor = DeconvolutorFactory.CreateDeconvolutor(parameters);

            Run.PeakList = new List<Peak>();

        }


        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        protected override void IterateOverScans()
        {
            foreach (var scanSet in Run.ScanSetCollection.ScanSetList)
            {

                //check ms level


                int currentMSLevel = Run.GetMSLevel(scanSet.PrimaryScanNumber);

                if (currentMSLevel == 1)
                {
                    _currentMSFeatures.Clear();

                }
                else if (currentMSLevel == 2)
                {
                    //TODO: create FragmentIonInfo class 
                    int parentScan = Run.GetParentScan(scanSet.PrimaryScanNumber);

                    if (parentScan != _currentMS1Scan)
                    {
                        _currentMS1Scan = parentScan;
                        GetMSPeaksAndMSFeatures();

                    }


                    //string scanInfo = Run.GetFragmentIonInfo(scanSet.PrimaryScanNumber);
                    //GetParentIonDetailsFromScanInfo(scanInfo, out parentMZ, out fragment)




                }
                else
                {
                    throw new System.ApplicationException(
                        "DeconMSn only works on MS1 and MS2 data; You are attempting MS3");
                }





            }

        }





        private void GetMSPeaksAndMSFeatures()
        {
            //TODO: fill this in



        }

        public override void ReportProgress()
        {
            throw new NotImplementedException();
        }
    }
}
