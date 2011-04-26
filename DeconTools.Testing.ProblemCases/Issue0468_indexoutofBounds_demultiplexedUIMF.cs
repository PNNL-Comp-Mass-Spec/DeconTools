using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;
using DeconTools.Backend;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class Issue0468_indexoutofBounds_demultiplexedUIMF
    {
        [Test]
        public void accessFrameSetsAndScansetsTest1()
        {

            string uimfFile = @"\\pnl\projects\MSSHARE\MonroeM\Sarc_MS2_UIMF_Test\Sarc_MS2_77_21Feb11_Cheetah_11-01-10_0000.uimf";

            UIMFRun run = new UIMFRun(uimfFile);

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run,0,2, 1, 1);
            fscc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 7, 1);

            sscc.Create();


            MSGeneratorFactory msgenfactory = new MSGeneratorFactory();

            var msgen =   msgenfactory.CreateMSGenerator(run.MSFileType);



            foreach (var frame in run.FrameSetCollection.FrameSetList)
            {
                run.CurrentFrameSet = frame;


                foreach (var scan in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scan;

                    msgen.Execute(run.ResultCollection);

                }

            }




        }

        [Test]
        public void procRunner_test1()
        {

            string uimfFile = @"\\pnl\projects\MSSHARE\MonroeM\Sarc_MS2_UIMF_Test\Sarc_MS2_77_21Feb11_Cheetah_11-01-10_0000.uimf";
            string paramFile = @"\\pnl\projects\MSSHARE\MonroeM\Sarc_MS2_UIMF_Test\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Thrash_2011-02-09.xml";

            OldSchoolProcRunner runner = new OldSchoolProcRunner(uimfFile, Globals.MSFileType.PNNL_UIMF, paramFile);

            runner.Execute();



        }



    }
}
