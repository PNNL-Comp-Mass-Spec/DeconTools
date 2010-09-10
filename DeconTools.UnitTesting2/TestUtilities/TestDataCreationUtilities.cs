using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend;

namespace DeconTools.UnitTesting2
{
    public class TestDataCreationUtilities
    {

        public static Run CreatePeakDataFromStandardOrbitrapData()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6050, 1, 1, false);
            sscc.Create();

            Task msgen = new GenericMSGenerator();
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector();
            peakDetector.StorePeakData = true;

            Task decon = new HornDeconvolutor();
            Task msScanInfoCreator = new ScanResultUpdater();
            Task flagger = new ResultValidatorTask();

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
            }

            return run;


        }


        public static Run CreateResultsFromThreeScansOfStandardOrbitrapData()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);


            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6020, 1, 1, false);
            sscc.Create();

            Task msgen = new GenericMSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            Task msScanInfoCreator = new ScanResultUpdater();
            Task flagger = new ResultValidatorTask();

            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);

                decon.Execute(run.ResultCollection);
                msScanInfoCreator.Execute(run.ResultCollection);
                flagger.Execute(run.ResultCollection);
            }

            return run;
        }

        public static Run CreateResultsFromTwoFramesOfStandardUIMFData()
        {
            UIMFRun run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile1);

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, 500, 501, 3, 1);
            fscc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 250, 270, 9, 1);
            sscc.Create();

            Task msgen = new UIMF_MSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new RapidDeconvolutor();
            Task refitter = new DeconToolsFitScoreCalculator();
            Task msScanInfoCreator = new ScanResultUpdater();
            Task flagger = new ResultValidatorTask();
            Task ticExtractor = new DeconTools.Backend.ProcessingTasks.UIMF_TICExtractor();
            Task driftTimeextractor = new UIMFDriftTimeExtractor();


            foreach (var frame in run.FrameSetCollection.FrameSetList)
            {
                run.CurrentFrameSet = frame;

                foreach (var scan in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scan;
                    msgen.Execute(run.ResultCollection);
                    peakDetector.Execute(run.ResultCollection);
                    decon.Execute(run.ResultCollection);
                    refitter.Execute(run.ResultCollection);
                    flagger.Execute(run.ResultCollection);
                    driftTimeextractor.Execute(run.ResultCollection);
                    msScanInfoCreator.Execute(run.ResultCollection);
                    ticExtractor.Execute(run.ResultCollection);
                }
                
            }

            return run;


        }


        public static List<MassTag> CreateN14N15TestMassTagList()
        {
            List<MassTag> mtList = new List<MassTag>();

            MassTag mt1 = new MassTag();
            mt1.ID = 23085473;
            mt1.NETVal = 0.3807834F;
            mt1.MonoIsotopicMass = 2538.33284203802;
            mt1.ChargeState = 3;
            mt1.MZ = mt1.MonoIsotopicMass / mt1.ChargeState + Globals.PROTON_MASS;
            mt1.PeptideSequence = "AIHQPAPTFAEQSTTSEILVTGIK";
            mt1.CreatePeptideObject();

            mtList.Add(mt1);


            MassTag mt2 = new MassTag();
            mt2.ID = 23085470;
            mt2.NETVal = 0.6053093f;
            mt2.MonoIsotopicMass = 2329.24291507994;
            mt2.ChargeState = 3;
            mt2.MZ = mt2.MonoIsotopicMass / mt2.ChargeState + Globals.PROTON_MASS;
            mt2.PeptideSequence = "TAIRDPNPVIFLENEILYGR";
            mt2.CreatePeptideObject();

            mtList.Add(mt2);

            return mtList;

        }


    }
}
