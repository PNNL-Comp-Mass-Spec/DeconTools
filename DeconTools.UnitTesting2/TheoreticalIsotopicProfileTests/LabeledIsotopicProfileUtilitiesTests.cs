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
            var elementLabeled = "C";

            var lightIsotope = 12;
            var heavyIsotope = 13;

            double percentLabeled1 = 0;        // first peptide population is unlabeled (0%)
            double percentLabeled2 = 10;       // second peptide population has 8% of its carbons labeled.
            var fractionPopulationLabeled = 0.50;     // fraction of peptides that have heavy label.

            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabeled, lightIsotope, heavyIsotope, percentLabeled1);
            var labeledIso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabeled, lightIsotope, heavyIsotope, percentLabeled2);

            isoCreator.AddIsotopicProfile(iso, 1 - fractionPopulationLabeled);
            isoCreator.AddIsotopicProfile(labeledIso, fractionPopulationLabeled);

            var mixedIso = isoCreator.GetMixedIsotopicProfile();

            TestUtilities.DisplayIsotopicProfileData(mixedIso);
        }

        [Test]
        public void GetMixedN15_Test1()
        {
            var isoCreator = new LabeledIsotopicProfileUtilities();

            var peptideSeq = "SAMPLERSAMPLER";
            var elementLabeled = "N";

            var lightIsotope = 14;
            var heavyIsotope = 15;

            double percentLabeled1 = 0;        // first peptide population is unlabeled (0%)
            double percentLabeled2 = 20;       // second peptide population has 8% of its carbons labeled.
            var fractionPopulationLabeled = 0.50;     // fraction of peptides that have heavy label.

            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabeled, lightIsotope, heavyIsotope, percentLabeled1);
            var labeledIso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabeled, lightIsotope, heavyIsotope, percentLabeled2);

            isoCreator.AddIsotopicProfile(iso, 1 - fractionPopulationLabeled);
            isoCreator.AddIsotopicProfile(labeledIso, fractionPopulationLabeled);

            var mixedIso = isoCreator.GetMixedIsotopicProfile();

            TestUtilities.DisplayIsotopicProfileData(mixedIso);
        }
    }
}
