using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using NUnit.Framework;
using UIMFLibrary;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMCS_0135_UIMF_MS_indexOutOfRange
    {
        [Test]
        public void Test1()
        {
            string uimfFile = @"D:\Data\UIMF\Problem_datasets\LSDF2_10-0457-03_A_26May11_Roc_11-02-26.uimf";

            var run = (UIMFRun)new RunFactory().CreateRun(uimfFile);

            int startFrame = 163;
            int stopFrame = 165;

            run.ScanSetCollection= ScanSetCollection.Create(run, run.MinLCScan, run.MaxLCScan, 7, 1);

            run.FrameSetCollection = FrameSetCollection.Create(run, startFrame, stopFrame, 1, 1);

            var msgen= MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            foreach (var frameSet in run.FrameSetCollection.FrameSetList)
            {
                Console.WriteLine("-------------------- Frame = "+ frameSet);


                run.CurrentFrameSet = frameSet;
                foreach (var scanSet in run.ScanSetCollection.ScanSetList)
                {
                    Console.WriteLine("Scan = "+ scanSet);

                    run.CurrentScanSet = scanSet;

                    msgen.Execute(run.ResultCollection);


                }


            }

        }


        [Test]
        public void GetFrameDetails1()
        {
            string uimfFile = @"D:\Data\UIMF\Problem_datasets\LSDF2_10-0457-03_A_26May11_Roc_11-02-26.uimf";

            var run = (UIMFRun)new RunFactory().CreateRun(uimfFile);

            int startFrame = 163;
            int stopFrame = 165;
            
            
            UIMFRunTester tester=new UIMFRunTester();

            tester.DisplayFrameParameters(run,startFrame,stopFrame);

        }


        [Test]
        public void UseUIMFReader1()
        {
            string uimfFile = @"D:\Data\UIMF\Problem_datasets\LSDF2_10-0457-03_A_26May11_Roc_11-02-26.uimf";
            using (UIMFLibrary.DataReader reader=new DataReader(uimfFile))
            {

                int frameStart = 164;
                int frameStop = 164;
                int scanStart = 5;
                int scanStop = 5;

                double[] mzArray;
                int[] intensityArray;

                reader.GetSpectrum(frameStart, frameStop, DataReader.FrameType.MS1, scanStart, scanStop, out mzArray,
                                   out intensityArray);
            }


        }

    }
}
