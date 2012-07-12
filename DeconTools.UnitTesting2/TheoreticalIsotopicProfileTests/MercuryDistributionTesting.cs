using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.MercuryIsotopicDistribution;
using NUnit.Framework;
using PNNLOmics.Data.Constants;

namespace DeconTools.UnitTesting2.TheoreticalIsotopicProfileTests
{
    [TestFixture]
    public class MercuryDistributionTesting
    {
        //private DeconToolsV2.clsMercuryIsotopeDistribution decon2LSMercuryDistribution;


        [Test]
        public void Test1()
        {
            //decon2LSMercuryDistribution = new DeconToolsV2.clsMercuryIsotopeDistribution();


        }

        [Test]
        public void mercuryIsoDist2_test1()
        {
            var mercury = new MercuryIsoDistCreator2();
            mercury.Resolution = 100000;
            var iso = mercury.GetIsotopePattern("C66H114N20O21S2", 2);    //Peptide 'SAMPLERSAMPLER'

            var timeVals = new List<long>();

            int numIterations = 100;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < numIterations; i++)
            {
                iso = mercury.GetIsotopePattern("C66H114N20O21S2", 2);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds / (double)numIterations);


        }


        [Test]
        public void ShowAverageMassesTest1()
        {

            string[] elements = { "C", "H", "N", "O", "S", "P", "Na", "Cl", "Si", "Hg" };

            foreach (var element in elements)
            {
                var monomass = Constants.Elements[element].MassAverage;

                Console.WriteLine(element + "\t" + monomass);
            }
        }




    }
}
