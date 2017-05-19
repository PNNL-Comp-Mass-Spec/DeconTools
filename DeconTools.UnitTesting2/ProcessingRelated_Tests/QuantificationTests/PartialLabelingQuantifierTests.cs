using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.LabeledIsotopicDistUtilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.QuantificationTests
{
    [TestFixture]
    public class PartialLabelingQuantifierTests
    {
        [Test]
        public void PartialLabelingQuantifierTest1()
        {
            // see https://jira.pnnl.gov/jira/browse/OMCS-743


            var peptideSeq = "SAMPLERSAMPLER";
             var isoCreator = new LabeledIsotopicProfileUtilities();

            var elementLabelled = "C";
            var lightIsotope = 12;
            var heavyIsotope = 13;

            var target = new PeptideTarget();
            target.Code = peptideSeq;
            target.EmpiricalFormula=   target.GetEmpiricalFormulaFromTargetCode();
            target.ChargeState = 2;
            target.CalculateMassesForIsotopicProfile(target.ChargeState);


            target.IsotopicProfile=   isoCreator.CreateIsotopicProfileFromEmpiricalFormula(target.EmpiricalFormula, elementLabelled, 12, 13, 0, target.ChargeState);
            

            double[] obsMZVals = {
                                       794.4027177, 794.9043951, 795.4060725, 795.9077499, 796.4094273, 796.9111047,
                                       797.4127822, 797.9144596, 798.416137, 798.9178144, 799.4194918, 799.9211693,
                                       800.4228467, 800.9245241, 801.4262015, 801.9278789, 802.4295564, 802.9312338,
                                       803.4329112, 803.9345886, 804.436266, 804.9379435, 805.4396209, 805.9412983,
                                       806.4429757, 806.9446531, 807.4463306, 807.948008, 808.4496854, 808.9513628,
                                       809.4530402, 809.9547176, 810.4563951
                                   };
            
            double[] obsIntensities = {
                                               0.202610146, 0.169786958, 0.10004317, 0.048528412, 0.023649271, 0.01375091,
                                               0.011401505, 0.010165125, 0, 0, 0, 0, 0, 0.016295369, 0.02039971, 0.02628524
                                               , 0.034368448, 0.042300575, 0.050392505, 0.057402354, 0.061016738,
                                               0.061990991, 0.059175713, 0.053291295, 0.046892873, 0.037952256, 0.030884989
                                               , 0.024965924, 0.019531111, 0.01613184, 0, 0, 0
                                           };
            



            var peakList = obsMZVals.Select((t, i) => new Peak(t, (float) obsIntensities[i], 0)).ToList();

            var partialLabelingQuantifier = new PartialLabelingQuantifier("C", 12, 13);

            var bestIso=  partialLabelingQuantifier.FindBestLabeledProfile(target, peakList, null);

            var counter = 0;
            foreach (var currentFitScore in partialLabelingQuantifier.FitScoreData)
            {
                Console.WriteLine(currentFitScore.Key + "\t" + currentFitScore.Value);
                counter++;
            }

        }

    }
}
