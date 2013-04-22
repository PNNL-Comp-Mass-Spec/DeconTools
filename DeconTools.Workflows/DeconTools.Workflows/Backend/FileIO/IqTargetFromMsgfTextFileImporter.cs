using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class IqTargetFromMsgfTextFileImporter:IqTargetImporter
    {

        #region Constructors

        public IqTargetFromMsgfTextFileImporter()
        {

        }


        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        protected override IqTarget ConvertTextToIqTarget(List<string> processedRowOfText)
        {
            throw new NotImplementedException();
        }
    }
}
