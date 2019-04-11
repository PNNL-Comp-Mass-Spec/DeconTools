using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public abstract class ResultValidator:Task
    {
        #region Constructors
        #endregion

        #region Properties
        public abstract IsosResult CurrentResult { get; set; }
        #endregion

        #region Public Methods
        public abstract void ValidateResult(ResultCollection resultColl, IsosResult currentResult);
        #endregion

        #region Private Methods
        #endregion

        public override void Execute(ResultCollection resultList)
        {
            ValidateResult(resultList, this.CurrentResult);
        }
    }
}
