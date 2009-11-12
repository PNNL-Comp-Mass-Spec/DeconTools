using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.DataProcessingConfiguration;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.UI.Testing
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DeconToolsV2.Peaks.clsPeakProcessorParameters parameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            parameters.ThresholdedData = true;
            Task peakDetector = new DeconToolsPeakDetector(parameters);


            Task msgen = new GenericMSGenerator(300, 1500);

            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new PeakDetectorConfig(peakDetector));
            Application.Run(new MSGeneratorConfig(msgen));

            
          
        }


        private static void test1()
        {
            DeconToolsV2.Peaks.clsPeakProcessorParameters parameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            Task peakDetector = new DeconToolsPeakDetector(parameters);
            DataProcessingConfiguration.PeakDetectorConfig frm = new DeconTools.DataProcessingConfiguration.PeakDetectorConfig(peakDetector);
            frm.Show();

        }

    }
}
