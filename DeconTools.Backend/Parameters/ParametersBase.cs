using System;
using System.Xml.Linq;

//[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace DeconTools.Backend.Parameters
{
    public abstract class ParametersBase
    {

        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public abstract void LoadParameters(XElement xElement);


        protected object GetEnum(XElement element, string elementName, Type enumType, object defaultVal)
        {
            var parameter = element.Element(elementName);

            if (parameter == null)
            {
               
                return defaultVal;
            }

            if (!Enum.IsDefined(enumType,parameter.Value))
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
                val = Convert.ToDouble(parameter.Value);
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
                val = Convert.ToInt32(parameter.Value);
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
