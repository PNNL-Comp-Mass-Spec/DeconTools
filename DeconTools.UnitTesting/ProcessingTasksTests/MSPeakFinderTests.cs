using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class MSPeakFinderTests
    {
        public string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_SQ_24V_0000.Accum_1_recal.imf";
        public string parameterFilename = "..\\..\\TestFiles\\testparam.xml";

        [Test]
        public void FindMSPeaksTest1()
        {
            ParameterLoader loader = new ParameterLoader();
            loader.LoadParametersFromFile(parameterFilename);

            Run run = new IMFRun(imfFilepath);

            
            run.CurrentScanSet = new ScanSet(1, 1, 100000);

            ResultCollection resultcollection = new ResultCollection(run);

            Task msgen = new GenericMSGenerator();
            msgen.Execute(resultcollection);

            DeconToolsPeakDetector peakfinder = new DeconToolsPeakDetector(loader.PeakParameters);
            peakfinder.Execute(resultcollection);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < resultcollection.Run.PeakList.Count; i++)
            {
                MSPeak mspeak=(MSPeak)resultcollection.Run.PeakList[i];


                sb.Append(resultcollection.Run.PeakList[i].XValue);
                sb.Append("\t");
                sb.Append(resultcollection.Run.PeakList[i].Height);
                sb.Append("\t");
                sb.Append(mspeak.SN);
                sb.Append("\t");
                sb.Append(resultcollection.Run.PeakList[i].Width);
                sb.Append("\t");
                sb.Append(Environment.NewLine);
            }
            Console.Write(sb.ToString());

            Assert.AreEqual(2438, resultcollection.Run.PeakList.Count);
            Assert.AreEqual(547.316411323136, Convert.ToDecimal(resultcollection.Run.PeakList[982].XValue));
            Assert.AreEqual(100385, resultcollection.Run.PeakList[982].Height);




        }


    }
}
