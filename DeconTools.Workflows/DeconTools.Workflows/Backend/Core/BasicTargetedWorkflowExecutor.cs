using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowExecutor : TargetedWorkflowExecutor
    {

        #region Constructors
        public BasicTargetedWorkflowExecutor(WorkflowExecutorBaseParameters parameters, string datasetPath) : base(parameters, datasetPath) { }

        #endregion


        #region Public Methods


 

        private void UpdateTargetMissingInfo()
        {
            bool canUseReferenceMassTags = MassTagsForReference != null && MassTagsForReference.TargetList.Count > 0;

            foreach (LcmsFeatureTarget target in Targets.TargetList)
            {
                bool isMissingMonoMass = target.MonoIsotopicMass <= 0;

                if (String.IsNullOrEmpty(target.EmpiricalFormula))
                {
                    if (ReferenceMassTagIDList.Contains(target.FeatureToMassTagID) && canUseReferenceMassTags)
                    {
                        var mt = MassTagsForReference.TargetList.First(p => p.ID == target.FeatureToMassTagID);

                        //in DMS, Sequest will put an 'X' when it can't differentiate 'I' and 'L'
                        //  see:   \\gigasax\DMS_Parameter_Files\Sequest\sequest_ETD_N14_NE.params
                        //To create the theoretical isotopic profile, we will change the 'X' to 'L'
                        if (mt.Code.Contains("X"))
                        {
                            mt.Code = mt.Code.Replace('X', 'L');
                            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
                        }

                        target.Code = mt.Code;
                        target.EmpiricalFormula = mt.EmpiricalFormula;
                    }
                    else if (!String.IsNullOrEmpty(target.Code))
                    {
                        //Create empirical formula based on code. Assume it is an unmodified peptide
                        target.EmpiricalFormula = new PeptideUtils().GetEmpiricalFormulaForPeptideSequence(target.Code);

                    }
                    else
                    {
                        if (isMissingMonoMass)
                        {
                            throw new ApplicationException(
                                "Trying to prepare target list, but Target is missing both the 'Code' and the Monoisotopic Mass. One or the other is needed.");
                        }
                        target.Code = "AVERAGINE";
                        target.EmpiricalFormula =
                            IsotopicDistributionCalculator.GetAveragineFormulaAsString(target.MonoIsotopicMass);
                    }
                }


                if (isMissingMonoMass)
                {
                    target.MonoIsotopicMass =
                        EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(target.EmpiricalFormula);

                    target.MZ = target.MonoIsotopicMass / target.ChargeState + DeconTools.Backend.Globals.PROTON_MASS;
                }



            }
        }

        #endregion

    }
}
