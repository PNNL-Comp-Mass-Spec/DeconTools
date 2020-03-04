using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public abstract class TargetedResultToTextExporter : TextFileExporter<TargetedResultDTO>
    {

        public static TargetedResultToTextExporter CreateExporter(WorkflowParameters workflowParameters, string outputFileName)
        {
            TargetedResultToTextExporter exporter;

            switch (workflowParameters.WorkflowType)
            {
                case Globals.TargetedWorkflowTypes.Undefined:
                    exporter = new UnlabeledTargetedResultToTextExporter(outputFileName);
                    break;
                case Globals.TargetedWorkflowTypes.UnlabeledTargeted1:
                    exporter = new UnlabeledTargetedResultToTextExporter(outputFileName);
                    break;
                case Globals.TargetedWorkflowTypes.O16O18Targeted1:
                    exporter = new O16O18TargetedResultToTextExporter(outputFileName);
                    break;
                case Globals.TargetedWorkflowTypes.SipperTargeted1:
                    exporter = new SipperResultToLcmsFeatureExporter(outputFileName);
                    break;
                case Globals.TargetedWorkflowTypes.N14N15Targeted1:
                    exporter = new N14N15TargetedResultToTextExporter(outputFileName);
                    break;
                case Globals.TargetedWorkflowTypes.TopDownTargeted1:
                    exporter = new TopDownTargetedResultToTextExporter(outputFileName);
                    break;
                case Globals.TargetedWorkflowTypes.Deuterated:
                    exporter = new DeuteratedTargetedResultToTextExporter(outputFileName);
                    break;

                case Globals.TargetedWorkflowTypes.TargetedAlignerWorkflow1:
                    throw new NotImplementedException("Cannot create exporter for this type of workflow");
                case Globals.TargetedWorkflowTypes.PeakDetectAndExportWorkflow1:
                    throw new NotImplementedException("Cannot create exporter for this type of workflow");
                case Globals.TargetedWorkflowTypes.BasicTargetedWorkflowExecutor1:
                    throw new NotImplementedException("Cannot create exporter for this type of workflow");
                default:
                    exporter = new UnlabeledTargetedResultToTextExporter(outputFileName);
                    break;
            }

            return exporter;

        }


        #region Constructors

        protected TargetedResultToTextExporter(string filename) : base(filename, '\t') { }

        #endregion


        protected override string buildResultOutput(TargetedResultDTO result)
        {
            var sb = new StringBuilder();

            sb.Append(addBasicTargetedResult(result));

            var additionalInfo = addAdditionalInfo(result);
            if (string.IsNullOrEmpty(additionalInfo))
                return sb.ToString();

            sb.Append(Delimiter);
            sb.Append(addAdditionalInfo(result));
            return sb.ToString();

        }

        protected virtual string addAdditionalInfo(TargetedResultDTO result)
        {
            return string.Empty;
        }

        protected virtual string addBasicTargetedResult(TargetedResultDTO result)
        {
            var data = new List<string>
            {
                result.DatasetName,
                result.TargetID.ToString(),
                result.Code,
                result.EmpiricalFormula,
                result.ChargeState.ToString(),
                result.ScanLC.ToString(),
                result.ScanLCStart.ToString(),
                result.ScanLCEnd.ToString(),
                result.NumMSScansSummed.ToString(),
                result.NET.ToString("0.00000"),
                result.NETError.ToString("0.000000"),
                result.NumChromPeaksWithinTol.ToString(),
                result.NumQualityChromPeaksWithinTol.ToString(),
                result.MonoMass.ToString("0.00000"),
                result.MonoMassCalibrated.ToString("0.00000"),
                result.MassErrorBeforeCalibration.ToString("0.00"),
                result.MassErrorAfterCalibration.ToString("0.00"),
                result.MonoMZ.ToString("0.00000"),
                result.Intensity.ToString("0.000"),
                result.FitScore.ToString("0.0000"),
                result.IScore.ToString("0.0000"),
                result.FailureType,
                result.ErrorDescription
            };

            return string.Join(Delimiter.ToString(), data);

        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                "Dataset",
                "TargetID",
                "Code",
                "EmpiricalFormula",
                "ChargeState",
                "Scan",
                "ScanStart",
                "ScanEnd",
                "NumMSSummed",
                "NET",
                "NETError",
                "NumChromPeaksWithinTol",
                "NumQualityChromPeaksWithinTol",
                "MonoisotopicMass",
                "MonoisotopicMassCalibrated",
                "MassErrorBefore",
                "MassErrorAfter",
                "MonoMZ",
                "IntensityRep",
                "FitScore",
                "IScore",
                "FailureType",
                "ErrorDescription"
            };

            return string.Join(Delimiter.ToString(), data);
        }
    }
}
