using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.LabeledIsotopicDistUtilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.TheoreticalIsotopicProfileTests
{
    [TestFixture]
    public class LabeledIsotopicProfileUtilitiesTests
    {
        [Test]
        public void GetMixedProfileTest_C13()
        {
            var isoCreator = new LabeledIsotopicProfileUtilities();

            var peptideSeq = "SAMPLERSAMPLER";
            var elementLabelled = "C";

            var lightIsotope = 12;
            var heavyIsotope = 13;

            double percentLabelled1 = 0;        // first peptide population is unlabelled (0%)
            double percentLabelled2 = 10;       // second peptide polulation has 8% of its carbons labelled. 
            var fractionPopulationLabelled = 0.50;     // fraction of peptides that have heavy label. 

            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled1);
            var labelledIso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled2);


            isoCreator.AddIsotopicProfile(iso, 1 - fractionPopulationLabelled);
            isoCreator.AddIsotopicProfile(labelledIso, fractionPopulationLabelled);

            var mixedIso = isoCreator.GetMixedIsotopicProfile();

            TestUtilities.DisplayIsotopicProfileData(mixedIso);
        }


        [Test]
        public void GetMixedN15_Test1()
        {
            var isoCreator = new LabeledIsotopicProfileUtilities();

            var peptideSeq = "SAMPLERSAMPLER";
            var elementLabelled = "N";

            var lightIsotope = 14;
            var heavyIsotope = 15;

            double percentLabelled1 = 0;        // first peptide population is unlabelled (0%)
            double percentLabelled2 = 20;       // second peptide polulation has 8% of its carbons labelled. 
            var fractionPopulationLabelled = 0.50;     // fraction of peptides that have heavy label. 

            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled1);
            var labelledIso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled2);


            isoCreator.AddIsotopicProfile(iso, 1 - fractionPopulationLabelled);
            isoCreator.AddIsotopicProfile(labelledIso, fractionPopulationLabelled);

            var mixedIso = isoCreator.GetMixedIsotopicProfile();

            TestUtilities.DisplayIsotopicProfileData(mixedIso);
        }


    }
}
