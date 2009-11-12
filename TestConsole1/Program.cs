using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;

namespace TestConsole1
{
    class Program
    {

        static void Main(string[] args)
        {
            string uimfFilepath = "..\\..\\..\\DeconTools.UnitTesting\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.uimf";
            string uimfIsosExporterTest1output = "..\\..\\..\\DeconTools.UnitTesting\\Testfiles\\UIMFIsosExporterTest1Output.csv";


            Project project = Project.getInstance();
            //TODO:  add project parameters  (for summing)

            project.RunCollection.Add(new UIMFRun(uimfFilepath, 1, 2399));

            Task msGen = new UIMF_MSGenerator(200, 2000);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;
            Task peakDetector = new DeconToolsPeakDetector(detectorParams);

            Task rapidDecon = new RapidDeconvolutor();

            Task driftTimeExtractor = new UIMFDriftTimeExtractor();

            project.TaskCollection.TaskList.Add(msGen);
            project.TaskCollection.TaskList.Add(peakDetector);
            project.TaskCollection.TaskList.Add(rapidDecon);
            project.TaskCollection.TaskList.Add(driftTimeExtractor);

            TaskController controller = new UIMF_TaskController(project.TaskCollection);
            controller.Execute(project.RunCollection);

            UIMFIsosExporter isosExporter = new UIMFIsosExporter(uimfIsosExporterTest1output);
            isosExporter.Export(project.RunCollection[0].ResultCollection);

            Console.ReadLine();

        }
    }
}
