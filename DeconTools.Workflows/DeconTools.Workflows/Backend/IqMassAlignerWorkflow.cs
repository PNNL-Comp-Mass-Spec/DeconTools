using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend
{
    public class IqMassAlignerWorkflow : IqExecutor
    {
        private IqTargetUtilities _targetUtilities = new IqTargetUtilities();

        private PeptideUtils _peptideUtils = new PeptideUtils();


        #region Constructors

        public IqMassAlignerWorkflow(WorkflowExecutorBaseParameters parameters, Run run)
            : base(parameters, run)
        {
            IsDataExported = false;
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods


        public override void LoadAndInitializeTargets(string targetsFilePath)
        {
            var importer = new IqTargetsFromFirstHitsFileImporter(targetsFilePath);
            Targets = importer.Import().Where(p => p.QualityScore < 0.01).OrderBy(p => p.QualityScore).ToList();


            List<IqTarget> filteredList = new List<IqTarget>();
            //calculate empirical formula for targets using Code and then monoisotopic mass

            foreach (var iqTarget in Targets)
            {

                iqTarget.Code = _peptideUtils.CleanUpPeptideSequence(iqTarget.Code);

                if (_peptideUtils.ValidateSequence(iqTarget.Code))
                {
                    iqTarget.EmpiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(iqTarget.Code, true, true);
                    double calcMonoMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(iqTarget.EmpiricalFormula);
                    double monoMassFromFirstHitsFile = iqTarget.MonoMassTheor;

                    bool massCalculationsAgree = Math.Abs(monoMassFromFirstHitsFile - calcMonoMass) < 0.02;
                    if (massCalculationsAgree)
                    {
                        iqTarget.ElutionTimeTheor = iqTarget.ScanLC/(double)Run.MaxLCScan;

                        filteredList.Add(iqTarget);
                        _targetUtilities.UpdateTargetMissingInfo(iqTarget, true);

                        IqTargetMsgfFirstHit chargeStateTarget = new IqTargetMsgfFirstHit();

                        _targetUtilities.CopyTargetProperties(iqTarget, chargeStateTarget);

                        iqTarget.AddTarget(chargeStateTarget);

                    }


                }


            }


            Targets = filteredList;

            TargetedWorkflowParameters workflowParameters = new BasicTargetedWorkflowParameters();
            workflowParameters.ChromNETTolerance = 0.005;
            workflowParameters.ChromGenTolerance = 50;


            var workflow = new BasicIqWorkflow(Run, workflowParameters);


            //define workflows for parentTarget and childTargets
            var parentWorkflow = new ChromPeakDeciderIqWorkflow(Run, workflowParameters);
            var childWorkflow = new ChargeStateChildIqWorkflow(Run, workflowParameters);

            IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
            workflowAssigner.AssignWorkflowToParent(parentWorkflow, Targets);
            workflowAssigner.AssignWorkflowToChildren(childWorkflow, Targets);




            //IqWorkflowAssigner workflowAssigner = new IqWorkflowAssigner();
            //workflowAssigner.AssignWorkflowToParent(workflow, Targets);


        }



        #endregion

        #region Private Methods

        #endregion

        public void SetMassTagReferences(List<IqTarget>massTagReferences )
        {
            MassTagReferences = massTagReferences;
        }

        protected List<IqTarget> MassTagReferences { get; set; }

        public void ExecuteMassAlignerWorkflow()
        {

            Execute(Targets);

            UpdateMsgfTargetsWithDatabaseNetValues(Targets);

            

        }

        private void UpdateMsgfTargetsWithDatabaseNetValues(List<IqTarget> targets)
        {
            var query = (from massTag in MassTagReferences
                         join target in targets on massTag.Code equals target.Code
                         select new
                         {
                             MassTag = massTag,
                             MsgfTarget = target
                         }).ToList();


            foreach (var thing in query)
            {
                thing.MsgfTarget.ID = thing.MassTag.ID;
                thing.MsgfTarget.ElutionTimeTheor = thing.MassTag.ElutionTimeTheor;
            }
           
        }
    }
}
