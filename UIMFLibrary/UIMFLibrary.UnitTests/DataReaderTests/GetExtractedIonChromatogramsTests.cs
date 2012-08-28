using System;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace UIMFLibrary.UnitTests.DataReaderTests
{
    [TestFixture]
    public class GetExtractedIonChromatogramsTests
    {

        DataReader m_reader;

        [Test]
        public void GetDriftTimeProfileTest1()
        {
            int startFrame = 1280;
            int startScan = 150;
            double targetMZ = 451.55;
            double toleranceInPPM = 10;

            double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;
           
            int[] scanVals = null;
            int[] intensityVals = null;

			using (m_reader = new DataReader(FileRefs.uimfStandardFile1))
			{
				m_reader.GetDriftTimeProfile(startFrame - 2, startFrame + 2, DataReader.FrameType.MS1, startScan - 100, startScan + 100, targetMZ, toleranceInMZ, ref scanVals, ref intensityVals);

				TestUtilities.display2DChromatogram(scanVals, intensityVals);

				//TODO:   assert some values
			}
        }

        [Test]
        public void GetLCChromatogramTest2()
        {
            //TODO:   changed the source file... so need to find a better targetMZ and frame range for this test

            int startFrame = 600;
            int endFrame = 800;

            int startScan = 100;
            int stopScan = 350;

			using (m_reader = new DataReader(FileRefs.uimfStandardFile1))
			{
				double targetMZ = 636.8466;    // see frame 1000, scan 170
				double toleranceInPPM = 20;
				double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;

				int[] frameVals = null;
				int[] intensityVals = null;

				//m_reader.GetDriftTimeProfile(testFrame, frameType, startScan, stopScan, targetMZ, toleranceInMZ, ref scanVals, ref intensityVals);
				Stopwatch sw = new Stopwatch();
				sw.Start();
				m_reader.GetLCProfile(startFrame, endFrame, DataReader.FrameType.MS1, startScan, stopScan, targetMZ, toleranceInMZ, out frameVals, out intensityVals);
				sw.Stop();
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < frameVals.Length; i++)
				{
					sb.Append(frameVals[i]);
					sb.Append('\t');
					sb.Append(intensityVals[i]);
					sb.Append(Environment.NewLine);
				}

				//Assert.AreEqual(171, frameVals[71]);
				//Assert.AreEqual(6770, intensityVals[71]);
				Assert.AreEqual(endFrame - startFrame + 1, frameVals.Length);
				//Console.Write(sb.ToString());
				Console.WriteLine("Time (ms) = " + sw.ElapsedMilliseconds);
			}
        }

        [Test]
        public void GetLCChromatogramTest3()
        {
            int startFrame = 1280;
            int startScan = 163;
            double targetMZ = 464.25486;
            double toleranceInPPM = 25;

            double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;
			using (m_reader = new DataReader(FileRefs.uimfStandardFile1))
			{
				int[] frameVals = null;
				// int[] scanVals = null;
				int[] intensityVals = null;

				Stopwatch sw = new Stopwatch();
				sw.Start();
				m_reader.GetLCProfile(startFrame - 200, startFrame + 200, DataReader.FrameType.MS1, startScan - 2, startScan + 2, targetMZ, toleranceInMZ, out frameVals, out intensityVals);
				sw.Stop();

				Console.WriteLine("Time (ms) = " + sw.ElapsedMilliseconds);

				//TestUtilities.display2DChromatogram(frameVals, intensityVals);
			}
        }

        [Test]
        public void Get3DElutionProfile_test1()
        {
            //int startFrame = 1000;
            //int stopFrame = 1003;

            int startFrame = 1280;
            int startScan = 163;
            double targetMZ = 464.25486;
            double toleranceInPPM = 25;

            double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;

			int[] frameVals = null;
			int[] scanVals = null;
			int[] intensityVals = null;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			using (m_reader = new DataReader(FileRefs.uimfStandardFile1))
			{
				m_reader.Get3DElutionProfile(startFrame - 20, startFrame + 20, 0, startScan - 20, startScan + 20, targetMZ, toleranceInMZ, out frameVals, out scanVals, out intensityVals);
			}

			sw.Stop();

			int max = TestUtilities.getMax(intensityVals);
			float[] normInten = new float[intensityVals.Length];
			for (int i = 0; i < intensityVals.Length; i++)
			{
				normInten[i] = (float)intensityVals[i] / max;

			}
            
			Console.WriteLine("Time (ms) = " + sw.ElapsedMilliseconds);
        }

        [Test]
        public void Get3DElutionProfile_test2()
        {
            int startFrame = 524;
            int startScan = 128;

            double targetMZ = 295.9019;    // see frame 2130, scan 153
            double toleranceInPPM = 25;
            double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;

			using (m_reader = new DataReader(FileRefs.uimfStandardFile1))
			{
				int[][] values = m_reader.GetFramesAndScanIntensitiesForAGivenMz(startFrame - 40, startFrame + 40, 0, startScan - 60, startScan + 60, targetMZ, toleranceInMZ);

				StringBuilder sb = new StringBuilder();

				for (int i = 0; i < values.Length; i++)
				{
					for (int j = 0; j < values[i].Length; j++)
					{
						sb.Append(values[i][j].ToString() + ",");
					}
					sb.Append(Environment.NewLine);

				}

				// Console.WriteLine(sb.ToString());
			}
        }

        [Test]
        public void Get3DElutionProfile_test3()
        {
            //int startFrame = 1000;
            //int stopFrame = 1003;

            int startFrame = 400;
            int stopFrame = 600;

            //int startScan = 150;
            //int stopScan = 200;

            int startScan = 110;
            int stopScan = 210;

            double targetMZ = 475.7499;
            double toleranceInPPM = 25;
            double toleranceInMZ = toleranceInPPM / 1e6 * targetMZ;

            string filePath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";

			int[] frameVals = null;
			int[] scanVals = null;
			int[] intensityVals = null;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			using (m_reader = new DataReader(filePath))
			{
				m_reader.Get3DElutionProfile(startFrame, stopFrame, 0, startScan, stopScan, targetMZ, toleranceInMZ, out frameVals, out scanVals, out intensityVals);
			}
            
            sw.Stop();
            Console.WriteLine("Time in millisec for extracting 3D profile = " + sw.ElapsedMilliseconds);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < frameVals.Length; i++)
            {
                sb.Append(frameVals[i] + "\t" + scanVals[i] + "\t" + intensityVals[i] + Environment.NewLine);

            }

            //Console.WriteLine(sb.ToString());
        }
        

    }
}
