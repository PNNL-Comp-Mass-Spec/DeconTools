using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend;
using DeconTools.UnitTesting2;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.ProcessingTasks.ResultValidators;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class InferferenceScorePeakBase_issue
    {

        /// <summary>
        /// afternote:
        /// I thought there was a problem but in the end there weren't anyproblems. PeakBased-i_score has a lot of 0 values. 
        /// </summary>

        [Test]
        public void interferenceScoreIsNotRight_uimf_test1()
        {
            string testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000_frame1200_scan142.txt";
            
            string paramFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans4_SumFrames3_noFit_Thrash_2009-11-02.xml";

            OldSchoolProcRunner runner = new OldSchoolProcRunner(testFile, Globals.MSFileType.Ascii, paramFile);
            runner.Execute();

        }


        [Test]
        public void interferenceScoreIsNotRight_uimf_test2()
        {
            string testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";

            string paramFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans4_SumFrames3_Frame1200.xml";

            OldSchoolProcRunner runner = new OldSchoolProcRunner(testFile, Globals.MSFileType.PNNL_UIMF, paramFile);
            runner.Execute();

        }


        [Test]
        public void test3()
        {
            string testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";


            UIMFRun run = new UIMFRun(testFile);


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            MSGenerator msgen= msgenFactory.CreateMSGenerator(run.MSFileType);
            msgen.MinMZ = 0;
            msgen.MaxMZ = 50000;

            FrameSet frame = new FrameSet(1200, new int[] { 1199, 1200, 1201 });
            ScanSet scanset = new ScanSet(142, new int[] { 138, 139,140, 141, 142, 143, 144, 145, 146 });

            //ScanSet scanset = new ScanSet(143, new int[] { 138, 139, 141, 142, 143, 144, 145,146,147 });
            run.CurrentFrameSet = frame;
            run.CurrentScanSet = scanset;


            DeconToolsPeakDetector peakdet = new DeconToolsPeakDetector(4, 3, Globals.PeakFitType.QUADRATIC, false);
            DeconToolsZeroFiller zerofiller = new DeconToolsZeroFiller(3);
            HornDeconvolutor horn = new HornDeconvolutor();
            horn.DeleteIntensityThreshold = 10;
            horn.IsMZRangeUsed = false;
            horn.LeftFitStringencyFactor = 2.5;
            horn.MaxFitAllowed = 0.4;
            horn.MinIntensityForScore = 10;
            horn.MinPeptideBackgroundRatio = 4;
            horn.RightFitStringencyFactor = 0.5;
            


            ResultValidatorTask validator = new ResultValidatorTask();

            msgen.Execute(run.ResultCollection);
            zerofiller.Execute(run.ResultCollection);
            peakdet.Execute(run.ResultCollection);
            horn.Execute(run.ResultCollection);

            validator.Execute(run.ResultCollection);

            //TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

            
        }






    }
}
