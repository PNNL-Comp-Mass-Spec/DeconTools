using System;
using System.IO;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    public class IsotopicProfileElutionExtractorTests
    {

        [Test]
        public void Get3DElutionTest1()
        {
            var peaksFile = FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var orbiFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            var run = RunUtilities.CreateAndLoadPeaks(orbiFile, peaksFile);

            Assert.IsNotNull(run);
            Assert.IsTrue(run.ResultCollection.MSPeakResultList.Count > 0);

            var extractor = new IsotopicProfileElutionExtractor();

            var minScan = 5900;
            var maxScan = 6300;
            double minMZ = 749;
            double maxMZ = 754;


            extractor.Get3DElutionProfileFromPeakLevelData(run, minScan, maxScan, minMZ, maxMZ, out var scans, out var mzBinVals, out var intensities);

            var intensities2D = extractor.GetIntensitiesAs2DArray();
            Console.WriteLine(extractor.OutputElutionProfileAsString());
        }

        [Test]
        public void Get3DElutionAndExportToFileTest1()
        {
            var peaksFile = FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var orbiFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;




            var run = RunUtilities.CreateAndLoadPeaks(orbiFile, peaksFile);

            var outputFile = Path.Combine(run.DataSetPath, run.DatasetName + "_sample3DelutionProfile.txt");


            Assert.IsNotNull(run);
            Assert.IsTrue(run.ResultCollection.MSPeakResultList.Count > 0);

            var extractor = new IsotopicProfileElutionExtractor();

            var minScan = 5900;
            var maxScan = 6300;
            double minMZ = 749;
            double maxMZ = 754;


            extractor.Get3DElutionProfileFromPeakLevelData(run, minScan, maxScan, minMZ, maxMZ, out var scans, out var mzBinVals, out var intensities);

            var intensities2D = extractor.GetIntensitiesAs2DArray();
            extractor.OutputElutionProfileToFile(outputFile);

        }



    }
}
