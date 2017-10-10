
namespace DeconTools.Utilities.SqliteUtils
{
    public class Field
    {

        #region Constructors
        public Field(string name, string type)
        {
            Name = name;
            Type = type;
        }
        #endregion

        #region Properties

        public string Name { get; set; }

        private string type;
        public string Type
        {
            get => type.ToUpper();
            set => type = value;
        }

        #endregion

        #region Public Methods
        public override string ToString()
        {
            return Name + " " + Type;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
