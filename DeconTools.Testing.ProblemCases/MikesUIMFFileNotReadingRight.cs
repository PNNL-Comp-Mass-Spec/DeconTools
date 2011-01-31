using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class MikesUIMFFileNotReadingRight
    {
        [Test]
        public void Test1()
        {
            string testFile = @"\\proto-5\IMS_DEV\Belov\QC_Shew_IMS4_QTOF3_45min_run3_4bit_0000\QC_Shew_IMS4_QTOF3_45min_run3_4bit_0000.uimf";

            UIMFRun run = new UIMFRun(testFile);


            Console.WriteLine("numframe = " + run.GetNumFrames());


            Console.WriteLine("minScan = "+run.MinScan);
            Console.WriteLine("maxScan = " + run.MaxScan);



        }

    }
}
