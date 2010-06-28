using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.TaskControllers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using NUnit.Framework;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks.NETAlignment;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    public class N14N15_CustomWorkflow1Tests
    {
        string rsph_AOnly_28_run1File = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_28_run1_26Oct07_Andromeda_07-09-02\acqus";
        string peaks_rsph_AOnly_28_run1File_Scans1000_1500 = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_28_run1_26Oct07_Andromeda_07-09-02\RSPH_Aonly_28_run1_26Oct07_Andromeda_07-09-02_scans1000-1500_peaks.txt";


        [Test]
        public void test1()
        {
            Run run = new BrukerRun(rsph_AOnly_28_run1File);
            List<MassTag> massTags = TestUtilities.CreateN14N15TestMassTagList();

            getAlignmentInfo(run);
            
            getTestMSPeaklistForChromagramGen(run);

            N14N15_CustomWorkflow1 workflow = new N14N15_CustomWorkflow1(run, massTags[0]);
            N14N15ResultObject n14n15Result =  workflow.GetN14N15Result();




        }

        private void getAlignmentInfo(Run run)
        {
            ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
            chromAligner.Execute(run);

        }

        private void getTestMSPeaklistForChromagramGen(Run run)
        {
            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(peaks_rsph_AOnly_28_run1File_Scans1000_1500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);
        }


        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
