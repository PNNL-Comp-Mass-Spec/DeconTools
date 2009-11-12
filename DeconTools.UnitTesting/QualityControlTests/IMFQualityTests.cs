using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using System.IO;
using DeconTools.Backend.Data;
using DeconTools.Backend;

namespace DeconTools.UnitTesting.QualityControlTests
{
    [TestFixture]
    public class IMFQualityTests
    {
        private string imfFilepath = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";
        private string xcaliburFilepath = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        string canonIMFIsosFilename = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_CANON_isos.csv";
        string canonXCaliburIsosFileName = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_isos.csv";

        string qtParameterfile1 = "..\\..\\TestFiles\\QT_ParameterFile1.xml";
        string qtRawParameterfile = "..\\..\\TestFiles\\QT_ParameterFile2.xml";


        [Test]
        public void IMFcompareToCanonicalResults_noSumming()
        {

            //generate results using new framework

            int numScansSummed = 1;

            Run run = new IMFRun(imfFilepath);

            ResultCollection results = new ResultCollection(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 231, 233, numScansSummed, 1);
            sscc.Create();

            ParameterLoader loader = new ParameterLoader();
            loader.LoadParametersFromFile(qtParameterfile1);

            foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                Task msgen = new GenericMSGenerator();
                msgen.Execute(results);


                Task peakDetector = new DeconToolsPeakDetector(loader.PeakParameters);
                peakDetector.Execute(results);

                Task horndecon = new HornDeconvolutor(loader.TransformParameters);
                horndecon.Execute(results);

                Task scanResultUpdater = new ScanResultUpdater();
                scanResultUpdater.Execute(results);
            }

            //read in results from canonical _isos
            List<IsosResult> canonIsos = readInIsos(canonIMFIsosFilename, Globals.MSFileType.PNNL_IMS);
            Assert.AreEqual(canonIsos.Count, results.ResultList.Count);

            //compare numbers

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < results.ResultList.Count; i++)
            {
                sb.Append("scanmass\t");
                sb.Append(results.ResultList[i].ScanSet.PrimaryScanNumber);
                sb.Append("\t");
                sb.Append(canonIsos[i].ScanSet.PrimaryScanNumber);
                sb.Append("\n");

                sb.Append("monoMass\t");
                sb.Append(results.ResultList[i].IsotopicProfile.MonoIsotopicMass.ToString("0.0000"));
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.MonoIsotopicMass);
                sb.Append("\n");

                sb.Append("intens\t");
                sb.Append(results.ResultList[i].IsotopicProfile.IntensityAggregate);
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.IntensityAggregate);
                sb.Append("\n");

                sb.Append("score\t");
                sb.Append(results.ResultList[i].IsotopicProfile.Score.ToString("0.0000"));
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.Score);
                sb.Append("\n");

                sb.Append("FWHM\t");
                sb.Append(results.ResultList[i].IsotopicProfile.GetFWHM().ToString("0.0000"));
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.GetFWHM());
                sb.Append("\n");

                sb.Append("s/n\t");
                sb.Append(results.ResultList[i].IsotopicProfile.GetSignalToNoise().ToString("0.0000"));
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.GetSignalToNoise());
                sb.Append("\n");

                sb.Append("\n");

            }

            Console.Write(sb.ToString());




        }

        [Test]
        public void XCaliburCompareToCanonicalResults_noSumming()
        {

            //generate results using new framework

            int numScansSummed = 1;

            Run run = new XCaliburRun(xcaliburFilepath);

            ResultCollection results = new ResultCollection(run);

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6050, numScansSummed, 1);
            sscc.Create();

            ParameterLoader loader = new ParameterLoader();
            loader.LoadParametersFromFile(qtRawParameterfile);
            loader.PeakParameters.ThresholdedData = true;      // only for RAW data

            foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                Task msgen = new GenericMSGenerator();
                msgen.Execute(results);


                Task peakDetector = new DeconToolsPeakDetector(loader.PeakParameters);
                peakDetector.Execute(results);

                Task horndecon = new HornDeconvolutor(loader.TransformParameters);
                horndecon.Execute(results);

                Task scanResultUpdater = new ScanResultUpdater();
                scanResultUpdater.Execute(results);
            }

            //read in results from canonical _isos
            List<IsosResult> canonIsos = readInIsos(canonXCaliburIsosFileName, Globals.MSFileType.Finnigan);
            Assert.AreEqual(canonIsos.Count, results.ResultList.Count);

            //compare numbers

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < results.ResultList.Count; i++)
            {
                sb.Append("scanmass\t");
                sb.Append(results.ResultList[i].ScanSet.PrimaryScanNumber);
                sb.Append("\t");
                sb.Append(canonIsos[i].ScanSet.PrimaryScanNumber);
                sb.Append("\n");

                sb.Append("monoMass\t");
                sb.Append(results.ResultList[i].IsotopicProfile.MonoIsotopicMass.ToString("0.0000"));
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.MonoIsotopicMass);
                sb.Append("\n");

                sb.Append("intens\t");
                sb.Append(results.ResultList[i].IsotopicProfile.IntensityAggregate);
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.IntensityAggregate);
                sb.Append("\n");

                sb.Append("score\t");
                sb.Append(results.ResultList[i].IsotopicProfile.Score.ToString("0.0000"));
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.Score);
                sb.Append("\n");

                sb.Append("FWHM\t");
                sb.Append(results.ResultList[i].IsotopicProfile.GetFWHM().ToString("0.0000"));
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.GetFWHM());
                sb.Append("\n");

                sb.Append("s/n\t");
                sb.Append(results.ResultList[i].IsotopicProfile.GetSignalToNoise().ToString("0.0000"));
                sb.Append("\t");
                sb.Append(canonIsos[i].IsotopicProfile.GetSignalToNoise());
                sb.Append("\n");

                sb.Append("\n");

            }

            Console.Write(sb.ToString());




        }

        private List<IsosResult> readInIsos(string canonIsosFilename, DeconTools.Backend.Globals.MSFileType filetype)
        {
            List<IsosResult> results = new List<IsosResult>();

            IsosImporter importer = new IsosImporter(canonIsosFilename, filetype);
            importer.Import(results);

            return results;

        }


    }
}
