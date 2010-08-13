using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks
{
    [TestFixture]
    public class DemoTests
    {
        string xcaliburTestFile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void createRunAndReadMSSpectraTest1()
        {
            //Create the run
            DeconTools.Backend.Runs.RunFactory runFactory = new RunFactory();
            DeconTools.Backend.Core.Run run = runFactory.CreateRun(xcaliburTestFile1);


            //Create the task
            MSGeneratorFactory msfactory = new MSGeneratorFactory();
            Task msgen = msfactory.CreateMSGenerator(run.MSFileType);


            //Create the target scan list (MS1 scans from 6005 - 6020). 
            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6005, 6020, 1, 1);
            sscc.Create();


            //iterate over each scan in the target collection get the mass spectrum 
            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                Console.WriteLine("Working on Scan " + scan.PrimaryScanNumber);
                run.CurrentScanSet = scan;
                msgen.Execute(run.ResultCollection);

                Console.WriteLine("XYPair count = " + run.XYData.Xvalues.Length); 
            }

            
            


        }


        [Test]
        public void getMSMSDataTest1()
        {
            //Create the run
            DeconTools.Backend.Runs.RunFactory runFactory = new RunFactory();
            DeconTools.Backend.Core.Run run = runFactory.CreateRun(xcaliburTestFile1);


            //Create the task
            MSGeneratorFactory msfactory = new MSGeneratorFactory();
            Task msgen = msfactory.CreateMSGenerator(run.MSFileType);


            int startScan = 6000;
            int stopScan = 7000;


            for (int i = startScan; i < stopScan; i++)
            {
                if (run.GetMSLevel(i)==2)
                {
                    ScanSet scanset = new ScanSet(i);
                    run.CurrentScanSet = scanset;
                    msgen.Execute(run.ResultCollection);

                    Console.WriteLine("Working on Scan " + scanset.PrimaryScanNumber);
                    Console.WriteLine("XYPair count = " + run.XYData.Xvalues.Length); 


                }


                
            }




        }



     
    }
}
