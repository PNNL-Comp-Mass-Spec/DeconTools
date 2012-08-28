using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using NUnit.Framework;

namespace UIMFLibrary.UnitTests.DataReaderTests
{

	class FrameAndScanInfo
	{
		public int startFrame;
		public int stopFrame;
		public int startScan;
		public int stopScan;

		public FrameAndScanInfo(int iStartFrame, int iStopFrame, int iStartScan, int iStopScan)
		{
			startFrame = iStartFrame;
			stopFrame = iStopFrame;
			startScan = iStartScan;
			stopScan = iStopScan;
		}
	}

    [TestFixture]
    public class GetMassSpectrumTests
    {
      
        FrameAndScanInfo testFrameScanInfo1 = new FrameAndScanInfo(0, 0, 110, 150);

        [Test]
        public void getSingleSummedMassSpectrumTest1()
        {
			using (DataReader dr = new DataReader(FileRefs.uimfStandardFile1))
			{
				GlobalParameters gp = dr.GetGlobalParameters();
				int[] intensities = new int[gp.Bins];
				double[] mzValues = new double[gp.Bins];

				int nonZeros = dr.GetSpectrum(testFrameScanInfo1.startFrame, testFrameScanInfo1.stopFrame, DataReader.FrameType.MS1, testFrameScanInfo1.startScan, testFrameScanInfo1.stopScan, out mzValues, out intensities);

				int nonZeroCount = (from n in mzValues where n != 0 select n).Count();
				Console.WriteLine("Num xy datapoints = " + nonZeroCount);
				//Assert.AreEqual(0, nonZeros);
			}
        }

        [Test]
        public void getFrame0_MS_Test1()
        {
			using (DataReader dr = new DataReader(FileRefs.uimfStandardFile1))
			{
				GlobalParameters gp = dr.GetGlobalParameters();
				int[] intensities = new int[gp.Bins];
				double[] mzValues = new double[gp.Bins];

				int nonZeros = dr.GetSpectrum(testFrameScanInfo1.startFrame, testFrameScanInfo1.stopFrame, DataReader.FrameType.MS1, testFrameScanInfo1.startScan, testFrameScanInfo1.stopScan, out mzValues, out intensities);
				TestUtilities.displayRawMassSpectrum(mzValues, intensities);
			}
        }

        [Test]
        public void getFrame0_MS_demultiplexedData_Test1()
        {
			using (DataReader dr = new DataReader(FileRefs.uimfStandardDemultiplexedFile1))
			{
				GlobalParameters gp = dr.GetGlobalParameters();
				int[] intensities = new int[gp.Bins];
				double[] mzValues = new double[gp.Bins];

				bool bRunTest = false;
				if (bRunTest)
				{
					int nonZeros = dr.GetSpectrum(testFrameScanInfo1.startFrame, testFrameScanInfo1.stopFrame, DataReader.FrameType.MS1, testFrameScanInfo1.startScan, testFrameScanInfo1.stopScan, out mzValues, out intensities);
					TestUtilities.displayRawMassSpectrum(mzValues, intensities);
				}
			
			}
        }

        [Test]
        public void getMultipleSummedMassSpectrumsTest1()
        {
			using (DataReader dr = new DataReader(FileRefs.uimfStandardFile1))
			{

				FrameAndScanInfo testFrameScanInfo2 = new FrameAndScanInfo(500, 550, 250, 256);

				for (int frame = testFrameScanInfo2.startFrame; frame <= testFrameScanInfo2.stopFrame; frame++)
				{
					GlobalParameters gp = dr.GetGlobalParameters();

					int[] intensities = new int[gp.Bins];
					double[] mzValues = new double[gp.Bins];

					int nonZeros = dr.GetSpectrum(frame, frame, DataReader.FrameType.MS1, testFrameScanInfo2.startScan, testFrameScanInfo2.stopScan, out mzValues, out intensities);

					//jump back
					nonZeros = dr.GetSpectrum(frame - 1, frame - 1, DataReader.FrameType.MS1, testFrameScanInfo2.startScan, testFrameScanInfo2.stopScan, out mzValues, out intensities);

					//and ahead... just testing it's ability to jump around
					nonZeros = dr.GetSpectrum(frame + 2, frame + 2, DataReader.FrameType.MS1, testFrameScanInfo2.startScan, testFrameScanInfo2.stopScan, out mzValues, out intensities);
				}
			}
        }
    }
}
