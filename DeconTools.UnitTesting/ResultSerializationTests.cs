using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;
using System.IO;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class ResultSerializationTests
    {
        private string xcaliburTestfile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";


        public string uimfFilepath2 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";




        private string serializedResultFilename1 = "..\\..\\TestFiles\\serializerTest1.bin";

        private string serializedAppendedResults = "..\\..\\TestFiles\\serializerTest1.bin";

        private string deserializerTestFile1 = "..\\..\\TestFiles\\deserializerTest1.bin";
        private string deserializerUIMFTestFile1 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf.bin";


        //generate results
        //serialize results after X results

        [Test]
        public void serializerWriteAllResultsTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile1, 4000, 5000);

            const int RESULTTHRESHOLD = 10000000;
            if (File.Exists(serializedResultFilename1)) File.Delete(serializedResultFilename1);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1, false);
            scansetCreator.Create();

            Task msgen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = true;

            Task peakdetector = new DeconToolsPeakDetector(detectorParams);

            Task decon = new RapidDeconvolutor();

            TaskCollection taskcollection = new TaskCollection();
            taskcollection.TaskList.Add(msgen);
            taskcollection.TaskList.Add(peakdetector);
            taskcollection.TaskList.Add(decon);

            IsosResultSerializer serializer = new IsosResultSerializer(serializedResultFilename1, System.IO.FileMode.Append, true);

            for (int i = 0; i < run.ScanSetCollection.ScanSetList.Count; i++)
            {
                run.CurrentScanSet = run.ScanSetCollection.ScanSetList[i];

                foreach (Task task in taskcollection.TaskList)
                {
                    task.Execute(run.ResultCollection);
                }


                //if (run.ResultCollection.ResultList.Count > RESULTTHRESHOLD)
                //{
                //    serializer.Serialize();

                //}


            }
            serializer.Serialize(run.ResultCollection);
            serializer.Close();
            Console.WriteLine(run.ResultCollection.ResultList.Count);

            Assert.AreEqual(true, File.Exists(serializedResultFilename1));

            FileInfo fi = new FileInfo(serializedResultFilename1);
            Assert.AreEqual(2355525, fi.Length);
            Assert.AreEqual(11147, run.ResultCollection.ResultList.Count);



        }

        [Test]
        public void serializerAppendResultsTest1()
        {
            Run run = new XCaliburRun(xcaliburTestfile1, 4000, 5000);

            const int RESULTTHRESHOLD = 10000;
            if (File.Exists(serializedAppendedResults)) File.Delete(serializedAppendedResults);

            ScanSetCollectionCreator scansetCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 1, 1);
            scansetCreator.Create();

            Task msgen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = true;

            Task peakdetector = new DeconToolsPeakDetector(detectorParams);

            Task decon = new RapidDeconvolutor();

            TaskCollection taskcollection = new TaskCollection();
            taskcollection.TaskList.Add(msgen);
            taskcollection.TaskList.Add(peakdetector);
            taskcollection.TaskList.Add(decon);

            IsosResultSerializer serializer = new IsosResultSerializer(serializedAppendedResults, System.IO.FileMode.Append, true);

            for (int i = 0; i < run.ScanSetCollection.ScanSetList.Count; i++)
            {
                run.CurrentScanSet = run.ScanSetCollection.ScanSetList[i];

                foreach (Task task in taskcollection.TaskList)
                {
                    task.Execute(run.ResultCollection);
                }


                if (run.ResultCollection.ResultList.Count > RESULTTHRESHOLD)
                {
                    serializer.Serialize(run.ResultCollection);
                    run.ResultCollection.ResultList.Clear();
                }


            }
            serializer.Serialize(run.ResultCollection);
            serializer.Close();
            Console.WriteLine(run.ResultCollection.ResultList.Count);

            Assert.AreEqual(true, File.Exists(serializedAppendedResults));

            FileInfo fi = new FileInfo(serializedAppendedResults);
            Assert.AreEqual(2489233, fi.Length);
            Assert.AreEqual(1130, run.ResultCollection.ResultList.Count);


        }

        [Test]
        public void serializeUIMFDataTest1()
        {
            Run run = new UIMFRun(uimfFilepath2);
            List<Run> runCollection = new List<Run>();
            runCollection.Add(run);

            int startFrame = 1200;
            int stopFrame = 1205;

            int numFramesSummed = 3;
            int numScansSummed = 3;


            FrameSetCollectionCreator framesetCreator = new FrameSetCollectionCreator(run, startFrame, stopFrame, numFramesSummed, 1);
            framesetCreator.Create();

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run, numScansSummed, 1);
            scanSetCreator.Create();

            Task msgen = new UIMF_MSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 3;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = true;

            Task peakdetector = new DeconToolsPeakDetector(detectorParams);

            Task decon = new RapidDeconvolutor();

            TaskCollection taskcollection = new TaskCollection();
            taskcollection.TaskList.Add(msgen);
            taskcollection.TaskList.Add(peakdetector);
            taskcollection.TaskList.Add(decon);


            TaskController taskcontroller = new UIMF_TaskController(taskcollection);
            taskcontroller.IsosResultThresholdNum = 50;
            taskcontroller.Execute(runCollection);


            ResultCollection results;
            IsosResultDeSerializer deserializer = new IsosResultDeSerializer(run.Filename + "_tmp.bin");
            int counter = 0;
            do
            {
                counter++;
                results = deserializer.GetNextSetOfResults();

            } while (results != null);

            Assert.AreEqual(8, counter);



        }



        [Test]
        public void deserializerGetResultsTest1()
        {
            IsosResultDeSerializer deserializer = new IsosResultDeSerializer(deserializerTestFile1);
            ResultCollection results1 = deserializer.GetNextSetOfResults();
            Assert.AreEqual(10017, results1.ResultList.Count);
            Assert.AreEqual(612.313809325556, (decimal)results1.ResultList[1000].IsotopicProfile.GetMZ());
            Assert.AreEqual(4094, results1.ResultList[1000].ScanSet.PrimaryScanNumber);


            ResultCollection results2 = deserializer.GetNextSetOfResults();
            Assert.AreEqual(1130, results2.ResultList.Count);
            Assert.AreEqual(558.615648653453, (decimal)results2.ResultList[1000].IsotopicProfile.GetMZ());
            Assert.AreEqual(4988, results2.ResultList[1000].ScanSet.PrimaryScanNumber);


            ResultCollection results3 = deserializer.GetNextSetOfResults();
            Assert.IsNull(results3);


        }


        [Test]
        public void deserializerGetResultsTest2()
        {

            ResultCollection results;
            IsosResultDeSerializer deserializer = new IsosResultDeSerializer(deserializerTestFile1); int counter = 0;
            do
            {
                counter++;
                results = deserializer.GetNextSetOfResults();

            } while (results != null);

            Assert.AreEqual(3, counter);


        }


        [Test]
        public void deserializerGetResultsTest3()
        {


            ResultCollection results;
            IsosResultDeSerializer deserializer = new IsosResultDeSerializer(deserializerUIMFTestFile1);
            int counter = 0;
            do
            {
                counter++;
                results = deserializer.GetNextSetOfResults();

                if (results != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (IsosResult result in results.ResultList)
                    {
                        UIMFIsosResult uimfResult = (UIMFIsosResult)result;
                        sb.Append(uimfResult.FrameSet.PrimaryFrame);
                        sb.Append("\t");
                        sb.Append(uimfResult.ScanSet.PrimaryScanNumber);
                        sb.Append("\t");
                        sb.Append(uimfResult.IsotopicProfile.GetMZ());
                        sb.Append("\t");
                        sb.Append(uimfResult.IsotopicProfile.GetAbundance());
                        sb.Append("\n");

                    }

                    Console.WriteLine("----------------------------------------------------------------------");
                    Console.Write(sb.ToString());
                }
            } while (results != null);

            Assert.AreEqual(8, counter);


        }

    }
}
