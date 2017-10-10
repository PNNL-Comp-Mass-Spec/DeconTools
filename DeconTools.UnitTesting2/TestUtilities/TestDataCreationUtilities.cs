using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2
{
    public class TestDataCreationUtilities
    {

        public static Run CreatePeakDataFromStandardOrbitrapData()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            run.ScanSetCollection .Create(run, 6000, 6050, 1, 1, false);

            Task msgen = new GenericMSGenerator();
            var peakDetector = new DeconToolsPeakDetectorV2 {
                PeaksAreStored = true
            };

            // ReSharper disable once UnusedVariable
            Task decon = new ThrashDeconvolutorV2();

            // ReSharper disable once UnusedVariable
            Task msScanInfoCreator = new ScanResultUpdater();

            // ReSharper disable once UnusedVariable
            Task flagger = new ResultValidatorTask();

            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);
            }

            return run;


        }


        public static Run CreateResultsFromThreeScansOfStandardOrbitrapData()
        {

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);
            Console.WriteLine("Opening " + run.Filename);

            run.ScanSetCollection.Create(run, 6000, 6020, 1, 1, false);

            Task msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            Task peakDetector = new DeconToolsPeakDetectorV2();
            Task msScanInfoCreator = new ScanResultUpdater();
            Task flagger = new ResultValidatorTask();

            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                Console.WriteLine("Deconvoluting scan " + scan);

                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);
                peakDetector.Execute(run.ResultCollection);

                Task decon = new ThrashDeconvolutorV2();
                decon.Execute(run.ResultCollection);

                msScanInfoCreator.Execute(run.ResultCollection);
                flagger.Execute(run.ResultCollection);
            }

            return run;
        }

        public static Run CreateResultsFromTwoFramesOfStandardUIMFData()
        {
            var run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile1)
            {
                ResultCollection = {ResultType = Globals.ResultType.UIMF_TRADITIONAL_RESULT}
            };

            run.ScanSetCollection .Create(run, 500, 501, 3, 1);

            run.IMSScanSetCollection.Create(run, 250, 270, 9, 1);

            Task msgen = new UIMF_MSGenerator();
            Task peakDetector = new DeconToolsPeakDetectorV2();
            Task decon = new ThrashDeconvolutorV2();
            Task msScanInfoCreator = new ScanResultUpdater();
            Task flagger = new ResultValidatorTask();
            Task ticExtractor = new UIMF_TICExtractor();
            Task driftTimeextractor = new UIMFDriftTimeExtractor();

            foreach (var frame in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = frame;

                foreach (var scanSet in run.IMSScanSetCollection.ScanSetList)
                {
                    var scan = (IMSScanSet)scanSet;

                    run.CurrentIMSScanSet = scan;
                    msgen.Execute(run.ResultCollection);
                    peakDetector.Execute(run.ResultCollection);
                    decon.Execute(run.ResultCollection);
                    flagger.Execute(run.ResultCollection);
                    driftTimeextractor.Execute(run.ResultCollection);
                    msScanInfoCreator.Execute(run.ResultCollection);
                    ticExtractor.Execute(run.ResultCollection);
                }

            }

            return run;


        }


        public static List<PeptideTarget> CreateN14N15TestMassTagList()
        {
            var mtList = new List<PeptideTarget>();

            var mt1 = new PeptideTarget
            {
                ID = 23085473,
                NormalizedElutionTime = 0.3807834F,
                MonoIsotopicMass = 2538.33284203802,
                ChargeState = 3
            };
            mt1.MZ = mt1.MonoIsotopicMass / mt1.ChargeState + Globals.PROTON_MASS;
            mt1.Code = "AIHQPAPTFAEQSTTSEILVTGIK";
            mt1.EmpiricalFormula = mt1.GetEmpiricalFormulaFromTargetCode();

            mtList.Add(mt1);


            var mt2 = new PeptideTarget
            {
                ID = 23085470,
                NormalizedElutionTime = 0.6053093f,
                MonoIsotopicMass = 2329.24291507994,
                ChargeState = 3
            };
            mt2.MZ = mt2.MonoIsotopicMass / mt2.ChargeState + Globals.PROTON_MASS;
            mt2.Code = "TAIRDPNPVIFLENEILYGR";
            mt1.EmpiricalFormula = mt1.GetEmpiricalFormulaFromTargetCode();

            mtList.Add(mt2);

            return mtList;

        }


    }
}
