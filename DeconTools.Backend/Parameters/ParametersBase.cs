using System;
using System.Globalization;
using System.Xml.Linq;

namespace DeconTools.Backend.Parameters
{
    [Serializable]
    public abstract class ParametersBase
    {
        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public abstract void LoadParameters(XElement xElement);

        /// <summary>
        /// This load method supports loading from an alternate source. This was done to support the old
        /// DeconTools parameter file format until the new parameter file was pushed out.
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadParametersV2(XElement xElement)
        {
            throw new NotImplementedException("LoadParametersV2 is not supported in this mode.");
        }

        protected object GetEnum(XElement element, string elementName, Type enumType, object defaultVal)
        {
            var parameter = element.Element(elementName);

            if (parameter == null)
            {
                return defaultVal;
            }

            if (!Enum.IsDefined(enumType, parameter.Value))
            {
                //Log.Warn("Trying to load Enum using an illegal value. Enum= " + enumType + "; Value= " + parameter.Value);
                return defaultVal;
            }

            return Enum.Parse(enumType, parameter.Value, true);
        }

        protected bool GetBoolVal(XElement element, string elementName, bool defaultVal = false)
        {
            var parameter = element.Element(elementName);

            if (parameter == null)
            {
                return defaultVal;
            }

            bool val;
            try
            {
                val = Convert.ToBoolean(parameter.Value);
            }
            catch (Exception)
            {
                val = defaultVal;
            }

            return val;
        }

        protected double GetDoubleValue(XElement element, string elementName, double defaultVal = 0)
        {
            var parameter = element.Element(elementName);

            if (parameter == null)
            {
                return defaultVal;
            }

            double val;
            try
            {
                val = double.Parse(parameter.Value, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                val = defaultVal;
            }

            return val;
        }

        protected int GetIntValue(XElement element, string elementName, int defaultVal = 0)
        {
            var parameter = element.Element(elementName);

            if (parameter == null)
            {
                return defaultVal;
            }

            int val;
            try
            {
                val = int.Parse(parameter.Value);
            }
            catch (Exception)
            {
                val = defaultVal;
            }

            return val;
        }

        protected string GetStringValue(XElement element, string elementName, string defaultVal = "")
        {
            var parameter = element.Element(elementName);

            if (parameter == null)
            {
                return defaultVal;
            }
            return parameter.Value;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
