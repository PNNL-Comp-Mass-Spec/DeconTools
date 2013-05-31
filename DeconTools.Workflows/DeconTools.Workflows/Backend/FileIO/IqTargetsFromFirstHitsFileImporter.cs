using System.Collections.Generic;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class IqTargetsFromFirstHitsFileImporter:IqTargetImporter
    {

        #region Constructors


        public IqTargetsFromFirstHitsFileImporter(string filename):base()
        {
            Filename = filename;

            TargetIDHeaders = new[] {"ResultID"};
            ScanHeaders = new[] { "ScanLC", "LCScan", "Scan", "scanClassRep", "ScanNum" };

            MassErrorHeaders = new[] {"DelM_PPM"};

            MzHeaders = new[] {"PrecursorMZ", "MZ"};

        }

        #endregion

        #region Properties

        protected string[] MassErrorHeaders { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        protected override IqTarget ConvertTextToIqTarget(List<string> processedRow)
        {
            IqTargetMsgfFirstHit target = new IqTargetMsgfFirstHit();
            target.ID = ParseIntField(processedRow, TargetIDHeaders, -1);

            if (target.ID == -1)
            {
                target.ID = GetAutoIncrementForTargetID();
            }

            target.EmpiricalFormula = ParseStringField(processedRow, EmpiricalFormulaHeaders, string.Empty);
            target.Code = ParseStringField(processedRow, CodeHeaders, "");
            target.ElutionTimeTheor = ParseDoubleField(processedRow, NETHeaders, 0);
            target.ScanLC = ParseIntField(processedRow, ScanHeaders, -1);
            target.QualityScore = ParseDoubleField(processedRow, QualityScoreHeaders, -1);
            target.ChargeState = ParseIntField(processedRow, ChargeStateHeaders, 0);
            target.MassError = ParseDoubleField(processedRow, MassErrorHeaders, 0);
            target.MZTheor = ParseDoubleField(processedRow, MzHeaders, 0);

            target.MonoMassTheor = (target.MZTheor - DeconTools.Backend.Globals.PROTON_MASS)*target.ChargeState;


            IqTargetUtilities targetUtilities = new IqTargetUtilities();

           

            return target;
        }
    }
}
