using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;

namespace DeconTools.Backend.Workflows
{
    public abstract class WorkflowParameters
    {
   
        #region Constructors
        #endregion

        #region Properties
        public abstract string WorkflowType { get;  }

        public Globals.MassTagResultType ResultType { get; set; }
      

        #endregion

        #region Public Methods


        public abstract void LoadParameters(string xmlFilename);

        public virtual string ToStringWithDetails()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> parameterValues = GetParameterTable();

            foreach (var item in parameterValues)
            {
                sb.Append(item.Key);
                sb.Append("\t");
                sb.Append(item.Value);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();

        }

        public virtual void SaveParametersToXML(string xmlFilename)
        {

            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> parameterValues = GetParameterTable();

            XElement xml = new XElement("WorkflowParameters", 
                from param in parameterValues 
                select new XElement(param.Key, param.Value)
                    );

            xml.Save(xmlFilename);


        }

        public Dictionary<string, string> GetParameterTable()
        {

            Dictionary<string, string> parameterValues = new Dictionary<string, string>();


            Type t = this.GetType();

            foreach (System.Reflection.MemberInfo mi in t.GetMembers().OrderBy(p=>p.Name))
            {
                if (mi.MemberType == System.Reflection.MemberTypes.Property)
                {
                    System.Reflection.PropertyInfo pi = (PropertyInfo)mi;


                    string propertyName = pi.Name;
                    string propertyValue = pi.GetValue(this, null).ToString();

                    if (!parameterValues.ContainsKey(propertyName))
                    {
                        parameterValues.Add(propertyName, propertyValue);
                    }
                }
            }

            return parameterValues;


        }



        #endregion

        #region Private Methods

        #endregion

    }
}
