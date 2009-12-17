using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Data;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class PeakChromatogramGeneratorTests
    {
        private string xcaliburPeakDataFile = "..\\..\\TestFiles\\XCaliburPeakDataScans5500-6500.txt";
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";


        [Test]
        public void getPeakChromatogramTest1()
        {
            MassTag massTag = new MassTag();
            massTag.ID = 56488;
            massTag.MonoIsotopicMass = 2275.1694779;
            massTag.ChargeState = 3;
            massTag.MZ = massTag.MonoIsotopicMass/massTag.ChargeState+1.00727649;


            Run run = new XCaliburRun(xcaliburTestfile);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(xcaliburPeakDataFile);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            run.CurrentMassTag = massTag;

            Task peakChromGen = new PeakChromatogramGenerator(10);
            peakChromGen.Execute(run.ResultCollection);

            Assert.AreEqual(true, run.ResultCollection.ResultList[0] is N14N15_TResult);
            Assert.AreEqual(52, ((N14N15_TResult)run.ResultCollection.ResultList[0]).ChromValues.Xvalues.Length);

            Assert.AreEqual(5509, ((N14N15_TResult)run.ResultCollection.ResultList[0]).ChromValues.Xvalues[1]);
            Assert.AreEqual(14191370, ((N14N15_TResult)run.ResultCollection.ResultList[0]).ChromValues.Yvalues[1]);

            displayChromValues(run.ResultCollection);


        }

        private void displayChromValues(ResultCollection resultCollection)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IsosResult result in resultCollection.ResultList)
            {
                N14N15_TResult targetedResult = (N14N15_TResult)result;
                sb.Append("---------------- Mass tag ");
                sb.Append(targetedResult.MassTag.ID);
                sb.Append("; MZ = \t");
                sb.Append(targetedResult.MassTag.MZ.ToString("0.00000"));
                sb.Append("  -------------------------\n");
                for (int i = 0; i < targetedResult.ChromValues.Xvalues.Length; i++)
                {
                    sb.Append(targetedResult.ChromValues.Xvalues[i]);
                    sb.Append("\t");
                    sb.Append(targetedResult.ChromValues.Yvalues[i]);
                    sb.Append(Environment.NewLine);
                    
                }
                sb.Append("----------------------- end -----------------------------\n");
            }

            Console.Write(sb.ToString());
        }


 
    }
}
