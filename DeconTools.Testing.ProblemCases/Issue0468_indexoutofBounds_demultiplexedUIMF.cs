using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class Issue0468_indexoutofBounds_demultiplexedUIMF
    {
        [Test]
        public void Test1()
        {

            string uimfFile = @"\\protoapps\userdata\Matt\IMS\DeconTools\Sarc_P01_F11_0071_16Apr11_Cheetah_11-02-19_inverse.uimf";

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

    }
}
