using System;
using System.Linq;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests.IQWorkflowTests
{
    [TestFixture]
    public class IQTargetUtilitiesTests
    {
        [Test]
        public void CreateTargetsFromEmpiricalFormulaOnlyTest1()
        {
            IqTargetUtilities util = new IqTargetUtilities();


            string[] formulas = new string[]{"C133H213N29O44","C95H155N29O39","C126H198N32O42","C109H168N24O37","C103H165N29O35"};


            var targets=  util.CreateTargets(formulas);

            
            foreach (IqTarget parentTarget in targets)
            {
                Console.WriteLine(parentTarget.ID + "\t" + parentTarget.MonoMassTheor.ToString("0.00000") + "\tNumChildren= " + parentTarget.ChildTargets().Count());
                if (parentTarget.HasChildren())
                {
                    foreach (IqTarget childTarget in parentTarget.ChildTargets())
                    {
                        Console.WriteLine("\t\t\t" + childTarget.ID + "\t" + childTarget.MonoMassTheor.ToString("0.000") + "\t" +
                                          childTarget.MZTheor.ToString("0.000") + "\t" + childTarget.ChargeState);

                    }
                }
            }

        }



        [Test]
        public void CreateTargetsFromEmpiricalFormulaOnlyLargePeptideTest1()
        {
            IqTargetUtilities util = new IqTargetUtilities();

            string peptideSequence =
                "PEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDE";

            var peptideUtil = new PeptideUtils();
            var empiricalFormula=  peptideUtil.GetEmpiricalFormulaForPeptideSequence(peptideSequence);



            string[] formulas = new string[] { empiricalFormula };


            var targets = util.CreateTargets(formulas);


            foreach (IqTarget parentTarget in targets)
            {
                Console.WriteLine(parentTarget.ID + "\t" + parentTarget.MonoMassTheor.ToString("0.00000") + "\tNumChildren= " + parentTarget.ChildTargets().Count());
                if (parentTarget.HasChildren())
                {
                    foreach (IqTarget childTarget in parentTarget.ChildTargets())
                    {
                        Console.WriteLine("\t\t\t" + childTarget.ID + "\t" + childTarget.MonoMassTheor.ToString("0.00000") + "\t" +
                                          childTarget.MZTheor.ToString("0.00000") + "\t" + childTarget.ChargeState + "\t" + (1.00235d/childTarget.ChargeState).ToString("0.0000"));

                    }
                }
            }




        }

    }
}
