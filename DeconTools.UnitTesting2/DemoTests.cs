using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting2
{
    [TestFixture]
    public class DemoTests
    {
        [Test]
        public void createRunAndReadMSSpectraTest1()
        {
            //Create the run
            DeconTools.Backend.Data.RunFactory runFactory = new RunFactory();
            DeconTools.Backend.Core.Run run = runFactory.CreateRun(FileRefs.OrbitrapStdFile1);


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
            DeconTools.Backend.Data.RunFactory runFactory = new RunFactory();
            DeconTools.Backend.Core.Run run = runFactory.CreateRun(FileRefs.OrbitrapStdFile1);


            //Create the task
            MSGeneratorFactory msfactory = new MSGeneratorFactory();
            Task msgen = msfactory.CreateMSGenerator(run.MSFileType);


            int startScan = 6000;
            int stopScan = 7000;


            for (int i = startScan; i < stopScan; i++)
            {
                if (run.GetMSLevel(i) == 2)
                {
                    ScanSet scanset = new ScanSet(i);
                    run.CurrentScanSet = scanset;
                    msgen.Execute(run.ResultCollection);

                    Console.Write("Working on Scan " + scanset.PrimaryScanNumber);
                    Console.WriteLine("; XYPair count = " + run.XYData.Xvalues.Length);


                }



            }




        }



        [Test]
        public void tempGetMSMSDataTest2()
        {
            DeconToolsV2.Readers.clsRawData run = new DeconToolsV2.Readers.clsRawData(FileRefs.OrbitrapStdFile1, DeconToolsV2.Readers.FileType.FINNIGAN);

            int startScan = 6000;
            int stopScan = 7000;

            for (int i = startScan; i < stopScan; i++)
            {
                string scanInfo = run.GetScanDescription(i);
                int scanLevel = run.GetMSLevel(i);

                Console.Write("Working on Scan " + i);
                Console.WriteLine("; Scan info = " + scanInfo);

            }


            
        }

    }
}
