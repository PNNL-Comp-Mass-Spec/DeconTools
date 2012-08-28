using System.Text;
using NUnit.Framework;

namespace UIMFLibrary.UnitTests.DataReaderTests
{
    public class MSMS_relatedTests
    {


        [Test]
        public void containsMSMSData_test1()
        {
            using (DataReader reader = new DataReader(FileRefs.uimfStandardFile1))
            {
                Assert.AreEqual(false, reader.HasMSMSData());
            }
        }


        [Test]
        public void containsMSMSData_test2()
        {
            using (var reader = new DataReader(FileRefs.uimfStandardFile1))
            {
                Assert.AreEqual(false, reader.HasMSMSData());
            }
        }

        [Test]
        public void containsMSMSDataTest3()
        {
            using (var reader = new DataReader(FileRefs.uimfContainingMSMSData1))
            {
                Assert.AreEqual(true, reader.HasMSMSData());

            }

        }

        [Test]
        public void GetFrameTypeTest1()
        {
            using (var reader = new DataReader(FileRefs.uimfContainingMSMSData1))
            {
                GlobalParameters gp = reader.GetGlobalParameters();

                int checkSum = 0;

                for (int frame = 1; frame <= gp.NumFrames; frame++)
                {
                    checkSum += frame * (int)reader.GetFrameTypeForFrame(frame);
                }

                Assert.AreEqual(222, checkSum);
            }
        }


        [Test]
        public void GetMSMSTest1()
        {
            using (var reader = new DataReader(FileRefs.uimfContainingMSMSData1))
            {
                int testFrame = 2;
                int startScan = 1;
                int stopScan = 300;

                int[] intensityArray;
                double[] mzArray;
                reader.GetSpectrum(testFrame, testFrame, DataReader.FrameType.MS2, startScan, stopScan, out mzArray,
                                   out intensityArray);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < mzArray.Length; i++)
                {
                    sb.Append(mzArray[i] + "\t" + intensityArray[i] + "\n");

                }
                Assert.IsNotNull(mzArray);
                Assert.IsTrue(mzArray.Length > 0);

            }
        }

    }
}
