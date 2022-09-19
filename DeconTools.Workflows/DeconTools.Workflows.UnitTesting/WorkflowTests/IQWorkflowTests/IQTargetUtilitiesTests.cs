using System;
using System.Collections.Generic;
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
            var util = new IqTargetUtilities();

            var formulas = new string[]{"C133H213N29O44","C95H155N29O39","C126H198N32O42","C109H168N24O37","C103H165N29O35"};

            var targets=  util.CreateTargets(formulas);

            foreach (var parentTarget in targets)
            {
                Console.WriteLine(parentTarget.ID + "\t" + parentTarget.MonoMassTheor.ToString("0.00000") + "\tNumChildren= " + parentTarget.ChildTargets().Count());
                if (parentTarget.HasChildren())
                {
                    foreach (var childTarget in parentTarget.ChildTargets())
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
            var util = new IqTargetUtilities();

            var peptideSequence =
                "PEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDEPEPITIDE";

            var peptideUtil = new PeptideUtils();
            var empiricalFormula=  peptideUtil.GetEmpiricalFormulaForPeptideSequence(peptideSequence);

            var formulas = new string[] { empiricalFormula };

            var targets = util.CreateTargets(formulas);

            foreach (var parentTarget in targets)
            {
                Console.WriteLine(parentTarget.ID + "\t" + parentTarget.MonoMassTheor.ToString("0.00000") + "\tNumChildren= " + parentTarget.ChildTargets().Count());
                if (parentTarget.HasChildren())
                {
                    foreach (var childTarget in parentTarget.ChildTargets())
                    {
                        Console.WriteLine("\t\t\t" + childTarget.ID + "\t" + childTarget.MonoMassTheor.ToString("0.00000") + "\t" +
                                          childTarget.MZTheor.ToString("0.00000") + "\t" + childTarget.ChargeState + "\t" + (1.00235d/childTarget.ChargeState).ToString("0.0000"));
                    }
                }
            }
        }

        [Test]
        public void NodeLevelTest1()
        {
            var util = new IqTargetUtilities();

            IqTarget iqTarget1 = new IqChargeStateTarget();
            IqTarget iqTarget2 = new IqChargeStateTarget();

            IqTarget iqTarget3_1 = new IqChargeStateTarget();
            iqTarget3_1.ID = 3001;

            IqTarget iqTarget3_2 = new IqChargeStateTarget();
            iqTarget3_2.ID = 3002;

            IqTarget iqTarget4 = new IqChargeStateTarget();

            IqTarget iqTarget5 = new IqChargeStateTarget();

            iqTarget4.AddTarget(iqTarget5);
            iqTarget3_1.AddTarget(iqTarget4);
            iqTarget2.AddTarget(iqTarget3_1);
            iqTarget2.AddTarget(iqTarget3_2);
            iqTarget1.AddTarget(iqTarget2);

            var rootNode = iqTarget5.RootTarget;

            Assert.AreEqual(rootNode, iqTarget1);

            var nodeLevelCount=   util.GetTotalNodelLevels(iqTarget1);

            Assert.AreEqual(5, nodeLevelCount);

            nodeLevelCount = util.GetTotalNodelLevels(iqTarget5);

            Assert.AreEqual(5, nodeLevelCount);

            var targetList = new List<IqTarget>
            {
                iqTarget1
            };

            var level2Targets=  util.GetTargetsFromNodelLevel(targetList, 2);

            foreach (var level2Target in level2Targets)
            {
                Console.WriteLine(level2Target);
            }
        }
    }
}
