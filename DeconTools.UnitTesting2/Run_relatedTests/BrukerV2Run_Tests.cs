using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class BrukerV2Run_Tests
    {
        [Test]
        public void bruker12T_checkDataSetNames_andPaths()
        {
            BrukerV2Run run = new BrukerV2Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);
            Assert.AreEqual("12Ttest_000003", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataBasePath + "\\Bruker\\Bruker_Solarix12T\\12Ttest_000003", run.DataSetPath);
            Assert.AreEqual("\\\\protoapps\\UserData\\Slysz\\DeconTools_TestFiles\\Bruker\\Bruker_Solarix12T\\12Ttest_000003", run.Filename);
            Assert.AreEqual("\\\\protoapps\\UserData\\Slysz\\DeconTools_TestFiles\\Bruker\\Bruker_Solarix12T\\12Ttest_000003\\ser", run.RawData.FileName);
        }

        [Test]
        public void bruker15T_checkDataSetNames_andPaths()
        {
            BrukerV2Run run = new BrukerV2Run(FileRefs.RawDataMSFiles.Bruker15TFile1);
            Assert.AreEqual("092410_ubiquitin_AutoCID_000004", run.DatasetName);
            Assert.AreEqual(FileRefs.RawDataBasePath + @"\Bruker\Bruker_15T\092410_ubiquitin_AutoCID_000004", run.DataSetPath);
            Assert.AreEqual(FileRefs.RawDataBasePath + @"\Bruker\Bruker_15T\092410_ubiquitin_AutoCID_000004", run.Filename);
            Assert.AreEqual(FileRefs.RawDataBasePath + @"\Bruker\Bruker_15T\092410_ubiquitin_AutoCID_000004\092410_ubiquitin_AutoCID_000004.d\ser", run.RawData.FileName);
        }

        [Test]
        public void ConstructorError_wrongFileInput()
        {
            string wrongFileExample1 = FileRefs.RawDataMSFiles.BrukerSolarix12TFile1 + "\\ser";

            PreconditionException ex = Assert.Throws<PreconditionException>(delegate
            {
                Run run = new DeconTools.Backend.Runs.BrukerV2Run(wrongFileExample1);
            });
            Assert.That(ex.Message, Is.EqualTo("Could not initialize Dataset. Looking for a folder path, but user supplied a file path."));
        }

        [Test]
        public void ConstructorError_wrongFileInput2()
        {
            string wrongFileExample1 = FileRefs.RawDataMSFiles.BrukerSolarix12TFile1 + "\\filenotfound";

            PreconditionException ex = Assert.Throws<PreconditionException>(delegate
            {
                Run run = new DeconTools.Backend.Runs.BrukerV2Run(wrongFileExample1);
            });
            Assert.That(ex.Message, Is.EqualTo("Could not initialize Dataset. Target dataset folder not found."));
        }


        [Test]
        public void settingsTest1()
        {
            using (BrukerV2Run run = new BrukerV2Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1))
            {

                Assert.AreEqual(184346289.692624m, (decimal)run.CalibrationData.ML1);
                Assert.AreEqual(12.8161378555916m, (decimal)run.CalibrationData.ML2);
                Assert.AreEqual(833333.333333334m, (decimal)run.CalibrationData.SW_h);
                Assert.AreEqual(524288, run.CalibrationData.TD);
                Assert.AreEqual(0m, (decimal)run.CalibrationData.FR_Low);
                Assert.AreEqual(0, run.CalibrationData.ByteOrder);
            }
            

              
        }


        [Test]
        public void bruker12T_GetNumSpectraTest1()
        {
            BrukerV2Run run = new BrukerV2Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);
            Assert.AreEqual(8, run.GetNumMSScans());
            Assert.AreEqual(8, run.MaxScan);
        }


        [Test]
        public void bruker12T_GetNumSpectraTest2()
        {
            BrukerV2Run run = new BrukerV2Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);

            for (int i = 0; i < 100; i++)
            {
                run.GetNumMSScans();

            }


            Assert.AreEqual(8, run.GetNumMSScans());
            Assert.AreEqual(8, run.MaxScan);
        }

        [Test]
        public void bruker12T_GetMassSpectrumTest1()
        {
            BrukerV2Run run = new BrukerV2Run(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1);
            ScanSet scanset=new ScanSet(4);
            run.GetMassSpectrum(scanset,0,50000);
            //run.XYData.Display();
        }


        [Test]
        public void bruker15T_GetNumSpectraTest1()
        {
            BrukerV2Run run = new BrukerV2Run(FileRefs.RawDataMSFiles.Bruker15TFile1);
            Assert.AreEqual(18, run.GetNumMSScans());
            Assert.AreEqual(18, run.MaxScan);
        }

           

        [Test]
        public void bruker15T_GetMassSpectrumTest1()
        {
            BrukerV2Run run = new BrukerV2Run(FileRefs.RawDataMSFiles.Bruker15TFile1);
            ScanSet scanset = new ScanSet(4);
            run.GetMassSpectrum(scanset, 0, 50000);

            Assert.AreEqual(789.967903595286m, (decimal)run.XYData.Xvalues[129658]);
            Assert.AreEqual(9503829, run.XYData.Yvalues[129658]);
                        
        }


   


    }
}
